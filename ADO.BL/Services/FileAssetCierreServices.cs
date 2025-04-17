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
    public class FileAssetCierreServices : IFileAssetCierreServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _AssetsDirectoryPath;        
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly string _connectionString;
        public FileAssetCierreServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _AssetsDirectoryPath = configuration["FilesAssetsPath"];            
            statusFileDataAccess = _statuFileDataAccess;
        }

        public async Task<ResponseQuery<string>> UploadFile(FileAssetsValidationDTO request, ResponseQuery<string> response)
        {
            try
            {
                string inputFolder = _AssetsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx").OrderBy(f => f).ToArray())
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet1 = package.Workbook.Worksheets[1];
                        var dataTableError = new DataTable();
                        var assetList = new List<AllAssetDTO>();
                        var fparentRegionList = new List<FparenRegionDTO>();

                        var statusFilesingle = new StatusFileDTO();

                        // Extraer el nombre del archivo sin la extensión
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                        statusFilesingle.UserId = request.UserId;
                        statusFilesingle.FileName = fileName;
                        statusFilesingle.FileType = "ASSETS";
                        statusFilesingle.Year = request.Year;
                        statusFilesingle.Month = request.Month;
                        statusFilesingle.Day = -1;
                        statusFilesingle.Status = 1;

                        statusFileList.Add(statusFilesingle);


                        // columnas tabla error
                        dataTableError.Columns.Add("C1");
                        dataTableError.Columns.Add("C2");

                        var listDataString = new StringBuilder();
                        var listDataStringUpdate = new StringBuilder();

                        for (int row = 3; row <= worksheet1.Dimension.End.Row; row++)
                        {
                            if (worksheet1.Cells[row, 1].Text != "")
                            {
                                listDataString.Append($"'{worksheet1.Cells[row, 1].Text.Trim().Replace(" ", "")}',");

                            }

                        }

                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();
                            var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                            var SelectQuery = $@"SELECT * FROM public.all_asset where code_sig in ({listDef})";
                            using (var reader = new NpgsqlCommand(SelectQuery, connection))
                            {
                                try
                                {

                                    using (var result = await reader.ExecuteReaderAsync())
                                    {
                                        while (await result.ReadAsync())
                                        {
                                            //listUIA.Append($"'{result[0]}',");
                                            var temp = new AllAssetDTO();
                                            temp.Id = long.Parse(result[0].ToString());
                                            temp.TypeAsset = result[1].ToString();
                                            temp.CodeSig = result[2].ToString();
                                            temp.Uia = result[3].ToString();
                                            temp.Codetaxo = result[4].ToString();
                                            temp.Fparent = result[5].ToString();
                                            temp.Latitude = float.Parse(result[6].ToString());
                                            temp.Longitude = float.Parse(result[7].ToString());
                                            temp.Poblation = result[8].ToString();
                                            temp.Group015 = result[9].ToString();
                                            temp.Uccap14 = result[10].ToString();
                                            temp.DateInst = DateOnly.FromDateTime(DateTime.Parse(result[11].ToString()));
                                            temp.DateUnin = DateOnly.FromDateTime(DateTime.Parse(result[12].ToString()));
                                            temp.State = int.Parse(result[13].ToString());                                            
                                            temp.IdRegion = long.Parse(result[14].ToString());
                                            temp.NameRegion = result[15].ToString();                                            
                                            temp.Address = result[16].ToString();
                                            temp.Year = int.Parse(result[17].ToString());
                                            temp.Month = int.Parse(result[18].ToString());


                                            assetList.Add(temp);
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

                            var fparentQuery = $@"SELECT a.fparent, a.id_region, b.name_region FROM maps.mp_fparent as a
                                                inner join maps.mp_region as b
                                                on a.id_region = b.id";
                            using (var reader2 = new NpgsqlCommand(fparentQuery, connection))
                            {
                                try
                                {

                                    using (var result2 = await reader2.ExecuteReaderAsync())
                                    {
                                        while (await result2.ReadAsync())
                                        {
                                            //listUIA.Append($"'{result[0]}',");
                                            var fparentTemp = result2[0].ToString();
                                            var regiontemp = result2[1].ToString();
                                            var nametemp = result2[2].ToString();
                                            var temp = new FparenRegionDTO();
                                            temp.fparent = result2[0].ToString();
                                            temp.id_region = long.Parse(result2[1].ToString());
                                            temp.name_region = result2[2].ToString();

                                            fparentRegionList.Add(temp);
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

                        for (int row = 3; row <= worksheet1.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 18; i++)
                            {
                                if (worksheet1.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 17)
                            {
                                break;
                            }

                            if (string.IsNullOrEmpty(worksheet1.Cells[row, 1].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 2].Text) ||
                                string.IsNullOrEmpty(worksheet1.Cells[row, 3].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 13].Text))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data en la línea {row}, hay uno o más campos vacíos y estos son Requeridos";
                                var rowText = new StringBuilder();
                                for (int i = 1; i < 18; i++)
                                {
                                    rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                }
                                newRowError[1] = rowText;
                                dataTableError.Rows.Add(newRowError);
                            }
                            else
                            {

                                var newEntity = new AllAssetDTO();

                                #region llenado de campos                                

                                var assetTemp = assetList.FirstOrDefault(x => x.CodeSig == worksheet1.Cells[row, 1].Text.Trim());

                                if (assetTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe este asset";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 18; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var regionTemp = fparentRegionList.FirstOrDefault(x => x.fparent == worksheet1.Cells[row, 2].Text.Trim());

                                if (regionTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe la región para este circuito";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 18; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                if (assetTemp.CodeSig == worksheet1.Cells[row, 1].Text.Trim() && assetTemp.Uia == worksheet1.Cells[row, 13].Text.Trim())
                                {
                                    continue;
                                }

                                if (assetTemp.CodeSig == worksheet1.Cells[row, 1].Text.Trim() && assetTemp.Uia != worksheet1.Cells[row, 13].Text.Trim())
                                {
                                    listDataStringUpdate.Append($"'{assetTemp.Id}',");
                                }

                                if (assetTemp.Fparent != worksheet1.Cells[row, 2].Text.Trim())
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, el circuito es incorrecto, no pertenece a esta ubicación";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 18; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                if (assetTemp.Group015 != worksheet1.Cells[row, 3].Text.Trim())
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, el grupo de calidad es incorrecto";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 18; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                newEntity.Id = 0;
                                newEntity.TypeAsset = assetTemp.TypeAsset;
                                newEntity.CodeSig = worksheet1.Cells[row, 1].Text.Trim();
                                newEntity.Uia = worksheet1.Cells[row, 13].Text.Trim();
                                newEntity.Codetaxo = assetTemp.Codetaxo;
                                newEntity.Fparent = worksheet1.Cells[row, 2].Text.Trim();
                                newEntity.Latitude = assetTemp.Latitude;
                                newEntity.Longitude = assetTemp.Longitude;
                                newEntity.Poblation = assetTemp.Poblation;
                                newEntity.Group015 = assetTemp.Group015;
                                newEntity.DateInst = assetTemp.DateInst;
                                newEntity.DateUnin = assetTemp.DateUnin;
                                newEntity.State = assetTemp.State;
                                newEntity.Uccap14 = assetTemp.Uccap14;                                
                                newEntity.IdRegion = regionTemp.id_region;
                                newEntity.NameRegion = regionTemp.name_region;
                                newEntity.Address = assetTemp.Address;
                                newEntity.Year = assetTemp.Year;
                                newEntity.Month = assetTemp.Month;

                                assetList.Add(newEntity);
                                #endregion

                            }
                        }

                        if (dataTableError.Rows.Count > 0)
                        {
                            errorFlag = true;
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        if (listDataStringUpdate.Length > 1)
                        {
                            using (var connection = new NpgsqlConnection(_connectionString))
                            {
                                connection.Open();
                                var listDefUpdate = listDataStringUpdate.ToString().Remove(listDataStringUpdate.Length - 1, 1);
                                var updateQuery = $@"update public.all_asset set state = 3 where id in ({listDefUpdate})";
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

                        if (assetList.Count > 0)
                        {
                            int i = 0;
                            while ((i * 10000) < assetList.Count())
                            {
                                var subgroup = assetList.Skip(i * 10000).Take(10000).ToList();
                                var EntityResult = mapper.Map<List<AllAsset>>(subgroup);
                                SaveData(EntityResult);
                                i++;
                                Console.WriteLine(i * 10000);
                            }

                            //var subgroupMap = mapper.Map<List<StatusFile>>(statusFileList);
                            //var resultSave = await statusFileDataAccess.SaveDataList(subgroupMap);

                        }
                    }

                }

                if (errorFlag)
                {
                    response.Message = "file with errors";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    response.Message = "All files are created";
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

        private static void RegisterError(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Error.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    csv.WriteField(row[0]);
                    csv.WriteField(row[1]);
                    csv.NextRecord();
                }
            }
        }        

        private async Task SaveData(List<AllAsset> dataList)
        {
            
        }        
    }
}
