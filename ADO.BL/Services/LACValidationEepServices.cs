using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class LACValidationEepServices : ILACValidationEepServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _FilesLACDirectoryPath;
        public LACValidationEepServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _FilesLACDirectoryPath = configuration["FilesLACPath"];
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
                var statusFileList = new List<StatusFileDTO>();
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {                    
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    int count = 1;
                    var columns = int.Parse(_configuration["Validations:LACColumns"]);                    
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    // Asumiendo que el formato del archivo es EEPDDDMM.csv

                    //// Obtener los primeros 4 dígitos como el año
                    //int month = int.Parse(fileName.Substring(6, 2));

                    //// Obtener los siguientes 2 dígitos como el mes
                    //int day = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "LAC";
                    statusFilesingle.Year = request.Year;
                    statusFilesingle.Month = request.Month;
                    statusFilesingle.Day = request.Day;

                    // columnas tabla datos correctos
                    for (int i = 1; i <= columns; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }                    

                    string[] fileLines = File.ReadAllLines(filePath);                    
                    
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
                            if (valueLines.Length < columns)
                            {
                                message = "Error de cantidad de columnas llenas";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[uia] == "")
                            {
                                message = "Error en el código UIA, no puede ser nulo";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && (valueLines[startDate] == "" && valueLines[endDate] == ""))
                            {
                                message = "Error de la data, no está llena correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] == "NA" && (valueLines[startDate] != "" || valueLines[endDate] != "" || valueLines[eventContinues] != ""))
                            {
                                message = "Error de la data, no está llena correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] != "" && valueLines[endDate] != "" && valueLines[eventContinues] == "S")
                            {
                                message = "Error de las fechas en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[endDate] != "" && valueLines[eventContinues] == "S")
                            {
                                message = "Error de la fecha de terminación y/o estado en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] != "")
                            {
                                var datefile = ParseDate(valueLines[startDate]);
                                var dateToday = DateTime.Now;
                                if (datefile == DateTime.Parse("31/12/2099 00:00:00"))
                                {
                                    message = "Error de la fecha en la data, no tiene el formato correcto";
                                    RegisterError(dataTableError, item, count, message);
                                }
                                else if (datefile > dateToday)
                                {
                                    message = "Error de la fecha en la data, no puede ser mayor a la fecha actual";
                                    RegisterError(dataTableError, item, count, message);
                                }
                                else
                                {
                                    InsertData(dataTable, valueLines, columns);
                                }
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
                        statusFilesingle.Status = 0;
                        errorFlag = true;
                        createCSVError(dataTableError, filePath);
                    }

                    statusFileList.Add(statusFilesingle);

                }
                if (errorFlag)
                {
                    response.Message = "file with errors";
                    response.SuccessData = false;
                    response.Success = false;
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

        private static void InsertData(DataTable dataTable, string[] valueLines, int columns)
        {
            var newRow = dataTable.NewRow();

            for (int i = 0; i < columns; i++)
            {
                newRow[i] = valueLines[i];
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

        private static void createCSV(DataTable table, string filePath, int columns)
        {
            string inputFolder = "C:\\Users\\ingen\\source\\repos\\DannteADOAPI\\files\\FilesLAC";
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

        private static void createCSVError(DataTable table, string filePath)
        {
            string inputFolder = "C:\\Users\\ingen\\source\\repos\\DannteADOAPI\\files\\FilesLAC";
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
            return DateTime.Parse("31/12/2099 00:00:00");            
        }
    }
}
