using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ADO.BL.Services
{
    public class FileAssetValidationServices : IFileAssetValidationServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _AssetsDirectoryPath;
        private readonly IFileAssetModifiedDataAccess fileAssetModifiedDataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        readonly IAllAssetOracleServices allAssetOracleServices;
        private readonly string _connectionString;
        private readonly IHubContext<NotificationHub> _hubContext;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public FileAssetValidationServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IFileAssetModifiedDataAccess _fileAssetModifiedDataAccess,
            IAllAssetOracleServices _AllAssetOracleServices,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _AssetsDirectoryPath = configuration["FilesAssetsPath"];
            fileAssetModifiedDataAccess = _fileAssetModifiedDataAccess;
            statusFileDataAccess = _statuFileDataAccess;
            allAssetOracleServices = _AllAssetOracleServices;
            _hubContext = hubContext;
        }

        public async Task<ResponseQuery<bool>> ReadFilesAssets(FileAssetsValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _AssetsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();

                // comentar si es para Essa
                //ResponseEntity<List<AllAssetDTO>> responseOracle = new ResponseEntity<List<AllAssetDTO>>();
                //await allAssetOracleServices.SearchData(responseOracle);

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx").OrderBy(f => f).ToArray())
                {
                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);                    
                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} se está validando");

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet1 = package.Workbook.Worksheets[0];
                        var dataTableError = new DataTable();
                        var dataTable = new DataTable();
                        var dataTableUpdate = new DataTable();
                        var assetList = new List<AllAssetDTO>();
                        var assetListCreate = new List<AllAssetDTO>();
                        var fparentRegionList = new List<FparenRegionDTO>();
                        var statusFilesingle = new StatusFileDTO();                        

                        // Obtener los primeros 4 dígitos como el año
                        int year = int.Parse(fileName.Substring(0, 4));

                        // Obtener los siguientes 2 dígitos como el mes
                        int month = int.Parse(fileName.Substring(4, 2));

                        statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                        statusFilesingle.UserId = request.UserId;
                        statusFilesingle.FileName = fileName;
                        statusFilesingle.FileType = "ASSETS";
                        statusFilesingle.Year = year;
                        statusFilesingle.Month = month;
                        statusFilesingle.Day = 1;
                        statusFilesingle.Status = 1;
                        statusFilesingle.DateRegister = ParseDate($"1/{month}/{year}");                         

                        // columnas tablas
                        dataTableError.Columns.Add("C1");
                        dataTableError.Columns.Add("C2");

                        for (int i = 1; i <= 25; i++)
                        {
                            dataTable.Columns.Add($"C{i}");
                        }

                        dataTableUpdate.Columns.Add("C1");
                        dataTableUpdate.Columns.Add("C2");

                        var listDataString = new StringBuilder();
                        //var listDataString = new List<string>();
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
                                //listDataString.Add($"'{codeSigDoc.Trim().Replace(" ", "")}'");                                    
                                if (codeSigDoc[0] == '0')
                                {
                                    listDataString.Append($"'{codeSigDoc.Trim().Replace(" ", "").Remove(0, 1)}',");
                                    //listDataString.Add($"'{codeSigDoc.Trim().Replace(" ", "").Remove(0, 1)}'");
                                }
                                else
                                {
                                    listDataString.Append($"'0{codeSigDoc.Trim().Replace(" ", "")}',");
                                    //listDataString.Add($"'0{codeSigDoc.Trim().Replace(" ", "")}'");
                                }

                            }

                        }                        

                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();

                            #region temporal
                            //var createTempTableCommand = new NpgsqlCommand(
                            //"CREATE TEMP TABLE temp_asset_codes (code_sig VARCHAR)", connection);
                            //await createTempTableCommand.ExecuteNonQueryAsync();

                            //// Paso 2: Carga masiva de datos usando COPY (el método más rápido)                            
                            //using (var writer = connection.BeginBinaryImport(
                            //    @"COPY temp_asset_codes (code_sig) FROM STDIN (FORMAT binary)"))
                            //{
                            //    foreach (var code in listDataString)
                            //    {
                            //        if (code != "") {
                            //            writer.StartRow();
                            //            writer.Write(code, NpgsqlTypes.NpgsqlDbType.Varchar);
                            //        }
                            //    }

                            //}

                            #endregion

                            #region tempAnterior
                            var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                            var SelectQuery = $@"SELECT id, code_sig, uia, fparent, date_inst, latitude, longitude, poblation, group015, year, month,
                                                 id_locality, name_locality, id_zone, name_zone, geographical_code, id_sector, name_sector
                                                 FROM public.all_asset where code_sig in ({listDef})";
                            using (var reader = new NpgsqlCommand(SelectQuery, connection))
                            {
                                try
                                {

                                    using (var result = await reader.ExecuteReaderAsync())
                                    {
                                        while (await result.ReadAsync())
                                        {
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
                                            temp.IdLocality = long.Parse(result[11].ToString());
                                            temp.NameLocality = result[12].ToString();
                                            temp.IdZone = long.Parse(result[13].ToString());
                                            temp.NameZone = result[14].ToString();
                                            temp.GeographicalCode = long.Parse(result[15].ToString());
                                            temp.IdSector = long.Parse(result[16].ToString());
                                            temp.NameSector = result[17].ToString();

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
                            #endregion

                            #region guardadoTemp
                            //var selectQuery = @"
                            //        SELECT a.id, a.code_sig, a.uia, a.fparent, a.date_inst, 
                            //               a.latitude, a.longitude, a.poblation, a.group015, 
                            //               a.year, a.month, a.id_locality, a.name_locality, 
                            //               a.id_zone, a.name_zone, a.geographical_code, 
                            //               a.id_sector, a.name_sector
                            //        FROM public.all_asset a
                            //        JOIN temp_asset_codes t ON a.code_sig = t.code_sig";

                            //await using (var command = new NpgsqlCommand(selectQuery, connection))
                            //await using (var reader = await command.ExecuteReaderAsync())
                            //{
                            //    while (await reader.ReadAsync())
                            //    {
                            //        var temp = new AllAssetDTO
                            //        {
                            //            Id = reader.GetInt64(0),
                            //            CodeSig = reader.GetString(1),
                            //            Uia = reader.GetString(2),
                            //            Fparent = reader.GetString(3),
                            //            DateInst = reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),
                            //            Latitude = reader.GetFloat(5),
                            //            Longitude = reader.GetFloat(6),
                            //            Poblation = reader.GetString(7),
                            //            Group015 = reader.GetString(8),
                            //            Year = reader.GetInt32(9),
                            //            Month = reader.GetInt32(10),
                            //            IdLocality = reader.GetInt64(11),
                            //            NameLocality = reader.GetString(12),
                            //            IdZone = reader.GetInt64(13),
                            //            NameZone = reader.GetString(14),
                            //            GeographicalCode = reader.GetInt64(15),
                            //            IdSector = reader.GetInt64(16),
                            //            NameSector = reader.GetString(17)
                            //        };
                            //        assetList.Add(temp);
                            //    }
                            //}
                            #endregion

                            var fparentQuery = $@"SELECT a.fparent, a.id_region, b.name_region FROM maps.mp_fparent as a
                                                inner join maps.mp_region as b
                                                on a.id_region = b.id";
                            using (var reader6 = new NpgsqlCommand(fparentQuery, connection))
                            {
                                try
                                {

                                    using (var result6 = await reader6.ExecuteReaderAsync())
                                    {
                                        while (await result6.ReadAsync())
                                        {
                                            var temp = new FparenRegionDTO();
                                            temp.fparent = result6[0].ToString();
                                            temp.id_region = long.Parse(result6[1].ToString());
                                            temp.name_region = result6[2].ToString();

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
                                #region validación y filtrado

                                if (worksheet1.Cells[row, 2].Text == "0")
                                {
                                    continue;
                                }
                                
                                var codeSigDoc = worksheet1.Cells[row, 2].Text.Trim();
                                var assetTempList = assetList.Where(x => x.CodeSig == codeSigDoc).ToList();                                
                                var assetTemp = assetTempList.FirstOrDefault(x => x.State == 2);
                                var uiaTemp = string.Empty;
                                if (assetTemp != null)
                                {
                                    uiaTemp = assetTemp.Uia;
                                }
                                var regionTempFparent = fparentRegionList.FirstOrDefault(x => x.fparent == worksheet1.Cells[row, 5].Text.Trim());
                                if (assetTemp == null)
                                {                                    
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text};");
                                    }                                    
                                    await CreateAsset(rowText, assetListCreate, dataTableError, row, assetList, year, month, dataTable, regionTempFparent);
                                    continue;
                                }

                                var existEntity2 = assetList.FirstOrDefault(x => x.CodeSig == codeSigDoc && x.Uia == worksheet1.Cells[row, 3].Text.Trim());
                                if (existEntity2 != null)
                                {
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

                                var latTemp = Math.Round(Decimal.Parse(worksheet1.Cells[row, 6].Text.Trim()), 5);

                                var assetTempLat = Math.Round(Decimal.Parse(assetTemp.Latitude.ToString()), 5);

                                if (assetTempLat != latTemp)
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

                                var longTemp = Math.Round(Decimal.Parse(worksheet1.Cells[row, 7].Text.Trim()), 5);

                                var assetTempLong = Math.Round(Decimal.Parse(assetTemp.Longitude.ToString()), 5);

                                if (assetTempLong != longTemp)
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

                                var date = ParseDate($"{worksheet1.Cells[row, 11].Text}");
                                var date2 = DateOnly.ParseExact($"31/12/2099", "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                if (date == date2)
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

                                

                                #endregion

                                var stateTemp = 2;
                                if(assetTemp.Year > year)
                                {
                                    stateTemp = 3;
                                }

                                else if ((assetTemp.Year == year) && (assetTemp.Month > month))
                                {
                                    stateTemp = 3;
                                }

                                else
                                {
                                    var newRowUpdate = dataTableUpdate.NewRow();
                                    newRowUpdate[0] = $"{uiaTemp}";
                                    newRowUpdate[1] = $"{date}";
                                    dataTableUpdate.Rows.Add(newRowUpdate);                                                                        
                                    
                                }

                                #region llenado de campos                                                                

                                var newRow = dataTable.NewRow();                                

                                newRow[0] = worksheet1.Cells[row, 1].Text.Trim().ToUpper();
                                newRow[1] = codeSigDoc;
                                newRow[2] = worksheet1.Cells[row, 3].Text.Trim();
                                newRow[3] = string.IsNullOrEmpty(worksheet1.Cells[row, 4].Text) ? "-1" : worksheet1.Cells[row, 4].Text.Trim();
                                newRow[4] = worksheet1.Cells[row, 5].Text.Trim().Replace(" ", "");
                                newRow[5] = float.Parse(worksheet1.Cells[row, 6].Text.Trim());
                                newRow[6] = float.Parse(worksheet1.Cells[row, 7].Text.Trim());
                                newRow[7] = string.IsNullOrEmpty(worksheet1.Cells[row, 8].Text) ? "-1" : worksheet1.Cells[row, 8].Text.Trim();
                                newRow[8] = worksheet1.Cells[row, 9].Text.Trim();
                                newRow[9] = string.IsNullOrEmpty(worksheet1.Cells[row, 12].Text) ? "-1" : worksheet1.Cells[row, 12].Text.Trim();
                                newRow[10] = date;
                                newRow[11] = ParseDate("31/12/2099");
                                newRow[12] = stateTemp;
                                newRow[13] = regionTempFparent == null ? '0' : regionTempFparent.id_region;
                                newRow[14] = regionTempFparent == null ? "SIN REGION" : regionTempFparent.name_region;
                                newRow[15] = string.IsNullOrEmpty(worksheet1.Cells[row, 10].Text) ? "-1" : worksheet1.Cells[row, 10].Text.Trim();
                                newRow[16] = year;
                                newRow[17] = month;
                                newRow[18] = assetTemp.IdZone == null ? '0' : assetTemp.IdZone;
                                newRow[19] = assetTemp.NameZone == null ? "SIN ZONA" : assetTemp.NameZone;
                                newRow[20] = assetTemp.IdLocality == null ? '0' : assetTemp.IdLocality;
                                newRow[21] = assetTemp.NameLocality == null ? "SIN LOCALIDAD" : assetTemp.NameLocality;
                                newRow[22] = assetTemp.IdSector == null ? '0' : assetTemp.IdSector;
                                newRow[23] = assetTemp.NameSector == null ? "SIN SECTOR" : assetTemp.NameSector;
                                newRow[24] = assetTemp.GeographicalCode == null ? "0" : assetTemp.GeographicalCode;

                                dataTable.Rows.Add(newRow);

                                var newEntity = new AllAssetDTO();
                                
                                newEntity.CodeSig = codeSigDoc;
                                newEntity.Uia = worksheet1.Cells[row, 3].Text.Trim();

                                assetListCreate.Add(newEntity);

                                #endregion

                            }
                        }

                        if (dataTableError.Rows.Count > 0)
                        {
                            errorFlag = true;
                            statusFilesingle.Status = 2;
                            RegisterError(dataTableError, inputFolder, filePath);
                            await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores, por favor corregirlo");
                        }

                        var subgroupMap = mapper.Map<QueueStatusAsset>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataAsset(subgroupMap);

                        if (dataTable.Rows.Count > 0)
                        {
                            RegisterAssets(dataTable, inputFolder, filePath);
                        }

                        if (dataTableUpdate.Rows.Count > 0)
                        {
                            RegisterUpdate(dataTableUpdate, inputFolder, filePath);
                        }
                    }

                }

                if (errorFlag)
                {
                    response.Message = "Archivo con errores";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    response.Message = "todos los archivos validados correctamente";
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

        private static void RegisterAssets(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        csv.WriteField(row[i]);
                    }                                        
                    csv.NextRecord();
                }
            }
        }

        private static void RegisterUpdate(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Update.csv");
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

        private async Task CreateAsset(StringBuilder data, 
            List<AllAssetDTO> assetListCreate, 
            DataTable dataTableError,            
            int row, 
            List<AllAssetDTO> assetList, 
            int year, 
            int month, 
            DataTable dataTable,
            FparenRegionDTO regionTempFparent)
        {

            var dataUnit = data.ToString().Split(';');

            var codeSigDoc = dataUnit[1].ToString().Trim();            

            var date = ParseDate($"{dataUnit[10]}");
            
            if (date == DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", CultureInfo.InvariantCulture))
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
                
                newEntity.CodeSig = codeSigDoc;
                newEntity.Uia = uiaTemp.Trim();

                assetListCreate.Add(newEntity);

                var newRow = dataTable.NewRow();

                newRow[0] = dataUnit[0].Trim().ToUpper();
                newRow[1] = codeSigDoc;
                newRow[2] = uiaTemp.Trim();
                newRow[3] = string.IsNullOrEmpty(dataUnit[3]) ? "-1" : dataUnit[3].Trim();
                newRow[4] = dataUnit[4].Trim().Replace(" ", "");
                newRow[5] = float.Parse(dataUnit[5].Trim());
                newRow[6] = float.Parse(dataUnit[6].Trim());
                newRow[7] = string.IsNullOrEmpty(dataUnit[7]) ? "-1" : dataUnit[7].Trim();
                newRow[8] = dataUnit[8].Trim();
                newRow[9] = string.IsNullOrEmpty(dataUnit[11]) ? "-1" : dataUnit[11].Trim();
                newRow[10] = date;
                newRow[11] = ParseDate("31/12/2099");
                newRow[12] = 2;
                newRow[13] = regionTempFparent == null ? '0' : regionTempFparent.id_region;
                newRow[14] = regionTempFparent == null ? "SIN REGION" : regionTempFparent.name_region;
                newRow[15] = string.IsNullOrEmpty(dataUnit[9]) ? "-1" : dataUnit[9].Trim();
                newRow[16] = year;
                newRow[17] = month;
                newRow[18] = '0';
                newRow[19] = "SIN ZONA";
                newRow[20] = '0';
                newRow[21] = "SIN LOCALIDAD";
                newRow[22] = 0;
                newRow[23] = "SIN SECTOR";
                newRow[24] = "0";

                dataTable.Rows.Add(newRow);

            }

        }
    }
}
