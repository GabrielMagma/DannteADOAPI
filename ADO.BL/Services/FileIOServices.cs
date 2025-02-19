//using ADO.BL.DataEntities;
//using ADO.BL.DTOs;
//using ADO.BL.Interfaces;
//using ADO.BL.Responses;
//using AutoMapper;
//using CsvHelper;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using OfficeOpenXml;
//using Oracle.ManagedDataAccess.Client;
//using System.Data;
//using System.Globalization;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace ADO.BL.Services
//{
//    public class FileIOServices : IFileIOServices
//    {
//        private readonly IMapper mapper;
//        private readonly string[] _timeFormats;
//        private readonly string _IOsDirectoryPath;
//        private readonly IFileIODataAccess fileIODataAccess;
//        public FileIOServices(IConfiguration configuration,
//            IMapper _mapper,
//            IFileIODataAccess _fileIODataAccess)
//        {
//            mapper = _mapper;
//            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
//            _IOsDirectoryPath = configuration["IOsPath"];
//            fileIODataAccess = _fileIODataAccess;
//        }

//        public async Task<ResponseQuery<string>> UploadIO(IOsValidationDTO iosValidation, ResponseQuery<string> response)
//        {
//            try
//            {
//                string inputFolder = _IOsDirectoryPath;
//                var errorFlag = false;

//                //Procesar cada archivo.xlsx en la carpeta
//                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
//                {
//                    using (var package = new ExcelPackage(new FileInfo(filePath)))
//                    {
//                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//                        var worksheet1 = package.Workbook.Worksheets[1];
//                        var worksheet2 = package.Workbook.Worksheets[3];
//                        var dataTableError = new DataTable();
//                        var listDTOMpLightning = new List<MpLightningDTO>();

//                        await DeleteDuplicateIO(filePath);

//                        // columnas tabla error
//                        dataTableError.Columns.Add("C1");

//                        for (int row = 6; row <= worksheet1.Dimension.End.Row; row++)
//                        {

//                            var beacon = 0;
//                            for (int i = 1; i <= 24; i++)
//                            {
//                                if (worksheet1.Cells[row, i].Text == "")
//                                {
//                                    beacon++;
//                                }
//                            }
//                            if (beacon == 24)
//                            {
//                                break;
//                            }

//                            var date = worksheet.Cells[row, fecha].Text != "" ? worksheet.Cells[row, fecha].Text : string.Empty;
//                            if (worksheet.Cells[row, fecha].Text == "" || worksheet.Cells[row, region].Text == "" || worksheet.Cells[row, zona].Text == "" ||
//                                worksheet.Cells[row, circuito].Text == "" || worksheet.Cells[row, latitud].Text == "" || worksheet.Cells[row, longitud].Text == "" ||
//                                worksheet.Cells[row, municipio].Text == "")
//                            {
//                                var newRowError = dataTableError.NewRow();
//                                newRowError[0] = $"Error en la data en la línea {row}, las columnas Fecha, Región, Zona, Circuito, Latitud, Longitud y Municipio son Requeridas";
//                                dataTableError.Rows.Add(newRowError);
//                            }
//                            else if (date != "")
//                            {
//                                date = ParseDate(date);
//                                if (date.Contains("Error"))
//                                {
//                                    var newRowError = dataTableError.NewRow();
//                                    newRowError[0] = date + $" En la línea {row} y columna Fecha";
//                                    dataTableError.Rows.Add(newRowError);
//                                }
//                                else
//                                {

