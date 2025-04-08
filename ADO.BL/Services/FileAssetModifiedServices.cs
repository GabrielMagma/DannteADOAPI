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
    public class FileAssetModifiedServices : IFileAssetModifiedServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _AssetsDirectoryPath;
        private readonly IFileAssetModifiedDataAccess fileAssetModifiedDataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly string _connectionString;
        public FileAssetModifiedServices(IConfiguration configuration,
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

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet1 = package.Workbook.Worksheets[0];
                        var dataTableError = new DataTable();
                        var assetList = new List<AllAssetDTO>();
                        var assetListCreate = new List<AllAssetDTO>();
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
                                var codeSigDoc = worksheet1.Cells[row, 2].Text.Trim();
                                if(codeSigDoc == "0")
                                {
                                    continue;
                                }
                                listDataString.Append($"'{codeSigDoc.Trim().Replace(" ", "")}',");
                                if (codeSigDoc[0] == '0')
                                {                                    
                                    listDataString.Append($"'{codeSigDoc.Trim().Replace(" ", "").Remove(0,1)}',");
                                }
                                else
                                {                                    
                                    listDataString.Append($"'0{codeSigDoc.Trim().Replace(" ", "")}',");
                                }

                            }

                        }

                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();
                            var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                            var SelectQuery = $@"SELECT id, code_sig, uia, fparent, date_inst, latitude, longitude, poblation, group015, year, month FROM public.all_asset where code_sig in ({listDef})";
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
                                            temp.CodeSig = result[1].ToString();
                                            temp.Uia = result[2].ToString();                                            
                                            temp.Fparent = result[3].ToString();
                                            if (!string.IsNullOrEmpty(result[4].ToString()))
                                            {
                                                temp.DateInst = DateOnly.FromDateTime(DateTime.Parse(result[4].ToString()));
                                            }
                                            temp.Latitude = float.Parse(result[5].ToString());
                                            temp.Longitude = float.Parse(result[6].ToString());
                                            temp.Poblation = result[7].ToString();
                                            temp.Group015 = result[8].ToString();
                                            temp.Year = int.Parse(result[9].ToString());
                                            temp.Month = int.Parse(result[10].ToString());

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
                            for (int i = 1; i <= 12; i++)
                            {
                                if (worksheet1.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 12)
                            {
                                break;
                            }

                            if (string.IsNullOrEmpty(worksheet1.Cells[row, 1].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 2].Text) ||
                                string.IsNullOrEmpty(worksheet1.Cells[row, 3].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 5].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 6].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 7].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 9].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 11].Text))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data en la línea {row}, hay uno o más campos vacíos y estos son Requeridos";
                                var rowText = new StringBuilder();
                                for (int i = 1; i < 13; i++)
                                {
                                    rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                }
                                newRowError[1] = rowText;
                                dataTableError.Rows.Add(newRowError);
                            }
                            else
                            {                               
                                #region llenado de campos                                

                                if (worksheet1.Cells[row, 2].Text == "0")
                                {
                                    continue;
                                }
                                
                                var codeSigDoc = worksheet1.Cells[row, 2].Text.Trim();
                                var assetTempList = assetList.Where(x => x.CodeSig == codeSigDoc).ToList();                                
                                var assetTemp = assetTempList.FirstOrDefault(x => x.State == 2);

                                if (assetTemp == null)
                                {                                    
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text};");
                                    }                                    
                                    await CreateAsset(rowText, assetListCreate, dataTableError, fparentRegionList, row, assetList, request);
                                    continue;
                                }

                                var existEntity2 = assetList.FirstOrDefault(x => x.CodeSig == codeSigDoc && x.Uia == worksheet1.Cells[row, 3].Text.Trim());
                                if (existEntity2 != null)
                                {
                                    continue;
                                }

                                var regionTemp = fparentRegionList.FirstOrDefault(x => x.fparent == worksheet1.Cells[row, 5].Text.Trim());

                                if (regionTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe la región para este circuito";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }
                                

                                if (assetTemp.Fparent != worksheet1.Cells[row, 5].Text.Trim())
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, el circuito es incorrecto, no pertenece a esta ubicación";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                if (assetTemp.Group015 != worksheet1.Cells[row, 9].Text.Trim())
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, el grupo de calidad es incorrecto";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }                                 

                                if (assetTemp.Latitude != float.Parse(worksheet1.Cells[row, 6].Text.Trim()))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, la latitud no corresponde para esta localización";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                if (assetTemp.Longitude != float.Parse(worksheet1.Cells[row, 7].Text.Trim()))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, la longitud no corresponde para esta localización";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                if (assetTemp.Poblation != worksheet1.Cells[row, 8].Text.Trim())
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, el tipo de población es incorrecto";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var date = ParseDate(worksheet1.Cells[row, 11].Text);
                                if (date == DateOnly.Parse("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, la fecha viene en un formato incorrecto";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var existEntity = assetListCreate.FirstOrDefault(x => x.CodeSig == codeSigDoc && x.Uia == worksheet1.Cells[row, 3].Text.Trim());                                
                                if (existEntity != null)
                                {
                                    continue;
                                }

                                var stateTemp = 2;
                                if(assetTemp.Year > request.Year)
                                {
                                    stateTemp = 3;
                                }

                                else if ((assetTemp.Year == request.Year) && (assetTemp.Month > request.Month))
                                {
                                    stateTemp = 3;
                                }

                                else
                                {                                    
                                    
                                    listDataStringUpdate.Append($"'{codeSigDoc}',");
                                    if (codeSigDoc[0] == '0')
                                    {                                            
                                        listDataStringUpdate.Append($"'{codeSigDoc.Remove(0,1)}',");
                                    }
                                    else
                                    {                                            
                                        listDataStringUpdate.Append($"'0{codeSigDoc}',");
                                    }
                                    
                                }                              
                                

                                var newEntity = new AllAssetDTO();

                                newEntity.Id = 0;
                                newEntity.TypeAsset = worksheet1.Cells[row, 1].Text.Trim().ToUpper();
                                newEntity.CodeSig = codeSigDoc;
                                newEntity.Uia = worksheet1.Cells[row, 3].Text.Trim();
                                newEntity.Codetaxo = string.IsNullOrEmpty(worksheet1.Cells[row, 4].Text) ? "-1" : worksheet1.Cells[row, 4].Text.Trim();
                                newEntity.Fparent = worksheet1.Cells[row, 5].Text.Trim().Replace(" ", "");
                                newEntity.Latitude = float.Parse(worksheet1.Cells[row, 6].Text.Trim());
                                newEntity.Longitude = float.Parse(worksheet1.Cells[row, 7].Text.Trim());
                                newEntity.Poblation = string.IsNullOrEmpty(worksheet1.Cells[row, 8].Text) ? "-1" : worksheet1.Cells[row, 8].Text.Trim();
                                newEntity.Group015 = worksheet1.Cells[row, 9].Text.Trim();
                                newEntity.DateInst = date;
                                newEntity.DateUnin = DateOnly.Parse("31/12/2099");                                
                                newEntity.State = stateTemp;
                                newEntity.Uccap14 = string.IsNullOrEmpty(worksheet1.Cells[row, 12].Text) ? "-1" : worksheet1.Cells[row, 12].Text.Trim();                                
                                newEntity.IdRegion = regionTemp.id_region;
                                newEntity.NameRegion = regionTemp.name_region;                                
                                newEntity.Address = string.IsNullOrEmpty(worksheet1.Cells[row, 10].Text) ? "-1" : worksheet1.Cells[row, 10].Text.Trim();
                                newEntity.Year = request.Year;
                                newEntity.Month = request.Month;

                                assetListCreate.Add(newEntity);
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

                            var subgroupMap = mapper.Map<List<QueueStatusAsset>>(statusFileList);
                            var resultSave = await statusFileDataAccess.SaveDataAssetList(subgroupMap);

                        }
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
            await fileAssetModifiedDataAccess.SaveData(dataList);
        }

        private DateOnly ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.Parse("31/12/2099");
        }

        private async Task CreateAsset(StringBuilder data, List<AllAssetDTO> assetListCreate, DataTable dataTableError, List<FparenRegionDTO> fparentRegionList, int row, List<AllAssetDTO> assetList, FileAssetsValidationDTO request)
        {
            var dataUnit = data.ToString().Split(';');

            var regionTemp = fparentRegionList.FirstOrDefault(x => x.fparent == dataUnit[4]);            

            if (regionTemp == null)
            {
                var newRowError = dataTableError.NewRow();
                newRowError[0] = $"Error en la data en la línea {row}, no existe la región para este circuito";
                var rowText = new StringBuilder();
                for (int i = 0; i < 12; i++)
                {
                    rowText.Append($"{dataUnit[i]}, ");
                }
                newRowError[1] = rowText;
                dataTableError.Rows.Add(newRowError);
                return;
            }

            var date = ParseDate(dataUnit[10]);

            if (date == DateOnly.Parse("31/12/2099"))
            {
                var newRowError = dataTableError.NewRow();
                newRowError[0] = $"Error en la data en la línea {row}, la fecha viene en un formato incorrecto";
                var rowText = new StringBuilder();
                for (int i = 0; i < 12; i++)
                {
                    rowText.Append($"{dataUnit[i]}, ");
                }
                newRowError[1] = rowText;
                dataTableError.Rows.Add(newRowError);
                return;
            }



            //var codeSigDoc = dataUnit[1].ToString()[0] == '0' ? dataUnit[1].ToString().Trim() : $"0{dataUnit[1].ToString().Trim()}";
            var codeSigDoc = dataUnit[1].ToString().Trim();

            var uiaTemp = dataUnit[1] == dataUnit[2] ? codeSigDoc : dataUnit[2];

            var existEntity = assetListCreate.FirstOrDefault(x => x.CodeSig == codeSigDoc && x.Uia == uiaTemp.Trim());
            var existEntity2 = assetList.FirstOrDefault(x => x.CodeSig == codeSigDoc && x.Uia == uiaTemp.Trim());
            if (existEntity != null || existEntity2 != null)
            {
                return;
            }
            else
            {

                var newEntity = new AllAssetDTO();

                newEntity.Id = 0;
                newEntity.TypeAsset = dataUnit[0].Trim().ToUpper();
                newEntity.CodeSig = codeSigDoc;
                newEntity.Uia = uiaTemp.Trim();
                newEntity.Codetaxo = string.IsNullOrEmpty(dataUnit[3]) ? "-1" : dataUnit[3].Trim();
                newEntity.Fparent = dataUnit[4].Trim().Replace(" ", "");
                newEntity.Latitude = float.Parse(dataUnit[5].Trim());
                newEntity.Longitude = float.Parse(dataUnit[6].Trim());
                newEntity.Poblation = string.IsNullOrEmpty(dataUnit[7]) ? "-1" : dataUnit[7].Trim();
                newEntity.Group015 = dataUnit[8].Trim();
                newEntity.DateInst = date;
                newEntity.DateUnin = DateOnly.Parse("31/12/2099");
                newEntity.State = 2;
                newEntity.Uccap14 = string.IsNullOrEmpty(dataUnit[11]) ? "-1" : dataUnit[11].Trim();                
                newEntity.IdRegion = regionTemp.id_region;
                newEntity.NameRegion = regionTemp.name_region;                
                newEntity.Address = string.IsNullOrEmpty(dataUnit[9]) ? "-1" : dataUnit[9].Trim();
                newEntity.Year = request.Year;
                newEntity.Month = request.Month;

                assetListCreate.Add(newEntity);
            }

        }
    }
}
