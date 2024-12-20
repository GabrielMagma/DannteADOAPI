using ADO.BL.Interfaces;
using ADO.BL.Responses;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class ExcelCSVCompensacionesESSAServices : IExcelCSVCompensacionesESSAServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _ESSACompDirectoryPath;
        public ExcelCSVCompensacionesESSAServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _ESSACompDirectoryPath = configuration["EssaCompensationsPath"];


        }

        public ResponseQuery<string> Convert(ResponseQuery<string> response)
        {
            try
            {


                string inputFolder = _ESSACompDirectoryPath;

                // Procesar cada archivo .xlsx en la carpeta

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
                {
                    ProcessXlsxToCsv(filePath, inputFolder);
                }


                Console.WriteLine("Proceso completado.");
                response.Message = "File created on the project root ./FilesCompensacionesESSA";
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

        private void ProcessXlsxToCsv(string filePath, string outputFolder)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var worksheet = package.Workbook.Worksheets[0];
                var dataTable = new DataTable();
                var dataTableError = new DataTable();

                // agregar nombres a columnas
                for (int i = 1; i <= 10; i++)
                {
                    dataTable.Columns.Add($"c{i}");
                }

                dataTableError.Columns.Add("C1");
                dataTableError.Columns.Add("C2");

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var newRow = dataTable.NewRow();

                    var cont = 0;

                    for (int i = 1; i <= 17; i++)
                    {
                        if (worksheet.Cells[row, i].Text != "")
                        { cont++; }

                    }

                    if (cont == 0)
                    {
                        break;
                    }

                    if (worksheet.Cells[row, 2].Text == "" || worksheet.Cells[row, 5].Text == "" || worksheet.Cells[row, 9].Text == "" ||
                        worksheet.Cells[row, 10].Text == "" || worksheet.Cells[row, 11].Text == "" || worksheet.Cells[row, 12].Text == "" ||
                        worksheet.Cells[row, 14].Text == "" || worksheet.Cells[row, 15].Text == "" || worksheet.Cells[row, 16].Text == "")
                    {
                        var newRowError = dataTableError.NewRow();
                        newRowError[0] = $"Error de la data en la fila {row}";
                        dataTableError.Rows.Add(newRowError);
                    }

                    var date = worksheet.Cells[row, 5].Text != "" ? worksheet.Cells[row, 5].Text.ToUpper() : string.Empty;
                    if (date != "")
                    {
                        date = ParseDate(date);
                        if (!date.Contains("Error"))
                        {
                            var dateDef = date.Split('/', '-');
                            newRow[0] = dateDef[1];
                            newRow[1] = dateDef[2];
                            newRow[2] = worksheet.Cells[row, 15].Text != "" ? worksheet.Cells[row, 15].Text.ToUpper() : string.Empty;
                            newRow[3] = worksheet.Cells[row, 14].Text != "" ? worksheet.Cells[row, 14].Text.ToUpper() : string.Empty;
                            newRow[4] = worksheet.Cells[row, 12].Text != "" ? worksheet.Cells[row, 12].Text.ToUpper() : string.Empty;
                            newRow[5] = worksheet.Cells[row, 16].Text != "" ? worksheet.Cells[row, 16].Text.ToUpper() : string.Empty;
                            newRow[6] = worksheet.Cells[row, 2].Text != "" ? worksheet.Cells[row, 2].Text.ToUpper() : string.Empty;
                            newRow[7] = worksheet.Cells[row, 11].Text != "" ? worksheet.Cells[row, 11].Text.ToUpper() : string.Empty;
                            newRow[8] = worksheet.Cells[row, 10].Text != "" ? worksheet.Cells[row, 10].Text.ToUpper() : string.Empty;
                            newRow[9] = worksheet.Cells[row, 9].Text != "" ? worksheet.Cells[row, 9].Text.ToUpper() : string.Empty;
                            dataTable.Rows.Add(newRow);
                        }
                        else
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"Error de la fecha en la fila {row}";
                            dataTableError.Rows.Add(newRowError);
                        }
                    }

                }

                // Guardar como CSV
                string outputFilePath = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
                using (var writer = new StreamWriter(outputFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {

                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }

                if (dataTableError.Rows.Count > 0)
                {
                    string outputFilePathError = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Error.csv");
                    using (var writer = new StreamWriter(outputFilePathError))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {

                        foreach (DataRow row in dataTableError.Rows)
                        {
                            csv.WriteField(row[0]);
                            csv.NextRecord();
                        }
                    }

                }
            }
        }

        private string ParseDate(string dateString)
        {
            //var _timeFormats = new List<string> {
            //        "yyyy-MM-dd HH:mm:ss",
            //        "yyyy-MM-dd HH:mm",
            //        "dd-MM-yyyy HH:mm",
            //        "yyyy/MM/dd HH:mm",
            //        "dd/MM/yyyy HH:mm",
            //        "dd/MM/yyyy HH:mm:ss",
            //        "dd/MM/yyyy",
            //        "d/MM/yyyy",
            //        "dd-MM-yyyy",
            //    };
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate.ToString();
                }
            }
            return $"Error en el formato de fecha {dateString} no es válido.";
            //throw new FormatException($"Error en el formato de fecha {dateString} no es válido.");
        }
    }
}