//                                    var newEntity = new MpLightningDTO();
//                                    var aniomes = worksheet.Cells[row, fecha].Text.Split('/', ' ');
//                                    newEntity.NameRegion = worksheet.Cells[row, region].Text.Trim().ToUpper();
//                                    newEntity.NameZone = worksheet.Cells[row, zona].Text.Trim().ToUpper();
//                                    newEntity.NameLocality = worksheet.Cells[row, municipio].Text.Trim().ToUpper();
//                                    newEntity.Fparent = worksheet.Cells[row, circuito].Text.Trim().Replace(" ", "");
//                                    newEntity.DateEvent = DateTime.Parse(worksheet.Cells[row, fecha].Text);
//                                    newEntity.Latitude = float.Parse(worksheet.Cells[row, latitud].Text.Trim());
//                                    newEntity.Longitude = float.Parse(worksheet.Cells[row, longitud].Text.Trim());
//                                    newEntity.Amperage = float.Parse(worksheet.Cells[row, corriente].Text.Trim());
//                                    newEntity.Error = float.Parse(worksheet.Cells[row, error].Text.Trim());
//                                    newEntity.Type = int.Parse(worksheet.Cells[row, tipo].Text.Trim());
//                                    newEntity.Year = int.Parse(aniomes[2]);
//                                    newEntity.Month = int.Parse(aniomes[1]);

//                                    listDTOMpLightning.Add(newEntity);

//                                }

//                            }
//                            else
//                            {
//                                var newRowError = dataTableError.NewRow();
//                                newRowError[0] = $"Error en la fila {row} y columna Fecha, la fecha no puede ser nula";
//                                dataTableError.Rows.Add(newRowError);
//                            }

//                        }

//                        if (dataTableError.Rows.Count > 0)
//                        {
//                            errorFlag = true;
//                            RegisterError(dataTableError, inputFolder, filePath);
//                        }

//                        if (listDTOMpLightning.Count > 0 && errorFlag == false)
//                        {
//                            int i = 0;
//                            while ((i * 1000) < listDTOMpLightning.Count())
//                            {
//                                var subgroup = listDTOMpLightning.Skip(i * 1000).Take(1000).ToList();
//                                var EntityResult = mapper.Map<List<MpLightning>>(subgroup);
//                                SaveData(EntityResult);
//                                i++;
//                                Console.WriteLine(i * 1000);
//                            }

//                        }
//                    }

//                }

//                if (errorFlag)
//                {
//                    response.Message = "file with errors";
//                    response.SuccessData = false;
//                    response.Success = false;
//                    return response;
//                }
//                else
//                {
//                    response.Message = "All files are created";
//                    response.SuccessData = true;
//                    response.Success = true;
//                    return response;
//                }

//            }
//            catch (FormatException ex)
//            {
//                response.Message = ex.Message;
//                response.Success = false;
//                response.SuccessData = false;
//            }
//            catch (Exception ex)
//            {
//                response.Message = ex.Message;
//                response.Success = false;
//                response.SuccessData = false;
//            }

//            return response;
//        }

//        public async Task<ResponseQuery<string>> UploadIO(IOsValidationDTO iosValidation, ResponseQuery<string> response)
//        {
//            try
//            {
//                int totalFiles = files.Count;
//                int processedFiles = 0;

//                for (var i = 0; i < files.Count; i++)
//                {
//                    var file = files[i];
//                    int month = months[i];
//                    int year = years[i];

//                    if (file == null || file.Length == 0)
//                    {
//                        response.Message = "Archivo(s) vacío(s).";
//                        return response;
//                    }

//                    var tempPath = Path.Combine(_env.WebRootPath, "uploads", file.FileName);
//                    await DeleteDuplicateIO(file.FileName);

//                    using (var stream = new FileStream(tempPath, FileMode.Create))
//                    {
//                        await file.CopyToAsync(stream);
//                    }

//                    using (var memoryStream = new MemoryStream(await System.IO.File.ReadAllBytesAsync(tempPath)))
//                    {
//                        await _hub.Clients.All.SendAsync("AwaitFiles", true);
//                        await ProcessExcelIOTrafo(memoryStream, month, year, file.FileName);
//                        await ProcessExcelIORs(memoryStream, month, year, file.FileName);
//                    }

//                    System.IO.File.Delete(tempPath);
//                    await _hub.Clients.All.SendAsync("AwaitFiles", true);
//                    processedFiles++;
//                    int percentProgress = (int)Math.Truncate((double)processedFiles / totalFiles * 100);
//                    await _hub.Clients.All.SendAsync("ProgressFiles", "IO", processedFiles, percentProgress);
//                }

