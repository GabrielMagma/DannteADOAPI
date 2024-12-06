using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using OfficeOpenXml;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class RayosCSVServices : IRayosCSVServices
    {
        private readonly IRayosCSVDataAccess rayosCSVDataAccess;
        private readonly IMapper mapper;
        public RayosCSVServices(IRayosCSVDataAccess _rayosCSVDataAccess, IMapper _mapper)
        {
            rayosCSVDataAccess = _rayosCSVDataAccess;
            mapper = _mapper;
        }

        public ResponseEntity<List<string>> SearchDataCSV(ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = "C:\\Users\\ingen\\source\\repos\\LecturaCSV\\LecturaCSV\\filesData";        

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    string[] fileLines = File.ReadAllLines(filePath);
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    //int count = 1;
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 12; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }
                    var listDTOMpLightning = new List<MpLightningDTO>();

                    // columnas tabla datos correctos
                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(";");
                        string message = string.Empty;
                        var dateTemp = valueLines[2] != "" ? valueLines[2] : string.Empty;
                        var date = dateTemp.Split(',', '.')[0];
                        if (valueLines.Length != 12)
                        {
                            message = "Error de cantidad de columnas llenas";
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"Error en la data {item}, formato incorrecto";
                            dataTableError.Rows.Add(newRowError);
                        }
                        else if (valueLines[0] == "" || valueLines[1] == "" || valueLines[2] == "" ||
                                valueLines[3] == "" || valueLines[4] == "" || valueLines[5] == "" ||
                                valueLines[9] == "")
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"Error en la data {item}, las columnas Fecha, Región, Zona, Circuito, Latitud, Longitud y Municipio son Requeridas";
                            dataTableError.Rows.Add(newRowError);

                        }
                        else if (date != "")
                        {
                            date = ParseDate(date);
                            if (date.Contains("Error"))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = date + $" En la data {item}";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else
                            {
                                var newRow = dataTable.NewRow();
                                newRow[0] = date;

                                newRow[1] = valueLines[0];
                                newRow[2] = valueLines[1];
                                newRow[3] = valueLines[11];
                                newRow[4] = valueLines[3].Replace(" ", "");
                                newRow[5] = valueLines[4];
                                newRow[6] = valueLines[5];
                                newRow[7] = valueLines[6];
                                newRow[8] = valueLines[7];
                                newRow[9] = valueLines[8];
                                newRow[10] = valueLines[9];
                                newRow[11] = valueLines[10];
                                dataTable.Rows.Add(newRow);

                                var newEntity = new MpLightningDTO();
                                var aniomes = date.Split('/', ' ');
                                newEntity.NameRegion = valueLines[0].Trim().ToUpper();
                                newEntity.NameZone = valueLines[1].Trim().ToUpper();
                                newEntity.NameLocality = valueLines[8].Trim().ToUpper();
                                newEntity.Fparent = valueLines[11].Trim().Replace(" ", "");
                                newEntity.DateEvent = DateTime.Parse(date);
                                newEntity.Latitude = float.Parse(valueLines[3].Replace(',', '.').Trim());
                                newEntity.Longitude = float.Parse(valueLines[4].Replace(',', '.').Trim());
                                newEntity.Amperage = float.Parse(valueLines[6].Replace(',', '.').Trim());
                                newEntity.Error = float.Parse(valueLines[7].Replace(',', '.').Trim());
                                newEntity.Type = int.Parse(valueLines[5].Trim());
                                newEntity.Year = int.Parse(aniomes[2]);
                                newEntity.Month = int.Parse(aniomes[1]);

                                listDTOMpLightning.Add(newEntity);

                            }

                        }
                        else
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"Error en la fila {item} y columna Fecha, la fecha no puede ser nula";
                            dataTableError.Rows.Add(newRowError);
                        }

                    }

                    // Guardar como CSV
                    string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
                    using (var writer = new StreamWriter(outputFilePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {

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
                        RegisterError(dataTableError, inputFolder, filePath);
                    }

                    if (listDTOMpLightning.Count > 0)
                    {
                        int i = 0;
                        while ((i * 1000) < listDTOMpLightning.Count())
                        {
                            var subgroup = listDTOMpLightning.Skip(i * 1000).Take(1000).ToList();
                            var EntityResult = mapper.Map<List<MpLightning>>(subgroup);
                            SaveData(EntityResult);
                            i++;
                            Console.WriteLine(i * 1000);
                        }

                    }


                    Console.WriteLine("Proceso completado.");
                }

                response.Message = "All Registers are created and/or updated";
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

        public ResponseEntity<List<string>> SearchDataExcel(ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = "C:\\Users\\ingen\\source\\repos\\LecturaCSV\\LecturaCSV\\filesData";

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
                {
                    ProcessXlsxToCsv(filePath, inputFolder);
                }


                void ProcessXlsxToCsv(string filePath, string inputFolder)
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet = package.Workbook.Worksheets[0];
                        var dataTable = new DataTable();
                        var dataTableError = new DataTable();
                        var listDTOMpLightning = new List<MpLightningDTO>();

                        // agregar nombres a columnas
                        for (int i = 1; i <= 12; i++)
                        {
                            dataTable.Columns.Add($"C{i}");
                        }

                        // columnas tabla error
                        dataTableError.Columns.Add("C1");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 12; i++)
                            {
                                if (worksheet.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 12)
                            {
                                break;
                            }
                            var date = worksheet.Cells[row, 1].Text != "" ? worksheet.Cells[row, 1].Text : string.Empty;
                            if (worksheet.Cells[row, 1].Text == "" || worksheet.Cells[row, 2].Text == "" || worksheet.Cells[row, 3].Text == "" ||
                                worksheet.Cells[row, 4].Text == "" || worksheet.Cells[row, 5].Text == "" || worksheet.Cells[row, 6].Text == "" ||
                                worksheet.Cells[row, 10].Text == "")
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data en la fila {row}, las columnas Fecha, Región, Zona, Circuito, Latitud, Longitud y Municipio son Requeridas";
                                dataTableError.Rows.Add(newRowError);

                            }
                            else if (date != "")
                            {
                                date = ParseDate(date);
                                if (date.Contains("Error"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = date + $" En la fila {row} y columna Fecha";
                                    dataTableError.Rows.Add(newRowError);
                                }
                                else
                                {
                                    var newRow = dataTable.NewRow();
                                    newRow[0] = date;
                                    for (int i = 1; i <= 11; i++)
                                    {
                                        newRow[i] = worksheet.Cells[row, i + 1].Text != "" ? worksheet.Cells[row, i + 1].Text.Trim().ToUpper() : string.Empty;
                                    }
                                    dataTable.Rows.Add(newRow);

                                    var newEntity = new MpLightningDTO();
                                    var aniomes = worksheet.Cells[row, 1].Text.Split('/', ' ');
                                    newEntity.NameRegion = worksheet.Cells[row, 2].Text.Trim().ToUpper();
                                    newEntity.NameZone = worksheet.Cells[row, 3].Text.Trim().ToUpper();
                                    newEntity.NameLocality = worksheet.Cells[row, 10].Text.Trim().ToUpper();
                                    newEntity.Fparent = worksheet.Cells[row, 4].Text.Trim();
                                    newEntity.DateEvent = DateTime.Parse(worksheet.Cells[row, 1].Text);
                                    newEntity.Latitude = float.Parse(worksheet.Cells[row, 5].Text.Trim());
                                    newEntity.Longitude = float.Parse(worksheet.Cells[row, 6].Text.Trim());
                                    newEntity.Amperage = float.Parse(worksheet.Cells[row, 8].Text.Trim());
                                    newEntity.Error = float.Parse(worksheet.Cells[row, 9].Text.Trim());
                                    newEntity.Type = int.Parse(worksheet.Cells[row, 7].Text.Trim());
                                    newEntity.Year = int.Parse(aniomes[2]);
                                    newEntity.Month = int.Parse(aniomes[1]);

                                    listDTOMpLightning.Add(newEntity);

                                }

                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} y columna Fecha, la fecha no puede ser nula";
                                dataTableError.Rows.Add(newRowError);
                            }

                        }

                        // Guardar como CSV
                        string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
                        using (var writer = new StreamWriter(outputFilePath))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {

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
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        if (listDTOMpLightning.Count > 0)
                        {
                            int i = 0;
                            while ((i * 1000) < listDTOMpLightning.Count())
                            {
                                var subgroup = listDTOMpLightning.Skip(i * 1000).Take(1000).ToList();
                                var EntityResult = mapper.Map<List<MpLightning>>(subgroup);
                                SaveData(EntityResult);
                                i++;
                                Console.WriteLine(i * 1000);
                            }

                        }
                    }

                }
                response.Message = "All Registers are created and/or updated";
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

        private static void RegisterError(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Error.csv");
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

        private static string ParseDate(string dateString)
        {
            var _timeFormats = new List<string> {
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss tt",
                    "yyyy-MM-dd HH:mm:ss,fff",
                    "yyyy-MM-dd HH:mm:ss,ffff",
                    "yyyy-MM-dd HH:mm:ss,fffff",
                    "yyyy-MM-dd HH:mm:ss,ffffff",
                    "yyyy-MM-dd HH:mm:ss.fff",
                    "yyyy-MM-dd HH:mm:ss.ffff",
                    "yyyy-MM-dd HH:mm:ss.fffff",
                    "yyyy-MM-dd HH:mm:ss.ffffff",
                    "yyyy-MM-dd HH:mm",
                    "dd-MM-yyyy HH:mm",
                    "yyyy/MM/dd HH:mm",
                    "dd/MM/yyyy HH:mm",
                    "d/MM/yyyy HH:mm",
                    "d/MM/yyyy H:mm",
                    "dd/MM/yyyy HH:mm:ss",
                    "d/MM/yyyy HH:mm:ss",
                    "d/MM/yyyy H:mm:ss",
                    "dd/MM/yyyy",
                    "d/MM/yyyy",
                    "dd-MM-yyyy",
            };
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString();
                }
            }
            return $"Error en el formato de fecha {dateString} no es válido.";
            //throw new FormatException($"Error en el formato de fecha {dateString} no es válido.");
        }

        // acciones en bd y mappeo

        public Boolean SaveData(List<MpLightning> request)
        {
            
                var result = rayosCSVDataAccess.SaveData(request);

                return result;            

        }        

    }
}
