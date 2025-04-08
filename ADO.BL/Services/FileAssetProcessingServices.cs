using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ADO.BL.Services
{
    public class FileAssetProcessingServices : IFileAssetProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _AssetsDirectoryPath;
        private readonly IFileAssetModifiedDataAccess fileAssetModifiedDataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly string _connectionString;
        public FileAssetProcessingServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IFileAssetModifiedDataAccess _fileAssetModifiedDataAccess)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _AssetsDirectoryPath = configuration["FilesAssetsPath"];
            fileAssetModifiedDataAccess = _fileAssetModifiedDataAccess;
            statusFileDataAccess = _statuFileDataAccess;
        }

        public async Task<ResponseQuery<string>> UploadFile(FileAssetsValidationDTO request, ResponseQuery<string> response)
        {
            try
            {
                

                string inputFolder = _AssetsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();
                var assetListCreate = new List<AllAssetDTO>();
                var listDataStringUpdate = new StringBuilder();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv").Where(file => !file.EndsWith("_Error.csv")).ToList())
                {                    

                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    var beginDate = DateOnly.Parse($"1/{month}/{year}");
                    var endDate = beginDate.AddMonths(-2);
                    var listDates = new StringBuilder();
                    var listFilesError = new StringBuilder();
                    var lacQueueList = new List<LacQueueDTO>();

                    while (endDate <= beginDate)
                    {
                        listDates.Append($"'{endDate.Day}-{endDate.Month}-{endDate.Year}',");
                        endDate = endDate.AddMonths(1);
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDatesDef = listDates.ToString().Remove(listDates.Length - 1, 1);
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_asset where date_register in ({listDatesDef})";
                        using (var reader = new NpgsqlCommand(SelectQuery, connection))
                        {
                            try
                            {

                                using (var result = await reader.ExecuteReaderAsync())
                                {
                                    while (await result.ReadAsync())
                                    {
                                        var temp = new LacQueueDTO();
                                        temp.file_name = result[0].ToString();
                                        temp.year = int.Parse(result[1].ToString());
                                        temp.month = int.Parse(result[2].ToString());
                                        temp.day = int.Parse(result[3].ToString());
                                        temp.status = int.Parse(result[4].ToString());

                                        lacQueueList.Add(temp);
                                    }
                                }
                            }
                            catch (NpgsqlException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }

                    var flagValidation = false;
                    foreach (var item in lacQueueList)
                    {
                        if (item.status == 0 || item.status == 2 || item.status == 3)
                        {
                            flagValidation = true;
                            listFilesError.Append($"{item.file_name},");
                        }
                    }

                    if (flagValidation)
                    {
                        var listFilesErrorDef = listFilesError.ToString().Remove(listFilesError.Length - 1, 1);
                        response.Message = $"Los archivos {listFilesErrorDef} no han sido procesados correctamente, favor corregirlos";
                        response.SuccessData = false;
                        response.Success = false;
                        return response;
                    }

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName.Replace("_Correct","").Replace("_Update","");
                    statusFilesingle.FileType = "ASSETS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = -1;
                    statusFilesingle.Status = 4;
                    statusFilesingle.DateRegister = DateOnly.Parse($"1-{year}-{month}");

                    #region llenado de campos                                

                    if (fileName.Contains("_Update")) 
                    {
                        string[] fileLines = File.ReadAllLines(filePath);
                        foreach (var item in fileLines)
                        {
                            var valueLines = item.Split(',');
                            listDataStringUpdate.Append($"'{valueLines[0]}',");
                        }
                    }

                    if (fileName.Contains("_Correct")) 
                    {
                        string[] fileLines = File.ReadAllLines(filePath);
                        foreach (var item in fileLines)
                        {
                            var valueLines = item.Split(',');
                            var newEntity = new AllAssetDTO();

                            newEntity.Id = 0;
                            newEntity.TypeAsset = valueLines[0];
                            newEntity.CodeSig = valueLines[1];
                            newEntity.Uia = valueLines[2];
                            newEntity.Codetaxo = valueLines[3];
                            newEntity.Fparent = valueLines[4];
                            newEntity.Latitude = float.Parse(valueLines[5]);
                            newEntity.Longitude = float.Parse(valueLines[6]);
                            newEntity.Poblation = valueLines[7];
                            newEntity.Group015 = valueLines[8];
                            newEntity.Uccap14 = valueLines[9];
                            newEntity.DateInst = DateOnly.Parse(valueLines[10]);
                            newEntity.DateUnin = DateOnly.Parse(valueLines[11]);
                            newEntity.State = int.Parse(valueLines[12]);
                            newEntity.IdRegion = long.Parse(valueLines[13]);
                            newEntity.NameRegion = valueLines[14];
                            newEntity.Address = valueLines[15];
                            newEntity.Year = int.Parse(valueLines[16]);
                            newEntity.Month = int.Parse(valueLines[17]);

                            assetListCreate.Add(newEntity);
                        }
                    }
                    #endregion

                    var subgroupMap = mapper.Map<QueueStatusAsset>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataAsset(subgroupMap);

                }

                if (listDataStringUpdate.Length > 1)
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDefUpdate = listDataStringUpdate.ToString().Remove(listDataStringUpdate.Length - 1, 1);
                        var updateQuery = $@"update public.all_asset set state = 3 where code_sig in ({listDefUpdate})";
                        using (var reader = new NpgsqlCommand(updateQuery, connection))
                        {
                            try
                            {
                                await reader.ExecuteReaderAsync();
                            }
                            catch (NpgsqlException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                    }
                }

                if (assetListCreate.Count > 0)
                {
                    int i = 0;
                    while ((i * 10000) < assetListCreate.Count())
                    {
                        var subgroup = assetListCreate.Skip(i * 10000).Take(10000).ToList();
                        var EntityResult = mapper.Map<List<AllAsset>>(subgroup);
                        SaveData(EntityResult);
                        i++;
                        Console.WriteLine(i * 10000);
                    }

                }

                if (errorFlag)
                {
                    response.Message = "File with errors";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    response.Message = "All files created";
                    response.SuccessData = true;
                    response.Success = true;
                    return response;
                }

            }
            catch (FormatException ex)
            {
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
            }

            return response;
        } 

        private async Task SaveData(List<AllAsset> dataList)
        {
            await fileAssetModifiedDataAccess.SaveData(dataList);
        }        
    }
}