//                response.Message = "Archivos procesados correctamente.";
//                return response;
//            }
//            catch (FormatException ex)
//            {
//                response.Message = ex.Message;
//                return response;
//            }
//            catch (OracleException ex)
//            {
//                response.Message = ex.Message;
//                return response;
//            }
//            catch (Exception ex)
//            {
//                response.Message = ex.Message;
//                return response;
//            }
//        }

//        private async Task ProcessExcelIOTrafo(Stream excelStream, int month, int year, string fileName)
//        {
//            var ioList = new List<FileIoDTO>();
//            CultureInfo esCulture = new CultureInfo("es-CO");
//            string[] dateFormats = { "dd/MM/yyyy HH:mm:ss", "d/MM/yyyy hh:mm:ss tt" };
//            string[] timeFormats = {
//                "HH:mm:ss", "H:mm:ss", "hh:mm:ss tt", "h:mm:ss tt",
//                "dd/MM/yyyy HH:mm:ss", "d/MM/yyyy hh:mm:ss tt",
//                "dd/MM/yyyy H:mm:ss", "d/MM/yyyy h:mm:ss tt",
//                "dd/M/yyyy HH:mm:ss", "d/M/yyyy hh:mm:ss tt",
//                "dd/M/yyyy H:mm:ss", "d/M/yyyy h:mm:ss tt",
//                "d/MM/yyyy HH:mm:ss", "d/MM/yyyy H:mm:ss",
//                "d/M/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss"
//            };

//            int currentRow = 0; // Contador de fila
//            try
//            {
//                using (var workbook = new XLWorkbook(excelStream))
//                {
//                    foreach (IXLWorksheet worksheet in workbook.Worksheets)
//                    {
//                        if (worksheet.Name.ToUpper().Contains("TRAF"))
//                        {
//                            var rows = worksheet.RowsUsed(r => r.RowNumber() >= 6);

//                            foreach (var row in rows)
//                            {
//                                currentRow++; // Incrementar el número de fila
//                                var newIO = new FilesIo
//                                {
//                                    TypeAsset = "TRANSFORMADOR",
//                                    Element = "NO DATA",
//                                    Component = "NO DATA",
//                                    DnaKwh = -1,
//                                    FileIo = fileName,
//                                    CodeSig = ConvertString(row.Cell("B").GetString()),
//                                    Fparent = row.Cell("C").GetString(),
//                                    Year = year,
//                                    Month = month,
//                                    Failure = 1
//                                };


//                                if (
//                                    string.IsNullOrEmpty(row.Cell("A").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("B").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("C").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("G").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("H").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("I").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("M").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("N").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("P").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("Q").GetString()) ||
//                                    string.IsNullOrEmpty(row.Cell("S").GetString()))
//                                {
//                                    await _hub.Clients.All.SendAsync("Error", $"Uno o más campos en la fila {currentRow} son vacíos o nulos. La carga no puede continuar.");
//                                    return;
//                                }


//                                // Procesamiento de la fecha
//                                DateTime parsedDateIo;
//                                if (DateTime.TryParseExact(row.Cell("A").GetString(), dateFormats, esCulture, DateTimeStyles.None, out parsedDateIo))
//                                {
//                                    newIO.DateIo = parsedDateIo;
//                                }
//                                else
//                                {
//                                    newIO.DateIo = null;
//                                }

//                                // Procesamiento de la hora de salida
//                                string hourOutStr = row.Cell("G").GetString();
//                                DateTime parsedHourOut;
//                                if (DateTime.TryParseExact(row.Cell("G").GetString(), timeFormats, esCulture, DateTimeStyles.None, out parsedHourOut))
//                                {
//                                    if (newIO.DateIo.HasValue)
//                                    {
//                                        newIO.HourOut = new DateTime(newIO.DateIo.Value.Year, newIO.DateIo.Value.Month, newIO.DateIo.Value.Day,
//                                                                     parsedHourOut.Hour, parsedHourOut.Minute, parsedHourOut.Second);
//                                    }
//                                }
//                                else
//                                {
//                                    newIO.HourOut = null;
//                                }

