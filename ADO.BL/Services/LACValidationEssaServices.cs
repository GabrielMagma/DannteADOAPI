using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class LACValidationEssaServices : ILACValidationEssaServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _FilesLACDirectoryPath;
        private readonly IMapper mapper;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        public LACValidationEssaServices(IConfiguration configuration,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _FilesLACDirectoryPath = configuration["FilesLACPath"];
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseEntity<List<StatusFileDTO>>> ValidationLAC(LacValidationDTO request, ResponseEntity<List<StatusFileDTO>> response)
        {            

            try
            {
                string inputFolder = _FilesLACDirectoryPath;
                var errorFlag = false;
                int eventCode = 0;
                int startDate =  1;
                int endDate =  2;
                int uia =  3;
                int eventContinues =  6;
                if (request.Encabezado == false)
                {
                    eventCode = request.columns.EventCode - 1;
                    startDate = request.columns.StartDate - 1;
                    endDate = request.columns.EndDate - 1;
                    uia = request.columns.Uia - 1;
                    eventContinues = request.columns.EventContinues - 1;
                }
                var statusFilesList = new List<StatusFileDTO>();
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv")
                                        .Where(file => !file.EndsWith("_Correct.csv")
                                                        && !file.EndsWith("_Error.csv")
                                                        && !file.EndsWith("_unchanged.csv")
                                                        && !file.EndsWith("_continues.csv")
                                                        && !file.EndsWith("_continuesInvalid.csv")
                                                        && !file.EndsWith("_closed.csv")
                                                        && !file.EndsWith("_closedInvalid.csv"))
                                        .ToList().OrderBy(f => f)
                     .ToArray()
                    )
                {                    
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    int count = 1;
                    var columns = int.Parse(_configuration["Validations:LACColumns"]);                    
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    string[] fileLines = File.ReadAllLines(filePath);
                    // Asumiendo que el formato del archivo es AAAAMMDD_LAC.csv

                    var resultYearMonth = getYearMonth(fileLines);
                    int year = 2099;
                    int month = 12;
                    int day = 31;                    
                    if (resultYearMonth.Count > 0)
                    {
                        year = int.Parse(resultYearMonth[0]);
                        month = int.Parse(resultYearMonth[1]);
                        day = int.Parse(resultYearMonth[2]);
                    }
                    var statusFilesingle = new StatusFileDTO();

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "LAC";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = day;
                    statusFilesingle.DateRegister = ParseDateTemp($"{day}/{month}/{year}");

                    

                    // columnas tabla datos correctos
                    for (int i = 1; i <= columns; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }                                                        
                    
                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
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
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[uia] == "")
                            {
                                message = "Error en el código UIA, no puede ser nulo";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] == "")
                            {
                                message = "Error en el código de evento, no puede ser nulo";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && (valueLines[startDate] == "" && valueLines[endDate] == ""))
                            {
                                message = "Error de la data, no está llena correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }                            

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] != "" && valueLines[endDate] != "" && valueLines[eventContinues] == "S")
                            {
                                message = "Error de las fechas en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] == "" && valueLines[endDate] != "" && valueLines[eventContinues] == "S")
                            {
                                message = "Error de la fecha de terminación y/o estado en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] != "" && valueLines[endDate] == "" && valueLines[eventContinues] == "N")
                            {
                                message = "Error de la fecha de terminación y/o estado en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            if (valueLines[eventCode] != "NA" && valueLines[eventCode] != "") {
                                InsertData(dataTable, valueLines, columns);
                            }
                        }

                        count++;
                        beacon = 0;
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

                    var entityMap = mapper.Map<QueueStatusLac>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataLAC(entityMap);

                    statusFilesList.Add(statusFilesingle);

                }
                if (errorFlag)
                {
                    response.Message = "file with errors";
                    response.SuccessData = false;
                    response.Success = false;
                    response.Data = statusFilesList;
                    return response;
                }
                else {
                    response.Message = "All files are created";
                    response.SuccessData = true;
                    response.Success = true;
                    response.Data = statusFilesList;
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

        private static void InsertData(DataTable dataTable, string[] valueLines, int columns)
        {
            var newRow = dataTable.NewRow();

            for (int i = 0; i < columns; i++)
            {
                newRow[i] = valueLines[i].ToUpper().Trim();
            }

            dataTable.Rows.Add(newRow);

        }

        private static void RegisterError(DataTable table, string item, int count, string message)
        {
            var messageError = $"{message} en la línea {count} del archivo cargado";

            var newRow = table.NewRow();

            newRow[0] = item;
            newRow[1] = messageError;

            table.Rows.Add(newRow);

        }

        private void createCSV(DataTable table, string filePath, int columns)
        {
            string inputFolder = _FilesLACDirectoryPath;
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
            string inputFolder = _FilesLACDirectoryPath;
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

        private DateTime ParseDate(string dateString)
        {           
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToUniversalTime();
                }
            }
            return ParseDate("31/12/2099 00:00:00");            
        }

        private DateOnly ParseDateTemp(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private List<string> getYearMonth(string[] lines)
        {
            var yearMonth = new List<string>();
            for (int i = 1; i < lines.Count(); i++)
            {
                var valueLines = lines[i].Split(',', ';');
                if (string.IsNullOrEmpty(valueLines[1]))
                {
                    continue;
                }
                var resultDate = ParseDate(valueLines[1]);
                if (resultDate != ParseDate("31/12/2099 00:00:00"))
                {
                    // formato fecha "dd/MM/YYYY"
                    var dateTemp = resultDate.ToString().Split('/', ' ');
                    yearMonth.Add(dateTemp[2]);
                    yearMonth.Add(dateTemp[1]);
                    yearMonth.Add(dateTemp[0]);
                    break;
                }

            }
            return yearMonth;
        }
    }
}
