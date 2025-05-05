using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class TT2ValidationServices : ITT2ValidationServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _TT2DirectoryPath;
        private readonly IMapper mapper;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly string _connectionString;

        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public TT2ValidationServices(IConfiguration configuration,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _TT2DirectoryPath = configuration["TT2DirectoryPath"];
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseEntity<List<StatusFileDTO>>> ValidationTT2(TT2ValidationDTO request, ResponseEntity<List<StatusFileDTO>> response)
        {
            try
            {
                var columns = int.Parse(_configuration["Validations:TT2Columns"]);
                string inputFolder = _TT2DirectoryPath;
                var errorFlag = false;
                int uia = 0;
                int grupoCalidad = 2;
                int latitud = 8;
                int longitud = 7;
                int fecha = 11;
                if (request.Encabezado == false)
                {
                    uia = request.columns.UIA - 1;
                    grupoCalidad = request.columns.GrupoCalidad - 1;
                    latitud = request.columns.Latitud - 1;
                    longitud = request.columns.Longitud - 1;
                    fecha = request.columns.Fecha - 1;
                }
                var statusFileList = new List<StatusFileDTO>();
                foreach (var filePath in Directory.GetFiles(inputFolder, "*_Fixed.csv")
                    .Where(file => !file.EndsWith("_Correct.csv") 
                    && !file.EndsWith("_Error.csv")
                    && !file.EndsWith("_completed.csv")
                    && !file.EndsWith("_insert.csv") 
                    && !file.EndsWith("_check.csv")
                    && !file.EndsWith("_create.csv")
                    && !file.EndsWith("_errorCodeSig.csv")
                    && !file.EndsWith("_update.csv"))
                    .ToList().OrderBy(f => f)
                     .ToArray()
                    )
                {

                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    int count = 1;                                                            
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    #region queue

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName.Replace("_Fixed", "");
                    statusFilesingle.FileType = "TT2";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;
                    statusFilesingle.DateRegister = ParseDateTemp($"1/{month}/{year}");

                    #endregion

                    // columnas tabla datos correctos
                    for (int i = 1; i <= columns; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }

                    string[] fileLines = File.ReadAllLines(filePath);
                    var assetList = new List<AllAssetDTO>();
                    var listDataString = new StringBuilder();

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
                        if (valueLines[0] != "COD_CREG")
                        {
                            listDataString.Append($"'{valueLines[1].Trim().Replace(" ", "")}',");
                        }
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                        var SelectQuery = $@"SELECT code_sig, latitude, longitude FROM public.all_asset where code_sig in ({listDef})";
                        using (var reader = new NpgsqlCommand(SelectQuery, connection))
                        {
                            try
                            {

                                using (var result = await reader.ExecuteReaderAsync())
                                {
                                    while (await result.ReadAsync())
                                    {
                                        var temp = new AllAssetDTO();                                        
                                        temp.CodeSig = result[0].ToString();                                        
                                        temp.Latitude = float.Parse(result[1].ToString());
                                        temp.Longitude = float.Parse(result[2].ToString());

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
                    }                    

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
                        if (valueLines[0] != "COD_CREG")
                        {
                            string message = string.Empty;
                            var beacon = 0;
                            for (int i = 0; i < columns; i++)
                            {
                                if (valueLines[i] != "")
                                {
                                    beacon++;
                                }
                            }

                            if (beacon > 0)
                            {                                

                                if (valueLines.Length != columns)
                                {
                                    message = "Error de cantidad de columnas llenas";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                if (valueLines[uia] == "")
                                {
                                    message = $"Error de la data de UIA de la fila {count}, no está llena correctamente, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                if (valueLines[1] == "")
                                {
                                    message = $"Error de la data de Code_Sig de la fila {count}, no está llena correctamente, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                if (valueLines[grupoCalidad].Length != 2)
                                {
                                    message = $"Error de la data de grupo calidad de la fila {count}, debe ser sólamente de dos dígitos, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                if (valueLines[latitud] == "" || valueLines[longitud] == "")
                                {
                                    message = $"Error de la data de Latitud y/o Longitud de la fila {count}, no está llena correctamente, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                if (valueLines[fecha] == "")
                                {
                                    message = $"Error de la fecha en la data de la fila {count}, no puede ser nula";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                var datefile = ParseDate(valueLines[fecha]);
                                var dateToday = DateTime.Now;

                                if (datefile.Contains("Error"))
                                {
                                    message = $"Error de la fecha en la data de la fila {count}, no tiene el formato correcto";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                if (DateTime.Parse(datefile) > dateToday)
                                {
                                    message = $"Error de la fecha en la data de la fila {count}, no puede ser mayor a la fecha actual";
                                    RegisterError(dataTableError, valueLines, count, message);
                                    count++;
                                    continue;
                                }

                                var assetTemp = assetList.FirstOrDefault(x => x.CodeSig == valueLines[1]);
                                int countDecLat = 5;
                                var latTemp = Math.Round(Decimal.Parse(valueLines[latitud]), countDecLat);
                                int countDecLong = 5;
                                var longTemp = Math.Round(Decimal.Parse(valueLines[longitud]), countDecLong);

                                if (assetTemp != null)
                                {                                    

                                    latTemp = Math.Round(Decimal.Parse(valueLines[latitud]), countDecLat);

                                    longTemp = Math.Round(Decimal.Parse(valueLines[longitud]), countDecLong);

                                    var assetTempLat = Math.Round(Decimal.Parse(assetTemp.Latitude.ToString()), countDecLat);

                                    var assetTempLong = Math.Round(Decimal.Parse(assetTemp.Longitude.ToString()), countDecLong);

                                    if (float.Parse(assetTempLat.ToString()) != float.Parse(latTemp.ToString()))
                                    {
                                        message = $"Error en la columna de Latitud de la fila {count}, no puede ser diferente a la latitud establecida para este code_sig";
                                        RegisterError(dataTableError, valueLines, count, message);
                                        count++;
                                        continue;
                                    }

                                    if (float.Parse(assetTempLong.ToString()) != float.Parse(longTemp.ToString()))
                                    {
                                        message = $"Error en la columna de Longitud de la fila {count}, no puede ser diferente a la longitud establecida para este code_sig";
                                        RegisterError(dataTableError, valueLines, count, message);
                                        count++;
                                        continue;
                                    }

                                    InsertData(dataTable, valueLines, columns, latTemp, longTemp);
                                    count++;
                                    continue;
                                }

                                InsertData(dataTable, valueLines, columns, latTemp, longTemp);
                                count++;
                                

                            }
                            beacon = 0;
                        }                        
                            
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        createCSV(dataTable, filePath, columns);
                    }

                    statusFilesingle.Status = 1;

                    if (dataTableError.Rows.Count > 0)
                    {
                        statusFilesingle.Status = 2;
                        errorFlag = true;
                        createCSVError(dataTableError, filePath);
                    }

                    var entityMap = mapper.Map<QueueStatusTt2>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataTT2(entityMap);

                    statusFileList.Add(statusFilesingle);

                }
                if (errorFlag)
                {
                    response.Message = "validation of the file with errors";
                    response.SuccessData = false;
                    //response.Success = false;
                    response.Success = true; // cambiar en prod
                    response.Data = statusFileList;
                    return response;
                }
                else
                {
                    response.Message = "All files are created";
                    response.SuccessData = true;
                    response.Success = true;
                    response.Data = statusFileList;
                    return response;
                }

            }
            catch (FormatException ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                response.Data = new List<StatusFileDTO>();
            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                response.Data = new List<StatusFileDTO>();
            }

            return response;

        }

        private static void InsertData(DataTable dataTable, string[] valueLines, int columns, decimal latTemp, decimal longTemp)
        {
            var newRow = dataTable.NewRow();

            for (int i = 0; i <= columns - 1; i++)
            {
                newRow[i] = valueLines[i].ToUpper().Trim();
            }
            newRow[0] = valueLines[0].ToUpper().Trim();
            newRow[1] = valueLines[1].ToUpper().Trim();
            newRow[2] = valueLines[2].ToUpper().Trim();
            newRow[3] = valueLines[3].ToUpper().Trim();
            newRow[4] = valueLines[4].ToUpper().Trim();
            newRow[5] = valueLines[5].ToUpper().Trim();
            newRow[6] = valueLines[6].ToUpper().Trim();
            newRow[7] = longTemp.ToString();
            newRow[8] = latTemp.ToString();
            newRow[9] = valueLines[9].ToUpper().Trim();
            newRow[10] = valueLines[10].ToUpper().Trim();
            newRow[11] = valueLines[11].ToUpper().Trim();
            newRow[12] = valueLines[12].ToUpper().Trim();


            dataTable.Rows.Add(newRow);

        }

        private static void RegisterError(DataTable table, string[] item, int count, string message)
        {            

            var newRow = table.NewRow();

            newRow[0] = message;
            var textLines = new StringBuilder();
            foreach (var text in item)
            {
                textLines.Append($"{text}, ");
            }
            newRow[1] = textLines;

            table.Rows.Add(newRow);

        }

        private void createCSV(DataTable table, string filePath, int columns)
        {
            string inputFolder = _TT2DirectoryPath;
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < columns; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private void createCSVError(DataTable table, string filePath)
        {
            string inputFolder = _TT2DirectoryPath;
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Error.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private string ParseDate(string dateString)
        {            
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString();
                }
            }
            return $"Error en el formato de fecha {dateString} no es válido.";            
        }

        private DateOnly ParseDateTemp(string dateString)
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
