using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class ExcelCSVCompensacionesEEPServices : IExcelCSVCompensacionesEEPServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _EEPCompDirectoryPath;
        public ExcelCSVCompensacionesEEPServices(IConfiguration configuration)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _EEPCompDirectoryPath = configuration["EepCompensationsPath"];
        }

        public ResponseQuery<string> Convert(ResponseQuery<string> response)
        {
            try
            {

                string inputFolder = _EEPCompDirectoryPath;

                // Procesar cada archivo .xlsx en la carpeta

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
                {
                    ProcessXlsxToCsv(filePath, inputFolder);
                }


                Console.WriteLine("Proceso completado.");
                response.Message = "File created on the project root ./FilesCompensacionesEEP";
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
                var listAssetsDTO = new List<AssetsDTO>();

                // agregar nombres a columnas
                dataTable.Columns.Add("month");
                dataTable.Columns.Add("year");
                dataTable.Columns.Add("fparent");
                dataTable.Columns.Add("code_sig");
                dataTable.Columns.Add("quality_group");
                dataTable.Columns.Add("tension_level");
                dataTable.Columns.Add("nui");
                dataTable.Columns.Add("vcf");
                dataTable.Columns.Add("vcd");
                dataTable.Columns.Add("vc");
                dataTable.Columns.Add("longitude");
                dataTable.Columns.Add("latitude");

                dataTableError.Columns.Add("C1");
                dataTableError.Columns.Add("C2");

                List<FparentDTO> items = new List<FparentDTO>
                {
                   new FparentDTO { Code="11", value="1VE" },
                   new FparentDTO { Code="12", value="2VE" },
                   new FparentDTO { Code="13", value="3VE" },
                   new FparentDTO { Code="14", value="4VE" },
                   new FparentDTO { Code="15", value="5VE" },
                   new FparentDTO { Code="16", value="6VE" },
                   new FparentDTO { Code="17", value="VHM" },
                   new FparentDTO { Code="18", value="IVE" },
                   new FparentDTO { Code="19", value="V14" },
                   new FparentDTO { Code="20", value="1DQ" },
                   new FparentDTO { Code="21", value="2DQ" },
                   new FparentDTO { Code="22", value="3DQ" },
                   new FparentDTO { Code="23", value="4DQ" },
                   new FparentDTO { Code="24", value="5DQ" },
                   new FparentDTO { Code="25", value="6DQ" },
                   new FparentDTO { Code="26", value="7DQ" },
                   new FparentDTO { Code="27", value="8DQ" },
                   new FparentDTO { Code="28", value="IDQ" },
                   new FparentDTO { Code="29", value="ANDI" },
                   new FparentDTO { Code="30", value="1CU" },
                   new FparentDTO { Code="31", value="3CU" },
                   new FparentDTO { Code="32", value="4CU" },
                   new FparentDTO { Code="33", value="5CU" },
                   new FparentDTO { Code="34", value="6CU" },
                   new FparentDTO { Code="35", value="7CU" },
                   new FparentDTO { Code="36", value="8CU" },
                   new FparentDTO { Code="37", value="9CU" },
                   new FparentDTO { Code="38", value="1CE" },
                   new FparentDTO { Code="39", value="2CE" },
                   new FparentDTO { Code="40", value="3CE" },
                   new FparentDTO { Code="41", value="4CE" },
                   new FparentDTO { Code="42", value="5CE" },
                   new FparentDTO { Code="43", value="POPA" },
                   new FparentDTO { Code="44", value="MAC" },
                   new FparentDTO { Code="45", value="1NA" },
                   new FparentDTO { Code="46", value="2NA" },
                   new FparentDTO { Code="47", value="3NA" },
                   new FparentDTO { Code="48", value="BADEA" },
                   new FparentDTO { Code="49", value="1PA" },
                   new FparentDTO { Code="50", value="2PA" },
                   new FparentDTO { Code="51", value="3PA" },
                   new FparentDTO { Code="52", value="IPA1" },
                   new FparentDTO { Code="53", value="IPA2" },
                   new FparentDTO { Code="58", value="2CU" },
                   new FparentDTO { Code="59", value="ICU" },
                   new FparentDTO { Code="60", value="7VE" },
                   new FparentDTO { Code="61", value="4NA" },
                   new FparentDTO { Code="62", value="ANDI DQ" },

                };

                var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteEepTesting";
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var SelectQueryAssets = $@"SELECT distinct code_sig, fparent, latitude, longitude from public.all_asset";
                    using (var reader = new NpgsqlCommand(SelectQueryAssets, connection))
                    {
                        try
                        {

                            using (var result = reader.ExecuteReader())
                            {
                                while (result.Read())
                                {
                                    var temp = new AssetsDTO();
                                    temp.CodeSig = result[0].ToString();
                                    temp.Fparent = result[1].ToString();
                                    temp.Latitude = float.Parse(result[2].ToString());
                                    temp.Longitude = float.Parse(result[3].ToString());

                                    listAssetsDTO.Add(temp);
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

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var newRow = dataTable.NewRow();

                    var cont = 0;

                    for (int i = 1; i < 40; i++)
                    {
                        if (worksheet.Cells[row, i].Text != "")
                        { cont++; }

                    }

                    if (cont == 0)
                    {
                        break;
                    }

                    if (worksheet.Cells[row, 1].Text == "" || worksheet.Cells[row, 2].Text == "" || worksheet.Cells[row, 6].Text == "" ||
                        worksheet.Cells[row, 9].Text == "" || worksheet.Cells[row, 10].Text == "" || worksheet.Cells[row, 33].Text == "" ||
                        worksheet.Cells[row, 34].Text == "" || worksheet.Cells[row, 35].Text == "" || worksheet.Cells[row, 39].Text == "")
                    {
                        var newRowError = dataTableError.NewRow();
                        newRowError[0] = $"Error de la data en la fila {row}";
                        dataTableError.Rows.Add(newRowError);
                    }

                    var date = worksheet.Cells[row, 1].Text != "" ? worksheet.Cells[row, 1].Text.ToUpper() : string.Empty;
                    if (date != "")
                    {
                        date = ParseDate(date);
                        if (!date.Contains("Error"))
                        {

                            var temp = listAssetsDTO.FirstOrDefault(x => x.CodeSig == worksheet.Cells[row, 39].Text);
                            if (temp != null)
                            {
                                if (temp.Latitude != null && temp.Longitude != null)
                                {
                                    var fparentDef = temp.Fparent;
                                    var fparentTemp = items.FirstOrDefault(x => x.Code == worksheet.Cells[row, 39].Text);
                                    if (fparentTemp != null)
                                    {
                                        fparentDef = fparentTemp.value;
                                    }

                                    var dateDef = date.Split('/', '-');
                                    newRow[0] = dateDef[1];
                                    newRow[1] = dateDef[2];
                                    newRow[2] = fparentDef;
                                    newRow[3] = worksheet.Cells[row, 39].Text != "" ? worksheet.Cells[row, 39].Text.ToUpper() : string.Empty;
                                    newRow[4] = worksheet.Cells[row, 10].Text != "" ? worksheet.Cells[row, 10].Text.ToUpper() : string.Empty;
                                    newRow[5] = worksheet.Cells[row, 9].Text != "" ? worksheet.Cells[row, 9].Text.ToUpper() : string.Empty;
                                    newRow[6] = worksheet.Cells[row, 2].Text != "" ? worksheet.Cells[row, 2].Text.ToUpper() : string.Empty;
                                    newRow[7] = worksheet.Cells[row, 33].Text != "" ? worksheet.Cells[row, 33].Text.ToUpper() : string.Empty;
                                    newRow[8] = worksheet.Cells[row, 34].Text != "" ? worksheet.Cells[row, 34].Text.ToUpper() : string.Empty;
                                    newRow[9] = worksheet.Cells[row, 35].Text != "" ? worksheet.Cells[row, 35].Text.ToUpper() : string.Empty;
                                    newRow[10] = temp.Longitude;
                                    newRow[11] = temp.Latitude;
                                    dataTable.Rows.Add(newRow);
                                }
                                else
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error de data en la fila {row}, sin fparent, latitud o longitud";
                                    dataTableError.Rows.Add(newRowError);
                                }
                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error de data en la fila {row}, sin fparent, latitud o longitud";
                                dataTableError.Rows.Add(newRowError);
                            }
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
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (var i = 0; i < 12; i++)
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