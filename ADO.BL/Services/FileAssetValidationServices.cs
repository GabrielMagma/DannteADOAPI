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
    public class FileAssetValidationServices : IFileAssetValidationServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _AssetsDirectoryPath;
        private readonly IFileAssetModifiedDataAccess fileAssetModifiedDataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly string _connectionString;
        public FileAssetValidationServices(IConfiguration configuration,
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
                        var dataTable = new DataTable();
                        var dataTableUpdate = new DataTable();
                        var assetList = new List<AllAssetDTO>();
                        var assetListCreate = new List<AllAssetDTO>();
                        var mpGeolocalityList = new List<MpGeolocalityDTO>();
                        var mpRegionList = new List<MpRegionDTO>();
                        var mpZoneList = new List<MpZoneDTO>();
                        var mpLocalityList = new List<MpLocalityDTO>();

                        var statusFilesingle = new StatusFileDTO();

                        // Extraer el nombre del archivo sin la extensión
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

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
                        statusFilesingle.DateRegister = DateOnly.Parse($"1-{year}-{month}");

                        // columnas tablas
                        dataTableError.Columns.Add("C1");
                        dataTableError.Columns.Add("C2");

                        for (int i = 1; i <= 25; i++)
                        {
                            dataTable.Columns.Add($"C{i}");
                        }

                        dataTableUpdate.Columns.Add("C1");

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
                        //var test = await fileAssetModifiedDataAccess.SearchData(listDef);

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
                            
                            var geolocalityQuery = $@"SELECT code_sig, codigo_geografico, zona, region, localidad FROM maps.mp_geolocality where code_sig in ({listDef})";
                            using (var reader2 = new NpgsqlCommand(geolocalityQuery, connection))
                            {
                                try
                                {

                                    using (var result2 = await reader2.ExecuteReaderAsync())
                                    {
                                        while (await result2.ReadAsync())
                                        {                                            
                                            var temp = new MpGeolocalityDTO();
                                            temp.code_sig = result2[0].ToString();
                                            temp.codigo_geografico = result2[1].ToString();
                                            temp.zona = result2[2].ToString();
                                            temp.region = result2[3].ToString();
                                            temp.localidad = result2[4].ToString();

                                            mpGeolocalityList.Add(temp);
                                            
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

                            var localityQuery = $@"SELECT id, name FROM maps.mp_locality";
                            using (var reader3 = new NpgsqlCommand(localityQuery, connection))
                            {
                                try
                                {

                                    using (var result3 = await reader3.ExecuteReaderAsync())
                                    {
                                        while (await result3.ReadAsync())
                                        {
                                            var temp = new MpLocalityDTO();                                            
                                            temp.id = long.Parse(result3[0].ToString());
                                            temp.name = result3[1].ToString();

                                            mpLocalityList.Add(temp);                                            
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

                            var regionQuery = $@"SELECT id, name_region FROM maps.mp_region";
                            using (var reader4 = new NpgsqlCommand(regionQuery, connection))
                            {
                                try
                                {

                                    using (var result4 = await reader4.ExecuteReaderAsync())
                                    {
                                        while (await result4.ReadAsync())
                                        {
                                            var temp = new MpRegionDTO();
                                            temp.id = long.Parse(result4[0].ToString());
                                            temp.name_region = result4[1].ToString();

                                            mpRegionList.Add(temp);                                            
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

                            var zoneQuery = $@"SELECT id, name FROM maps.mp_zone";
                            using (var reader5 = new NpgsqlCommand(zoneQuery, connection))
                            {
                                try
                                {

                                    using (var result5 = await reader5.ExecuteReaderAsync())
                                    {
                                        while (await result5.ReadAsync())
                                        {
                                            var temp = new MpZoneDTO();
                                            temp.id = long.Parse(result5[0].ToString());
                                            temp.name = result5[1].ToString();

                                            mpZoneList.Add(temp);
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

                                if (assetTemp == null)
                                {                                    
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text};");
                                    }                                    
                                    await CreateAsset(rowText, assetListCreate, dataTableError, mpGeolocalityList, mpRegionList, mpZoneList, mpLocalityList, row, assetList, year, month, dataTable);
                                    continue;
                                }

                                var existEntity2 = assetList.FirstOrDefault(x => x.CodeSig == codeSigDoc && x.Uia == worksheet1.Cells[row, 3].Text.Trim());
                                if (existEntity2 != null)
                                {
                                    continue;
                                }

                                var geolocalityTemp = mpGeolocalityList.FirstOrDefault(x => x.code_sig == codeSigDoc);

                                if (geolocalityTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe la región para este Registro";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var localityTemp = mpLocalityList.FirstOrDefault(x => x.name.ToUpper() == geolocalityTemp.localidad.ToUpper());

                                if (localityTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe la Localidad para este Registro";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var regionTemp = mpRegionList.FirstOrDefault(x => x.name_region.ToUpper() == geolocalityTemp.region.ToUpper());

                                if (regionTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe la Región para este Registro";
                                    var rowText = new StringBuilder();
                                    for (int i = 1; i < 13; i++)
                                    {
                                        rowText.Append($"{worksheet1.Cells[row, i].Text}, ");
                                    }
                                    newRowError[1] = rowText;
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var zoneTemp = mpZoneList.FirstOrDefault(x => x.name.ToUpper() == geolocalityTemp.zona.ToUpper());

                                if (zoneTemp == null)
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row}, no existe la Zona para este Registro";
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
                                    newRowUpdate[0] = $"'{codeSigDoc}'";
                                    dataTableUpdate.Rows.Add(newRowUpdate);
                                    
                                    if (codeSigDoc[0] == '0')
                                    {                                                                                    
                                        newRowUpdate[0] = $"'{codeSigDoc.Remove(0, 1)}'";
                                        dataTableUpdate.Rows.Add(newRowUpdate);
                                    }
                                    else
                                    {                                                                                    
                                        newRowUpdate[0] = $"'0{codeSigDoc}'";
                                        dataTableUpdate.Rows.Add(newRowUpdate);
                                    }
                                    
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
                                newRow[11] = DateOnly.Parse("31/12/2099");
                                newRow[12] = stateTemp;
                                newRow[13] = regionTemp == null ? '0' : regionTemp.id;
                                newRow[14] = geolocalityTemp.region;
                                newRow[15] = string.IsNullOrEmpty(worksheet1.Cells[row, 10].Text) ? "-1" : worksheet1.Cells[row, 10].Text.Trim();
                                newRow[16] = year;
                                newRow[17] = month;
                                newRow[18] = zoneTemp == null ? '0' : zoneTemp.id;
                                newRow[19] = geolocalityTemp.zona;
                                newRow[20] = localityTemp == null ? '0' : localityTemp.id;
                                newRow[21] = geolocalityTemp.localidad;
                                newRow[22] = 0;
                                newRow[23] = "SIN SECTOR";
                                newRow[24] = geolocalityTemp.codigo_geografico;

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
                    csv.NextRecord();
                }
            }
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

        private async Task CreateAsset(StringBuilder data, 
            List<AllAssetDTO> assetListCreate, 
            DataTable dataTableError, 
            List<MpGeolocalityDTO> mpGeolocalityList, 
            List<MpRegionDTO> mpRegionList,
            List<MpZoneDTO> mpZoneList,
            List<MpLocalityDTO> mpLocalityList,
            int row, 
            List<AllAssetDTO> assetList, 
            int year, 
            int month, 
            DataTable dataTable)
        {

            var dataUnit = data.ToString().Split(';');

            var codeSigDoc = dataUnit[1].ToString().Trim();

            var geolocalityTemp = mpGeolocalityList.FirstOrDefault(x => x.code_sig == codeSigDoc);

            if (geolocalityTemp == null)
            {
                var newRowError = dataTableError.NewRow();
                newRowError[0] = $"Error en la data en la línea {row}, no existe la región para este Registro";
                var rowText = new StringBuilder();
                for (int i = 0; i < 12; i++)
                {
                    rowText.Append($"{dataUnit[i]}, ");
                }
                newRowError[1] = rowText;
                dataTableError.Rows.Add(newRowError);
                return;
            }

            var localityTemp = mpLocalityList.FirstOrDefault(x => x.name.ToUpper() == geolocalityTemp.localidad.ToUpper());

            if (localityTemp == null)
            {
                var newRowError = dataTableError.NewRow();
                newRowError[0] = $"Error en la data en la línea {row}, no existe la Localidad para este Registro";
                var rowText = new StringBuilder();
                for (int i = 0; i < 12; i++)
                {
                    rowText.Append($"{dataUnit[i]}, ");
                }
                newRowError[1] = rowText;
                dataTableError.Rows.Add(newRowError);
                return;
            }

            var regionTemp = mpRegionList.FirstOrDefault(x => x.name_region.ToUpper() == geolocalityTemp.region.ToUpper());

            if (regionTemp == null)
            {
                var newRowError = dataTableError.NewRow();
                newRowError[0] = $"Error en la data en la línea {row}, no existe la Región para este Registro";
                var rowText = new StringBuilder();
                for (int i = 0; i < 12; i++)
                {
                    rowText.Append($"{dataUnit[i]}, ");
                }
                newRowError[1] = rowText;
                dataTableError.Rows.Add(newRowError);
                return;
            }

            var zoneTemp = mpZoneList.FirstOrDefault(x => x.name.ToUpper() == geolocalityTemp.zona.ToUpper());

            if (zoneTemp == null)
            {
                var newRowError = dataTableError.NewRow();
                newRowError[0] = $"Error en la data en la línea {row}, no existe la Zona para este Registro";
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
                newRow[11] = DateOnly.Parse("31/12/2099");
                newRow[12] = 2;
                newRow[13] = regionTemp == null ? '0' : regionTemp.id;
                newRow[14] = geolocalityTemp.region;
                newRow[15] = string.IsNullOrEmpty(dataUnit[9]) ? "-1" : dataUnit[9].Trim();
                newRow[16] = year;
                newRow[17] = month;
                newRow[18] = zoneTemp == null ? '0' : zoneTemp.id;
                newRow[19] = geolocalityTemp.zona;
                newRow[20] = localityTemp == null ? '0' : localityTemp.id;
                newRow[21] = geolocalityTemp.localidad;
                newRow[22] = 0;
                newRow[23] = "SIN SECTOR";
                newRow[24] = geolocalityTemp.codigo_geografico;

                dataTable.Rows.Add(newRow);

            }

        }
    }
}
