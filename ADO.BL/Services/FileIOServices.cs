using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ADO.BL.Services
{
    public class FileIOServices : IFileIOServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _IOsDirectoryPath;
        private readonly IFileIODataAccess fileIODataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        public FileIOServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IFileIODataAccess _fileIODataAccess)
        {
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _IOsDirectoryPath = configuration["IOsPath"];
            fileIODataAccess = _fileIODataAccess;
            statusFileDataAccess = _statuFileDataAccess;
        }

        public async Task<ResponseQuery<string>> UploadIO(IOsValidationDTO iosValidation, ResponseQuery<string> response)
        {
            try
            {
                string inputFolder = _IOsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var count = 0;
                        var index1 = 0;
                        var index2 = 0;
                        foreach (var item in package.Workbook.Worksheets)
                        {
                            if (item.Name.Contains("PAR") && !item.Name.Contains("CIR-PAR"))
                            {
                                index1 = count;
                            }
                            else if (item.Name.Contains("TRAF"))
                            {
                                index2 = count;
                            }
                            count++;
                        }

                        var worksheet1 = package.Workbook.Worksheets[index1];                        
                        var worksheet2 = package.Workbook.Worksheets[index2];
                        var dataTableError = new DataTable();                        
                        var ioList = new List<FileIoDTO>();
                        var ioCompleteList = new List<FileIoCompleteDTO>();

                        var statusFilesingle = new StatusFileDTO();

                        // Extraer el nombre del archivo sin la extensión
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        var resultYearMonth = getYearMonth(worksheet1);                        
                        int year = 2099;
                        int month = 12;                        
                        if (resultYearMonth.Count > 0)
                        {
                            year = int.Parse(resultYearMonth[0]);
                            month = int.Parse(resultYearMonth[1]);                            
                        }

                        statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                        statusFilesingle.UserId = iosValidation.UserId;
                        statusFilesingle.FileName = fileName;
                        statusFilesingle.FileType = "IO";
                        statusFilesingle.Year = year;
                        statusFilesingle.Month = month;
                        statusFilesingle.Day = -1;
                        statusFilesingle.Status = 1;

                        statusFileList.Add(statusFilesingle);

                        CultureInfo esCulture = new CultureInfo("es-CO");
                        string[] dateFormats = { "dd/MM/yyyy HH:mm:ss", "d/MM/yyyy hh:mm:ss tt" };
                        string[] timeFormats = {
                            "HH:mm:ss", "H:mm:ss", "hh:mm:ss tt", "h:mm:ss tt", "HH:mm:ss.fff",
                            "dd/MM/yyyy HH:mm:ss", "d/MM/yyyy hh:mm:ss tt",
                            "dd/MM/yyyy H:mm:ss", "d/MM/yyyy h:mm:ss tt",
                            "dd/M/yyyy HH:mm:ss", "d/M/yyyy hh:mm:ss tt",
                            "dd/M/yyyy H:mm:ss", "d/M/yyyy h:mm:ss tt",
                            "d/MM/yyyy HH:mm:ss", "d/MM/yyyy H:mm:ss",
                            "d/M/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss"
                        };
                        
                        await DeleteDuplicateIO(fileName);

                        // columnas tabla error
                        dataTableError.Columns.Add("C1");

                        for (int row = 6; row <= worksheet1.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 24; i++)
                            {
                                if (worksheet1.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 24)
                            {
                                break;
                            }

                            var date = worksheet1.Cells[row, 1].Text != "" ? worksheet1.Cells[row, 1].Text : string.Empty;
                            if (string.IsNullOrEmpty(worksheet1.Cells[row, 1].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 2].Text) ||
                                string.IsNullOrEmpty(worksheet1.Cells[row, 4].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 5].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 6].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 8].Text) ||
                                string.IsNullOrEmpty(worksheet1.Cells[row, 9].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 10].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 11].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 12].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 13].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 15].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 19].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 20].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 22].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 23].Text))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data en la línea {row}, hay uno o más campos vacíos y estos son Requeridos";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else if (date != "")
                            {
                                var date2 = ParseDate(date);
                                if (date2 == DateOnly.Parse("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $" En la línea {row} y columna Fecha";
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }                                
                                    string element = worksheet1.Cells[row, 5].Text.ToUpper();
                                    string codeSig = worksheet1.Cells[row, 2].Text.ToUpper();
                                if (!(element.Contains("INTERRUPTOR") || element.Contains("SECCIONADOR") || element.Contains("RECONECTADOR")))
                                {
                                    continue;
                                }

                                if (string.IsNullOrEmpty(codeSig) ||
                                    (element.Contains("SECCIONADOR") || element.Contains("RECONECTADOR")) && codeSig.Equals("N.A."))
                                {
                                    continue;
                                }

                                string hourOutStr = worksheet1.Cells[row, 8].Text.Trim();
                                string hourInStr = worksheet1.Cells[row, 9].Text.Trim();
                                string cregCauseStr = worksheet1.Cells[row, 13].Text.Trim();

                                if (!IsValidTime(hourOutStr, timeFormats) || !IsValidTime(hourInStr, timeFormats))
                                {
                                    continue;
                                }

                                if (string.IsNullOrEmpty(cregCauseStr) || cregCauseStr.Contains("N.A."))
                                {
                                    continue;
                                }

                                var newEntity = new FileIoDTO();
                                var newEntityComplete = new FileIoCompleteDTO();

                                //tabla files_io
                                #region tabla io
                                var aniomes = worksheet1.Cells[row, 1].Text.Split('/', ' ');
                                newEntity.CodeSig = codeSig;
                                newEntity.Element = element;
                                newEntity.Fparent = worksheet1.Cells[row, 4].Text.Trim().Replace(" ", "");
                                newEntity.TypeAsset = element;
                                newEntity.Component = worksheet1.Cells[row, 6].Text.Trim();
                                newEntity.EventType = worksheet1.Cells[row, 19].Text.Trim();
                                newEntity.Dependence = worksheet1.Cells[row, 20].Text.Trim();
                                newEntity.Maneuver = worksheet1.Cells[row, 16].Text.Trim();
                                newEntity.FileIo = fileName;
                                newEntity.Year = int.Parse(aniomes[2]);
                                newEntity.Month = int.Parse(aniomes[1]);
                                newEntity.Failure = 1;
                                newEntity.DateIo = ParseDate(worksheet1.Cells[row, 1].Text);                                                        


                                if (!float.TryParse(worksheet1.Cells[row, 10].Text, out float minInterruption))
                                {
                                    minInterruption = -1;
                                }
                                newEntity.MinInterruption = minInterruption;


                                if (!int.TryParse(worksheet1.Cells[row, 12].Text, out int cause))
                                {
                                    cause = -1;
                                }
                                newEntity.Cause = cause;


                                if (!int.TryParse(cregCauseStr, out int cregCause))
                                {
                                    cregCause = -1;
                                }
                                newEntity.CregCause = cregCause;


                                if (!int.TryParse(worksheet1.Cells[row, 23].Text, out int users))
                                {
                                    users = -1;
                                }
                                newEntity.Users = users;

                                if (!float.TryParse(worksheet1.Cells[row, 22].Text, out float dnaKwh))
                                {
                                    dnaKwh = -1;
                                }
                                newEntity.DnaKwh = dnaKwh;

                                if (!Regex.IsMatch(newEntity.Maneuver, @"\d"))
                                {
                                    newEntity.Maneuver = "NO DATA";
                                }

                                if (newEntity.CodeSig.StartsWith("S")) newEntity.TypeAsset = "SWITCH";
                                else if (newEntity.CodeSig.StartsWith("R")) newEntity.TypeAsset = "RECONECTADOR";


                                newEntity.HourOut = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 8].Text}");

                                newEntity.HourIn = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 9].Text}");

                                #endregion

                                //tabla complete
                                #region complete
                                newEntityComplete.DateIo = ParseDate(worksheet1.Cells[row, 1].Text);
                                newEntityComplete.CodeGis = codeSig;

                                string textoLoc = worksheet1.Cells[row, 3].Text.Trim().ToUpper();
                                int longitudLoc = Math.Min(50, textoLoc.Length);
                                newEntityComplete.Location = string.IsNullOrEmpty(textoLoc) ? "-1" : textoLoc.Substring(0, longitudLoc);
                                                        
                                newEntityComplete.Ubication = worksheet1.Cells[row, 4].Text.Trim().ToUpper();
                                newEntityComplete.Element = element;
                                newEntityComplete.Component = worksheet1.Cells[row, 6].Text.Trim().ToUpper();                                                        

                                string texto = worksheet1.Cells[row, 7].Text.Trim().ToUpper();
                                int longitud = Math.Min(2048, texto.Length);                                                        
                                //newEntityComplete.AffectedSector = string.IsNullOrEmpty(texto) ? "-1" : texto.Substring(0, longitud);

                                newEntityComplete.HourOut = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 8].Text}");
                                newEntityComplete.HourIn = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 9].Text}");
                                if (!float.TryParse(worksheet1.Cells[row, 10].Text, out float minInterruptionComplete))
                                {
                                    minInterruptionComplete = -1;
                                }
                                newEntityComplete.MinInterruption = minInterruptionComplete;

                                string textoDesc = worksheet1.Cells[row, 11].Text.Trim().ToUpper();
                                int longitudDesc = Math.Min(50, textoDesc.Length);
                                newEntityComplete.DescCause = string.IsNullOrEmpty(textoDesc) ? "-1" : textoDesc.Substring(0, longitudDesc);
                                                        
                                if (!int.TryParse(worksheet1.Cells[row, 12].Text, out int CodCauseEepComplete))
                                {
                                    CodCauseEepComplete = -1;
                                }
                                newEntityComplete.CodCauseEvent = CodCauseEepComplete;
                                if (!int.TryParse(worksheet1.Cells[row, 13].Text, out int CodCauseComplete))
                                {
                                    CodCauseComplete = -1;
                                }
                                newEntityComplete.Cause = CodCauseComplete;                                                        

                                string texto2 = worksheet1.Cells[row, 14].Text.Trim().ToUpper();
                                int longitud2 = Math.Min(2048, texto2.Length);
                                //newEntityComplete.Observation = string.IsNullOrEmpty(texto2) ? "-1" : texto2.Substring(0, longitud2);

                                newEntityComplete.Maneuver = worksheet1.Cells[row, 15].Text.Trim().ToUpper();
                                newEntityComplete.FuseQuant = string.IsNullOrEmpty(worksheet1.Cells[row, 16].Text) ? "-1" : worksheet1.Cells[row, 16].Text.Trim().ToUpper();
                                newEntityComplete.FuseCap = string.IsNullOrEmpty(worksheet1.Cells[row, 17].Text) ? "-1" : worksheet1.Cells[row, 17].Text.Trim().ToUpper();
                                if (!int.TryParse(worksheet1.Cells[row, 18].Text, out int CodConsigComplete))
                                {
                                    CodConsigComplete = -1;
                                }
                                newEntityComplete.CodeConsig = CodConsigComplete;
                                newEntityComplete.TypeEvent = worksheet1.Cells[row, 19].Text.Trim().ToUpper();
                                newEntityComplete.Dependency = worksheet1.Cells[row, 20].Text.Trim().ToUpper();
                                if (!float.TryParse(worksheet1.Cells[row, 21].Text, out float outPowerComplete))
                                {
                                    outPowerComplete = -1;
                                }
                                newEntityComplete.OutPower = outPowerComplete;
                                if (!float.TryParse(worksheet1.Cells[row, 22].Text, out float DnaComplete))
                                {
                                    DnaComplete = -1;
                                }
                                newEntityComplete.DnaKwh = DnaComplete;
                                if (!int.TryParse(worksheet1.Cells[row, 23].Text, out int usersComplete))
                                {
                                    usersComplete = -1;
                                }
                                newEntityComplete.Users = usersComplete;
                                newEntityComplete.ApplicationId = string.IsNullOrEmpty(worksheet1.Cells[row, 24].Text) ? "-1" : worksheet1.Cells[row, 24].Text.Trim().ToUpper();


                                #endregion
                                ioList.Add(newEntity);
                                ioCompleteList.Add(newEntityComplete);
                             
                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} y columna Fecha, la fecha no puede ser nula";
                                dataTableError.Rows.Add(newRowError);
                            }

                        }

                        for (int row = 6; row <= worksheet2.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 20; i++)
                            {
                                if (worksheet2.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 20)
                            {
                                break;
                            }                            

                            var date = worksheet2.Cells[row, 1].Text != "" ? worksheet2.Cells[row, 1].Text : string.Empty;
                            if (worksheet2.Cells[row, 1].Text == "" || worksheet2.Cells[row, 2].Text == "" || worksheet2.Cells[row, 3].Text == "" ||
                                worksheet2.Cells[row, 7].Text == "" || worksheet2.Cells[row, 8].Text == "" || worksheet2.Cells[row, 9].Text == "" ||
                                worksheet2.Cells[row, 13].Text == "" || worksheet2.Cells[row, 14].Text == "" || worksheet2.Cells[row, 16].Text == "" || 
                                worksheet2.Cells[row, 17].Text == "" || worksheet2.Cells[row, 19].Text == "")
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data en la línea {row}, uno o más valores están vacíos y estos son Requeridos";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else if (date != "")
                            {
                                var date2 = ParseDate(date);
                                if (date2 == DateOnly.Parse("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $" En la línea {row} y columna Fecha";
                                    dataTableError.Rows.Add(newRowError);
                                }
                                else
                                {
                                    string hourOutStr2 = worksheet2.Cells[row, 8].Text.Trim();
                                    string hourInStr2 = worksheet2.Cells[row, 9].Text.Trim();
                                
                                    if (!IsValidTime(hourOutStr2, timeFormats) || !IsValidTime(hourInStr2, timeFormats))
                                    {
                                        continue;
                                    }

                                    // tablas io
                                    #region tabla io
                                    var newEntity = new FileIoDTO();
                                        
                                    var aniomes2 = worksheet2.Cells[row, 1].Text.Split('/', ' ');
                                    newEntity.CodeSig = ConvertString(worksheet2.Cells[row, 2].Text.Trim());
                                    newEntity.Fparent = worksheet2.Cells[row, 3].Text.Trim().Replace(" ", "");                                        
                                    newEntity.FileIo = fileName;
                                    newEntity.DnaKwh = -1;
                                    newEntity.Component = "NO DATA";
                                    newEntity.Element = "NO DATA";
                                    newEntity.TypeAsset = "TRANSFORMADOR";
                                    newEntity.Year = int.Parse(aniomes2[2]);
                                    newEntity.Month = int.Parse(aniomes2[1]);
                                    newEntity.DateIo = ParseDate(worksheet2.Cells[row, 1].Text);

                                    newEntity.HourOut = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 7].Text}");

                                    newEntity.HourIn = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 8].Text}");


                                    // Procesamiento de otros campos
                                    if (!float.TryParse(worksheet2.Cells[row, 9].Text, out float minInterruption))
                                    {
                                        minInterruption = -1;
                                    }
                                    newEntity.MinInterruption = minInterruption;



                                    if (!int.TryParse(worksheet2.Cells[row, 13].Text, out int cause))
                                    {
                                        cause = -1;
                                    }
                                    newEntity.Cause = cause;

                                    if (!int.TryParse(worksheet2.Cells[row, 14].Text, out int cregCause))
                                    {
                                        cregCause = -1;
                                    }
                                    newEntity.CregCause = cregCause;

                                    newEntity.EventType = worksheet2.Cells[row, 16].Text;
                                    newEntity.Dependence = worksheet2.Cells[row, 17].Text;

                                    if (!int.TryParse(worksheet2.Cells[row, 19].Text, out int users))
                                    {
                                        users = -1;
                                    }
                                    newEntity.Users = users;

                                    newEntity.Maneuver = worksheet2.Cells[row, 18].Text;

                                    if (!Regex.IsMatch(newEntity.Maneuver, @"\d"))
                                    {
                                        newEntity.Maneuver = "NO DATA";
                                    }
                                        

                                    if (newEntity.MinInterruption == 0 || newEntity.MinInterruption == 1440 || string.IsNullOrEmpty(hourInStr2) || string.IsNullOrEmpty(hourOutStr2))
                                    {
                                        newEntity.MinInterruption = -1;
                                        newEntity.HourInterruption = -1f;
                                    }
                                    else
                                    {
                                        newEntity.HourInterruption = (float)Math.Round(newEntity.MinInterruption / 60, 2);
                                    }

                                    #endregion

                                    #region complete
                                    var newEntityComplete = new FileIoCompleteDTO();

                                    newEntityComplete.DateIo = ParseDate(worksheet2.Cells[row, 1].Text);
                                    newEntityComplete.CodeGis = ConvertString(worksheet2.Cells[row, 2].Text.Trim());
                                    newEntityComplete.Ubication = worksheet2.Cells[row, 3].Text.Trim().ToUpper();
                                    if (!float.TryParse(worksheet2.Cells[row, 4].Text, out float capacityKVAComplete))
                                    {
                                        capacityKVAComplete = -1;
                                    }
                                    newEntityComplete.CapacityKva = capacityKVAComplete;

                                    string textoLocation = worksheet2.Cells[row, 5].Text.Trim().ToUpper();
                                    int longitudLocation = Math.Min(50, textoLocation.Length);
                                    newEntityComplete.Location = string.IsNullOrEmpty(textoLocation) ? "-1" : textoLocation.Substring(0, longitudLocation);

                                    newEntityComplete.Type = worksheet2.Cells[row, 6].Text.Trim().ToUpper();
                                    newEntityComplete.HourOut = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 7].Text}");
                                    newEntityComplete.HourIn = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 8].Text}");
                                    if (!float.TryParse(worksheet2.Cells[row, 10].Text, out float minInterruptionComplete))
                                    {
                                        minInterruptionComplete = -1;
                                    }
                                    newEntityComplete.MinInterruption = minInterruptionComplete;
                                        

                                    string texto2 = worksheet2.Cells[row, 10].Text.Trim().ToUpper();
                                    int longitud2 = Math.Min(2048, texto2.Length);
                                    //newEntityComplete.Observation = string.IsNullOrEmpty(texto2) ? "-1" : texto2.Substring(0, longitud2);

                                    newEntityComplete.Ownership = worksheet2.Cells[row, 11].Text.Trim().ToUpper();

                                    string textoDesc = worksheet2.Cells[row, 12].Text.Trim().ToUpper();
                                    int longitudDesc = Math.Min(50, textoDesc.Length);
                                    newEntityComplete.DescCause = string.IsNullOrEmpty(textoDesc) ? "-1" : textoDesc.Substring(0, longitudDesc);
                                        
                                    if (!int.TryParse(worksheet2.Cells[row, 13].Text, out int CodCauseEepComplete))
                                    {
                                        CodCauseEepComplete = -1;
                                    }
                                    newEntityComplete.CodCauseEvent = CodCauseEepComplete;
                                    if (!int.TryParse(worksheet2.Cells[row, 14].Text, out int CodCauseComplete))
                                    {
                                        CodCauseComplete = -1;
                                    }
                                    newEntityComplete.Cause = CodCauseComplete;
                                    if (!int.TryParse(worksheet2.Cells[row, 15].Text, out int CodConsigComplete))
                                    {
                                        CodConsigComplete = -1;
                                    }
                                    newEntityComplete.CodeConsig = CodConsigComplete;
                                    newEntityComplete.TypeEvent = worksheet2.Cells[row, 16].Text.Trim().ToUpper();
                                    newEntityComplete.Dependency = worksheet2.Cells[row, 17].Text.Trim().ToUpper();
                                    newEntityComplete.Maneuver = worksheet2.Cells[row, 18].Text.Trim().ToUpper();
                                    if (!int.TryParse(worksheet2.Cells[row, 19].Text, out int usersComplete))
                                    {
                                        usersComplete = -1;
                                    }
                                    newEntityComplete.Users = usersComplete;
                                    newEntityComplete.ApplicationId = worksheet2.Cells[row, 20].Text.Trim().ToUpper();


                                    #endregion

                                    ioCompleteList.Add(newEntityComplete);
                                    ioList.Add(newEntity);
                                    

                                }

                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} y columna Fecha, la fecha no puede ser nula";
                                dataTableError.Rows.Add(newRowError);
                            }

                        }

                        if (dataTableError.Rows.Count > 0)
                        {
                            errorFlag = true;
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        if (ioList.Count > 0)
                        {
                            int i = 0;
                            while ((i * 10000) < ioList.Count())
                            {
                                var subgroup = ioList.Skip(i * 10000).Take(10000).ToList();
                                var EntityResult = mapper.Map<List<FilesIo>>(subgroup);
                                SaveData(EntityResult);
                                i++;
                                Console.WriteLine(i * 10000);
                            }
                            
                            //var subgroupMap = mapper.Map<List<StatusFile>>(statusFileList);
                            //var resultSave = await statusFileDataAccess.s(subgroupMap);

                        }

                        if (ioCompleteList.Count > 0)
                        {
                            int i = 0;
                            while ((i * 10000) < ioCompleteList.Count())
                            {
                                var subgroup = ioCompleteList.Skip(i * 10000).Take(10000).ToList();
                                var EntityResult = mapper.Map<List<FilesIoComplete>>(subgroup);
                                SaveDataComplete(EntityResult);
                                i++;
                                Console.WriteLine(i * 10000);
                            }

                        }
                    }

                }

                if (errorFlag)
                {
                    response.Message = "file with errors";
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
                    csv.NextRecord();
                }
            }
        }

        static string ConvertString(string input)
        {

            if (int.TryParse(input, out int number))
            {

                if (number < 1000)
                {

                    return number.ToString("0000");
                }
                else
                {
                    return input;
                }
            }
            else
            {
                return input;
            }
        }

        static bool IsValidTime(string timeStr, string[] formats)
        {
            CultureInfo esCulture = new CultureInfo("es-CO");
            DateTime parsedTime;
            if (!string.IsNullOrEmpty(timeStr) &&
                DateTime.TryParseExact(timeStr, formats, esCulture, DateTimeStyles.None, out parsedTime) &&
                !(parsedTime.Hour == 0 && parsedTime.Minute == 0 && parsedTime.Second == 0))
            {
                return true; // Es un tiempo válido y no es medianoche
            }
            return false; // No es un tiempo válido o es medianoche
        }

        private async Task SaveData(List<FilesIo> dataList)
        {
            await fileIODataAccess.SaveData(dataList);
        }

        private async Task SaveDataComplete(List<FilesIoComplete> dataList)
        {
            await fileIODataAccess.SaveDataComplete(dataList);
        }

        private async Task DeleteDuplicateIO(string fileName)
        {

            await fileIODataAccess.DeleteData(fileName);

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

        private List<string> getYearMonth(ExcelWorksheet worksheet1)
        {
            var yearMonth = new List<string>();
            for (int i = 6; i < worksheet1.Dimension.End.Row; i++)
            {
                if(!string.IsNullOrEmpty(worksheet1.Cells[i, 1].Text))
                {
                    if (string.IsNullOrEmpty(worksheet1.Cells[i, 1].Text))
                    {
                        continue;
                    }
                    var resultDate = ParseDate(worksheet1.Cells[i, 1].Text);
                    if (resultDate != DateOnly.Parse("31/12/2099"))
                    {
                        // formato fecha "dd/MM/YYYY"
                        var dateTemp = resultDate.ToString().Split('/', ' ');
                        yearMonth.Add(dateTemp[2]);
                        yearMonth.Add(dateTemp[1]);
                        break;
                    }
                }
            }
            return yearMonth;
        }
    }
}
