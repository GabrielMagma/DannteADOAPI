using ADO.BL.Interfaces;
using ADO.BL.Responses;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class FileLACValidationServices : IFileLACValidationServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _FilesLACDirectoryPath;
        public FileLACValidationServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _FilesLACDirectoryPath = configuration["FilesLACPath"];
        }

        public ResponseQuery<bool> ValidationLAC(IFormFile file, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _FilesLACDirectoryPath;

                var filePath = $"{inputFolder}\\{file.FileName}";
                    
                string nameFile = file.FileName.Replace(".csv","");
                var dataTable = new DataTable();
                var dataTableError = new DataTable();
                int count = 1;
                var columns = int.Parse(_configuration["Validations:LACColumns"]);
                var UIAPos = int.Parse(_configuration["Validations:LACUIA"]);
                // columnas tabla error
                dataTableError.Columns.Add("C1");
                dataTableError.Columns.Add("C2");

                // columnas tabla datos correctos
                for (int i = 1; i <= columns; i++)
                {
                    dataTable.Columns.Add($"C{i}");
                }

                var reader = new StreamReader(file.OpenReadStream());
                    
                while (reader.Peek() >= 0)
                {
                    var lines = string.Empty;
                    lines = reader.ReadLine();
                    var valueLines = lines.Split(",");
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
                        RegisterError(dataTableError, lines, count, nameFile, message);
                    }

                        else if (valueLines[UIAPos] == "")
                        {
                            message = "Error en el código UIA, no puede ser nulo";
                            RegisterError(dataTableError, lines, count, nameFile, message);
                        }

                        else if (valueLines[0] == "NA" && (valueLines[1] != "" || valueLines[2] != "" || valueLines[5] != ""
                        && valueLines[6] != "" || valueLines[7] != "" || valueLines[8] != "" || valueLines[9] != ""))
                    {
                        message = "Error de la data, no está llena correctamente";
                        RegisterError(dataTableError, lines, count, nameFile, message);
                    }

                    else if (valueLines[0] != "NA" && valueLines[1] == "")
                    {
                        message = "Error de las fechas en la data, no están llenas correctamente";
                        RegisterError(dataTableError, lines, count, nameFile, message);
                    }

                    else if (valueLines[0] != "NA" && valueLines[2] != "" && valueLines[6] == "S")
                    {
                        message = "Error de la fecha de terminación y/o estado en la data, no están llenas correctamente";
                        RegisterError(dataTableError, lines, count, nameFile, message);
                    }

                    else if (valueLines[0] != "NA" && valueLines[1] != "")
                    {
                        var datefile = ParseDate(valueLines[1]);
                        var dateToday = DateTime.Now;
                        if (datefile.Contains("Error"))
                        {
                            message = "Error de la fecha en la data, no tiene el formato correcto";
                            RegisterError(dataTableError, lines, count, nameFile, message);
                        }
                        else if (DateTime.Parse(datefile) > dateToday)
                        {
                            message = "Error de la fecha en la data, no puede ser mayor a la fecha actual";
                            RegisterError(dataTableError, lines, count, nameFile, message);
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

                if (dataTableError.Rows.Count > 0)
                {
                    createCSVError(dataTableError, filePath);
                }
                                
                response.Message = "All files are created";
                response.SuccessData = true;
                response.Success = true;
                return response;
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

        private static void InsertData(DataTable dataTable, string[] valueLines, int columns)
        {
            var newRow = dataTable.NewRow();

            for (int i = 0; i < columns; i++)
            {
                newRow[i] = valueLines[i];
            }

            dataTable.Rows.Add(newRow);

        }

        private static void RegisterError(DataTable table, string item, int count, string fileName, string message)
        {
            var messageError = $"{message} en la línea {count} del archivo {fileName}";

            var newRow = table.NewRow();

            newRow[0] = item;
            newRow[1] = messageError;

            table.Rows.Add(newRow);

        }

        private static void createCSV(DataTable table, string filePath, int columns)
        {
            string inputFolder = ".\\filesLAC";
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
            string inputFolder = ".\\filesLAC";
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
                if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString();
                }
            }
            return $"Error en el formato de fecha {dateString} no es válido.";            
        }
    }
}
