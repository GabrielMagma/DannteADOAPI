﻿using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

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
        private readonly IHubContext<NotificationHub> _hubContext;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public FileAssetProcessingServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IFileAssetModifiedDataAccess _fileAssetModifiedDataAccess,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _AssetsDirectoryPath = configuration["FilesAssetsPath"];
            fileAssetModifiedDataAccess = _fileAssetModifiedDataAccess;
            statusFileDataAccess = _statuFileDataAccess;
            _hubContext = hubContext;
        }

        public async Task<ResponseQuery<bool>> ReadFilesAssets(FileAssetsValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                
                string inputFolder = _AssetsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();
                var assetListCreate = new List<AllAssetDTO>();
                var listDataStringUpdate = new StringBuilder();
                var updatesList = new List<AssetDTO>();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv").Where(file => !file.EndsWith("_Error.csv")).ToList().OrderBy(f => f).ToArray())
                {                    

                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = fileName.Replace("_Correct", "");
                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileNameTemp} se está Procesando");


                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    var beginDate = ParseDate($"1/{month}/{year}");
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
                    statusFilesingle.Day = 1;
                    statusFilesingle.Status = 4;
                    statusFilesingle.DateRegister = ParseDate($"1/{month}/{year}");

                    #region llenado de campos                                

                    if (fileName.Contains("_Update")) 
                    {
                        string[] fileLines = File.ReadAllLines(filePath);
                        foreach (var item in fileLines)
                        {
                            var valueLines = item.Split(',');
                            var updateAssetUnit = new AssetDTO()
                            {
                                Uia = valueLines[0],
                                DateInst = ParseDate(valueLines[1])
                            };
                            updatesList.Add(updateAssetUnit);                                
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
                            newEntity.DateInst = ParseDate(valueLines[10]);
                            newEntity.DateUnin = ParseDate(valueLines[11]);
                            newEntity.State = int.Parse(valueLines[12]);
                            newEntity.IdRegion = long.Parse(valueLines[13]);
                            newEntity.NameRegion = valueLines[14];
                            newEntity.Address = valueLines[15];
                            newEntity.Year = int.Parse(valueLines[16]);
                            newEntity.Month = int.Parse(valueLines[17]);
                            newEntity.IdZone = long.Parse(valueLines[18]);
                            newEntity.NameZone = valueLines[19];
                            newEntity.IdLocality = long.Parse(valueLines[20]);
                            newEntity.NameLocality = valueLines[21];
                            newEntity.IdSector = long.Parse(valueLines[22]);
                            newEntity.NameSector = valueLines[23];
                            newEntity.GeographicalCode = long.Parse(valueLines[24]);

                            assetListCreate.Add(newEntity);
                        }
                    }
                    #endregion

                    var subgroupMap = mapper.Map<QueueStatusAsset>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataAsset(subgroupMap);

                }

                if (updatesList.Count > 0)
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            var updateQuery = new StringBuilder();
                            updateQuery.Append("WITH update_values (uia, date_unin) AS (VALUES ");

                            for (int i = 0; i < updatesList.Count; i++)
                            {
                                if (i > 0) updateQuery.Append(",");
                                updateQuery.Append($"(@uia{i}, @dateUnin{i})");
                            }

                            updateQuery.Append(") ");
                            updateQuery.Append("UPDATE public.all_asset SET state = 3, date_unin = uv.date_unin ");
                            updateQuery.Append("FROM update_values uv ");
                            updateQuery.Append("WHERE public.all_asset.uia = uv.uia;");

                            var updateCommand = new NpgsqlCommand(updateQuery.ToString(), connection);

                            for (int i = 0; i < updatesList.Count; i++)
                            {                                
                                updateCommand.Parameters.AddWithValue($"@uia{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updatesList[i].Uia);                                
                                updateCommand.Parameters.AddWithValue($"@dateUnin{i}", NpgsqlTypes.NpgsqlDbType.Date, updatesList[i].DateInst ?? (object)DBNull.Value);
                            }

                            await updateCommand.ExecuteNonQueryAsync();
                            await transaction.CommitAsync();
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

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();                    
                    var UpdateRegionQuery = $@"UPDATE public.all_asset a
                                                SET 
                                                    id_region = r.id_region,
                                                    name_region = r.name_region
                                                FROM (
                                                    SELECT 
                                                        f.fparent::varchar AS fparent,
                                                        r.id AS id_region,
                                                        r.name_region
                                                    FROM maps.mp_fparent f
                                                    JOIN maps.mp_region r ON f.id_region = r.id
                                                ) r
                                                WHERE a.fparent = r.fparent;";
                    using (var reader = new NpgsqlCommand(UpdateRegionQuery, connection))
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

                if (errorFlag)
                {
                    response.Message = "Archivo con errores, favor corregirlos";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    response.Message = "Todos los registros creados satisfactoriamente.";
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

        private DateOnly ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", _spanishCulture);
        }

    }
}