//                                // Procesamiento de la hora de entrada
//                                string hourInStr = row.Cell("H").GetString();
//                                DateTime parsedHourIn;
//                                if (DateTime.TryParseExact(hourInStr, timeFormats, esCulture, DateTimeStyles.None, out parsedHourIn))
//                                {
//                                    if (newIO.DateIo.HasValue)
//                                    {
//                                        newIO.HourIn = new DateTime(newIO.DateIo.Value.Year, newIO.DateIo.Value.Month, newIO.DateIo.Value.Day,
//                                                                    parsedHourIn.Hour, parsedHourIn.Minute, parsedHourIn.Second);
//                                    }
//                                }
//                                else
//                                {
//                                    newIO.HourIn = null;
//                                }

//                                // Procesamiento de otros campos
//                                if (!float.TryParse(row.Cell("I").GetString(), out float minInterruption))
//                                {
//                                    minInterruption = -1;
//                                }
//                                newIO.MinInterruption = minInterruption;



//                                if (!int.TryParse(row.Cell("M").GetString(), out int cause))
//                                {
//                                    cause = -1;
//                                }
//                                newIO.Cause = cause;

//                                if (!int.TryParse(row.Cell("N").GetString(), out int cregCause))
//                                {
//                                    cregCause = -1;
//                                }
//                                newIO.CregCause = cregCause;

//                                newIO.EventType = row.Cell("P").GetString();
//                                newIO.Dependence = row.Cell("Q").GetString();

//                                if (!int.TryParse(row.Cell("S").GetString(), out int users))
//                                {
//                                    users = -1;
//                                }
//                                newIO.Users = users;

//                                newIO.Maneuver = row.Cell("R").GetString();

//                                if (!Regex.IsMatch(newIO.Maneuver, @"\d"))
//                                {
//                                    newIO.Maneuver = "NO DATA";
//                                }
//                                newIO.FileIo = fileName;

//                                if (newIO.MinInterruption == 0 || newIO.MinInterruption == 1440 || string.IsNullOrEmpty(hourInStr) || string.IsNullOrEmpty(hourOutStr))
//                                {
//                                    newIO.MinInterruption = -1;
//                                    newIO.HourInterruption = -1f;
//                                }
//                                else
//                                {
//                                    newIO.HourInterruption = (float)Math.Round(newIO.MinInterruption / 60, 2);
//                                }

//                                ioList.Add(newIO);
//                            }
//                        }
//                    }
//                }
//                await ProcessIo(ioList);
//            }
//            catch (FormatException ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"FormatException: {ex.Message}");
//            }
//            catch (OracleException ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"Exception: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"Exception: {ex.Message}");
//            }
//        }

//        private async Task ProcessExcelIORs(Stream excelStream, int month, int year, string fileName)
//        {
//            var ioList = new List<FileIoDTO>();
//            CultureInfo esCulture = new CultureInfo("es-CO");
//            string[] dateFormats = { "dd/MM/yyyy HH:mm:ss", "d/MM/yyyy hh:mm:ss tt" };
//            string[] timeFormats = {
//                "HH:mm:ss", "H:mm:ss", "hh:mm:ss tt", "h:mm:ss tt",
//                "dd/MM/yyyy HH:mm:ss", "d/MM/yyyy hh:mm:ss tt",
//                "dd/MM/yyyy H:mm:ss", "d/MM/yyyy h:mm:ss tt",
//                "dd/M/yyyy HH:mm:ss", "d/M/yyyy hh:mm:ss tt",
//                "dd/M/yyyy H:mm:ss", "d/M/yyyy h:mm:ss tt",
//                "d/MM/yyyy HH:mm:ss", "d/MM/yyyy H:mm:ss",
//                "d/M/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss"
//            };

//            int currentRow = 0;
//            try
//            {
//                using (var workbook = new XLWorkbook(excelStream))
//                {
//                    foreach (IXLWorksheet worksheet in workbook.Worksheets)
//                    {
//                        if (worksheet.Name.ToUpper().Contains("PAR") && !worksheet.Name.ToUpper().Contains("CIR-PAR"))
//                        {
//                            var rows = worksheet.RowsUsed(r => r.RowNumber() >= 6);

