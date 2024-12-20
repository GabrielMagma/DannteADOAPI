using ADO.BL.Interfaces;
using ADO.BL.Responses;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class ExcelCSVServices : IExcelCSVServices
    {
        private readonly string _ExcelCSVDirectoryPath;
        public ExcelCSVServices(IConfiguration configuration)
        {
            _ExcelCSVDirectoryPath = configuration["ExcelCSVPath"];
        }

        public ResponseQuery<string> ProcessXlsx(ResponseQuery<string> response)
        {
            try
            {

                string inputFolder = _ExcelCSVDirectoryPath;

                // Procesar cada archivo .xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
                {
                    ProcessXlsxToCsv(filePath, inputFolder);
                }

                Console.WriteLine("Proceso completado.");

                response.Message = "File created on the project root ./filesData";
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

        static void ProcessXlsxToCsv(string filePath, string inputFolder)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var worksheet = package.Workbook.Worksheets[0];
                var dataTable = new DataTable();

                // agregar nombres a columnas
                dataTable.Columns.Add("TIPO DE ACTIVO");
                dataTable.Columns.Add("CÓDIGO CREG (CODE_SIG)");
                dataTable.Columns.Add("IDENTIFICACIÓN DE ACTIVO (UIA)");
                dataTable.Columns.Add("CÓDIGO TAXONÓMICO (CODETAXO)");
                dataTable.Columns.Add("CÓDIGO DEL CIRCUITO AL QUE PERTENECE EL ACTIVO (FPARENT)");
                dataTable.Columns.Add("UBICACIÓN GEOGRÁFICA DE ACTIVO (LATITUDE)");
                dataTable.Columns.Add("UBICACIÓN GEOGRÁFICA DE ACTIVO (LONGITUDE)");
                dataTable.Columns.Add("TIPO DE POBLACIÓN (POBLATION)");
                dataTable.Columns.Add("GRUPO DE CALIDAD (GROUP015)");
                dataTable.Columns.Add("UNIDAD CONSTRUCTIVA DEL ACTIVO (UCCAP14)");
                dataTable.Columns.Add("FECHA DE PUESTA EN OPERACIÓN (DATE_INST)");
                dataTable.Columns.Add("FECHA DE DESINSTALACIÓN (DATE_UNIN)");
                dataTable.Columns.Add("ESTADO (STATE)");
                dataTable.Columns.Add("ID ZONA (ID_ZONE)");
                dataTable.Columns.Add("NOMBRE ZONA (NAME_ZONE)");
                dataTable.Columns.Add("ID REGION (ID_REGION)");
                dataTable.Columns.Add("NOMBRE REGION (NAME_REGION)");
                dataTable.Columns.Add("ID LOCALITY (ID_LOCALITY)");
                dataTable.Columns.Add("NOMBRE LOCALITY (NAME_LOCALITY)");
                dataTable.Columns.Add("ID SECTOR (ID_SECTOR)");
                dataTable.Columns.Add("NOMBRE SECTOR (NAME_SECTOR)");
                dataTable.Columns.Add("CÓDIGO GEOGRÁFICO (GEOGRAPHICAL_CODE)");
                dataTable.Columns.Add("DIRECCIÓN (ADDRESS)");

                for (int row = 3; row <= worksheet.Dimension.End.Row; row++)
                {
                    var newRow = dataTable.NewRow();

                    var date = worksheet.Cells[row, 11].Text != "" ? worksheet.Cells[row, 11].Text.ToUpper() : string.Empty;
                    if (date != "")
                    {
                        date = ParseDate(date);
                    }
                    newRow[0] = worksheet.Cells[row, 1].Text != "" ? worksheet.Cells[row, 1].Text.ToUpper() : string.Empty;
                    newRow[1] = worksheet.Cells[row, 2].Text != "" ? worksheet.Cells[row, 2].Text.ToUpper() : string.Empty;
                    newRow[2] = worksheet.Cells[row, 3].Text != "" ? worksheet.Cells[row, 3].Text.ToUpper() : string.Empty;
                    newRow[3] = worksheet.Cells[row, 4].Text != "" ? worksheet.Cells[row, 4].Text.ToUpper() : string.Empty;
                    newRow[4] = worksheet.Cells[row, 5].Text != "" ? worksheet.Cells[row, 5].Text.ToUpper() : string.Empty;
                    newRow[5] = worksheet.Cells[row, 6].Text != "" ? worksheet.Cells[row, 6].Text.ToUpper() : string.Empty;
                    newRow[6] = worksheet.Cells[row, 7].Text != "" ? worksheet.Cells[row, 7].Text.ToUpper() : string.Empty;
                    newRow[7] = worksheet.Cells[row, 8].Text != "" ? worksheet.Cells[row, 8].Text.ToUpper() : string.Empty;
                    newRow[8] = worksheet.Cells[row, 9].Text != "" ? worksheet.Cells[row, 9].Text.ToUpper() : string.Empty;
                    newRow[9] = worksheet.Cells[row, 12].Text != "" ? worksheet.Cells[row, 12].Text.ToUpper() : string.Empty;
                    newRow[10] = date;
                    newRow[11] = "2099-12-31";
                    newRow[12] = "2";
                    newRow[13] = "1";
                    newRow[14] = "GENERAL";
                    newRow[15] = "1";
                    newRow[16] = "ZONA GENERAL";
                    newRow[17] = "1";
                    newRow[18] = "LOCALIDAD GENERAL";
                    newRow[19] = "1";
                    newRow[20] = "SECTOR GENERAL";
                    newRow[21] = "NO DATA";
                    newRow[22] = worksheet.Cells[row, 10].Text != "" ? worksheet.Cells[row, 10].Text.ToUpper() : string.Empty;

                    dataTable.Rows.Add(newRow);
                }

                // Guardar como CSV
                string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_assets.csv");
                using (var writer = new StreamWriter(outputFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {

                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (var i = 0; i < 23; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }

        private static string ParseDate(string dateString)
        {
            var _timeFormats = new List<string> {
                    "yyyy-MM-dd HH:mm",
                    "dd-MM-yyyy HH:mm",
                    "yyyy/MM/dd HH:mm",
                    "dd/MM/yyyy HH:mm",
                    "dd/MM/yyyy",
                    "d/MM/yyyy",
                };
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    var datesplit = parsedDate.ToString().Split("/");
                    var date = datesplit[2] + "-" + datesplit[1] + "-" + datesplit[0] + " 00:00:00";
                    return date;
                }
            }
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }
    }

}