//                            foreach (var row in rows)
//                            {
//                                currentRow++;
//                                if (string.IsNullOrEmpty(row.Cell("A").GetString().ToUpper()) ||
//                                     string.IsNullOrEmpty(row.Cell("B").GetString().ToUpper()) ||
//                                     string.IsNullOrEmpty(row.Cell("D").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("E").GetString().ToUpper()) ||
//                                     string.IsNullOrEmpty(row.Cell("F").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("H").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("I").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("J").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("L").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("M").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("O").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("S").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("T").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("V").GetString().Trim()) ||
//                                     string.IsNullOrEmpty(row.Cell("W").GetString().Trim()))
//                                {
//                                    await _hub.Clients.All.SendAsync("Error", $"Uno o más campos en la fila {currentRow} son vacíos o nulos. La carga no puede continuar.");
//                                    return;
//                                }


//                                string element = row.Cell("E").GetString().ToUpper();
//                                if (!(element.Contains("INTERRUPTOR") || element.Contains("SECCIONADOR") || element.Contains("RECONECTADOR")))
//                                {
//                                    continue;
//                                }

//                                string codeSig = row.Cell("B").GetString().ToUpper();
//                                if (string.IsNullOrEmpty(codeSig) ||
//                                    (element.Contains("SECCIONADOR") || element.Contains("RECONECTADOR")) && codeSig.Equals("N.A."))
//                                {
//                                    continue;
//                                }

//                                string dateStr = row.Cell("A").GetString().ToUpper();
//                                if (string.IsNullOrEmpty(dateStr) || dateStr.Contains("N.A."))
//                                {
//                                    continue;
//                                }

//                                string hourOutStr = row.Cell("H").GetString().Trim();
//                                string hourInStr = row.Cell("I").GetString().Trim();

//                                if (!IsValidTime(hourOutStr, timeFormats) || !IsValidTime(hourInStr, timeFormats))
//                                {
//                                    continue;
//                                }

//                                string cregCauseStr = row.Cell("M").GetString().Trim();
//                                if (string.IsNullOrEmpty(cregCauseStr) || cregCauseStr.Contains("N.A."))
//                                {
//                                    continue;
//                                }

//                                var newIO = new FileIoDTO
//                                {
//                                    CodeSig = codeSig,
//                                    Element = element,
//                                    Fparent = row.Cell("D").GetString(),
//                                    Component = row.Cell("F").GetString(),
//                                    EventType = row.Cell("S").GetString(),
//                                    Dependence = row.Cell("T").GetString(),
//                                    Maneuver = row.Cell("O").GetString(),
//                                    FileIo = fileName,
//                                    Year = year,
//                                    Month = month,
//                                    Failure = 1
//                                };

//                                DateTime parsedDateIo;
//                                if (DateTime.TryParseExact(dateStr, dateFormats, esCulture, DateTimeStyles.None, out parsedDateIo))
//                                {
//                                    newIO.DateIo = parsedDateIo;
//                                }
//                                else
//                                {
//                                    newIO.DateIo = null;
//                                }

//                                if (!float.TryParse(row.Cell("J").GetString(), out float minInterruption))
//                                {
//                                    minInterruption = -1;
//                                }
//                                newIO.MinInterruption = minInterruption;


//                                if (!int.TryParse(row.Cell("L").GetString(), out int cause))
//                                {
//                                    cause = -1;
//                                }
//                                newIO.Cause = cause;


//                                if (!int.TryParse(cregCauseStr, out int cregCause))
//                                {
//                                    cregCause = -1;
//                                }
//                                newIO.CregCause = cregCause;


//                                if (!int.TryParse(row.Cell("W").GetString(), out int users))
//                                {
//                                    users = -1;
//                                }
//                                newIO.Users = users;

//                                if (!float.TryParse(row.Cell("V").GetString(), out float dnaKwh))
//                                {
//                                    dnaKwh = -1;
//                                }
//                                newIO.DnaKwh = dnaKwh;

//                                if (!Regex.IsMatch(newIO.Maneuver, @"\d"))
//                                {
//                                    newIO.Maneuver = "NO DATA";
//                                }

//                                if (newIO.CodeSig.StartsWith("S")) newIO.TypeAsset = "SWITCH";
//                                else if (newIO.CodeSig.StartsWith("R")) newIO.TypeAsset = "RECONECTADOR";

//                                DateTime parsedHourOut;
//                                if (DateTime.TryParseExact(hourOutStr, timeFormats, esCulture, DateTimeStyles.None, out parsedHourOut))
//                                {
//                                    if (newIO.DateIo.HasValue)
//                                    {
//                                        newIO.HourOut = new DateTime(newIO.DateIo.Value.Year, newIO.DateIo.Value.Month, newIO.DateIo.Value.Day,
//                                                                     parsedHourOut.Hour, parsedHourOut.Minute, parsedHourOut.Second);
//                                    }
//                                    else
//                                    {
//                                        newIO.HourOut = parsedHourOut;
//                                    }
//                                }
//                                else
//                                {
//                                    newIO.HourOut = null;
//                                }

//                                DateTime parsedHourIn;
//                                if (DateTime.TryParseExact(hourInStr, timeFormats, esCulture, DateTimeStyles.None, out parsedHourIn))
//                                {
//                                    if (newIO.DateIo.HasValue)
//                                    {
//                                        newIO.HourIn = new DateTime(newIO.DateIo.Value.Year, newIO.DateIo.Value.Month, newIO.DateIo.Value.Day,
//                                                                    parsedHourIn.Hour, parsedHourIn.Minute, parsedHourIn.Second);
//                                    }
//                                    else
//                                    {
//                                        newIO.HourIn = parsedHourIn;
//                                    }
//                                }
//                                else
//                                {
//                                    newIO.HourIn = null;
//                                }

//                                ioList.Add(newIO);
//                            }
//                        }
//                    }
//                }
//                await ProcessIo(ioList);
//            }
//            catch (FormatException ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"FormatException: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"Exception: {ex.Message}");
//            }
//        }

//        static string ConvertString(string input)
//        {

//            if (int.TryParse(input, out int number))
//            {

//                if (number < 1000)
//                {

//                    return number.ToString("0000");
//                }
//                else
//                {
//                    return input;
//                }
//            }
//            else
//            {
//                return input;
//            }
//        }

//        static bool IsValidTime(string timeStr, string[] formats)
//        {
//            CultureInfo esCulture = new CultureInfo("es-CO");
//            DateTime parsedTime;
//            if (!string.IsNullOrEmpty(timeStr) &&
//                DateTime.TryParseExact(timeStr, formats, esCulture, DateTimeStyles.None, out parsedTime) &&
//                !(parsedTime.Hour == 0 && parsedTime.Minute == 0 && parsedTime.Second == 0))
//            {
//                return true; // Es un tiempo válido y no es medianoche
//            }
//            return false; // No es un tiempo válido o es medianoche
//        }

//        private async Task ProcessIo(List<FileIoDTO> dataList)
//        {
//            try
//            {

//                int totalRows = dataList.Count;
//                int processedRows = 0;
//                int batch = 500;
//                int percentProgress = 0;

//                foreach (var ioTrafo in dataList)
//                {
//                    _context.FilesIos.Add(ioTrafo);
//                    if (processedRows % batch == 0)
//                    {
//                        await _context.SaveChangesAsync();
//                    }
//                    processedRows++;
//                    percentProgress = (int)Math.Truncate((double)processedRows / totalRows * 100);
//                    await _hub.Clients.All.SendAsync("ProgressData", "IO", processedRows, percentProgress);
//                }
//                await _context.SaveChangesAsync();
//            }
//            catch (FormatException ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"FormatException: {ex.Message}");
//            }
//            catch (OracleException ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"OracleException: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                Json(new { ErrorMessage = ex.Message, Success = false });
//                await _hub.Clients.All.SendAsync("Error", $"Exception: {ex.Message}");
//            }
//        }

//        private async Task DeleteDuplicateIO(string fileName)
//        {

//            await fileIODataAccess.DeleteData(fileName);

//        }

//        private DateOnly ParseDate(string dateString)
//        {
//            foreach (var format in _timeFormats)
//            {
//                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
//                {
//                    return parsedDate;
//                }
//            }
//            return DateOnly.Parse("31/12/2099");
//        }

//    }
//}
