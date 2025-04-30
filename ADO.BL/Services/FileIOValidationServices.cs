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
    public class FileIOValidationServices : IFileIOValidationServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _IOsDirectoryPath;
        private readonly IFileIODataAccess fileIODataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IIoCommentsDataAccess ioCommentsDataAccess;

        private static readonly CultureInfo _spanishCultureOnly = new CultureInfo("es-CO"); // o "es-ES"

        public FileIOValidationServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IFileIODataAccess _fileIODataAccess,
            IIoCommentsDataAccess _ioCommentsDataAccess)
        {
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _IOsDirectoryPath = configuration["IOsPath"];
            fileIODataAccess = _fileIODataAccess;
            statusFileDataAccess = _statuFileDataAccess;
            ioCommentsDataAccess = _ioCommentsDataAccess;
        }

        public async Task<ResponseQuery<bool>> ReadFilesIos(IOsValidationDTO iosValidation, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _IOsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx").OrderBy(f => f).ToArray())
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
                        var dataTable = new DataTable();
                        var dataTableComplete = new DataTable();                        
                        var ioList = new List<FileIoDTO>();
                        var ioCompleteList = new List<FileIoCompleteDTO>();

                        var statusFilesingle = new StatusFileDTO();

                        // Extraer el nombre del archivo sin la extensión
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        // Obtener los primeros 4 dígitos como el año
                        int year = int.Parse(fileName.Substring(0, 4));

                        // Obtener los siguientes 2 dígitos como el mes
                        int month = int.Parse(fileName.Substring(4, 2));

                        // Obtener los siguientes 2 dígitos como el día
                        int day = int.Parse(fileName.Substring(6, 2));

                        statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                        statusFilesingle.UserId = iosValidation.UserId;
                        statusFilesingle.FileName = fileName;
                        statusFilesingle.FileType = "IO";
                        statusFilesingle.Year = year;
                        statusFilesingle.Month = month;
                        statusFilesingle.Day = day;
                        statusFilesingle.DateRegister = ParseDate($"{day}/{month}/{year}");

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

                        // columnas tablas para csv
                        dataTableError.Columns.Add("C1");

                        for (int i = 1; i <= 21; i++)
                        {
                            dataTable.Columns.Add($"C{i}");
                        }

                        for (int i = 1; i <= 28; i++)
                        {
                            dataTableComplete.Columns.Add($"C{i}");
                        }                        

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
                                newRowError[0] = $"Error en la data en la línea {row} de la hoja '{worksheet1}', hay uno o más campos vacíos y estos son Requeridos";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else if (date != "")
                            {
                                var date2 = ParseDate(date);
                                if (date2 == ParseDate("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $" En la línea {row} de la hoja '{worksheet1}' y columna Fecha";
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
                                
                                var newEntityComplete = new FileIoCompleteDTO();

                                //tabla files_io
                                #region tabla io

                                var newRow = dataTable.NewRow();
                                newRow[0] = ParseDate(worksheet1.Cells[row, 1].Text).ToString();
                                newRow[1] = codeSig;
                                newRow[2] = element;
                                var tempRow4 = Regex.Replace(worksheet1.Cells[row, 4].Text, @"\s+", " ");
                                newRow[3] = tempRow4.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "").ToString();
                                newRow[4] = element;
                                newRow[5] = worksheet1.Cells[row, 6].Text.Trim().ToUpper().ToString();                                
                                newRow[6] = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 8].Text}").ToString();
                                newRow[7] = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 9].Text}").ToString();

                                if (!float.TryParse(worksheet1.Cells[row, 10].Text, out float minInterruption))
                                {
                                    minInterruption = -1;
                                }
                                newRow[8] = minInterruption.ToString();
                                newRow[9] = minInterruption == -1 ? "-1" : (minInterruption/60).ToString();

                                if (!int.TryParse(cregCauseStr, out int cregCause))
                                {
                                    cregCause = -1;
                                }
                                newRow[10] = cregCause.ToString();

                                if (!int.TryParse(worksheet1.Cells[row, 12].Text, out int cause))
                                {
                                    cause = -1;
                                }
                                newRow[11] = cause;

                                newRow[12] = worksheet1.Cells[row, 19].Text.Trim().ToString();
                                newRow[13] = worksheet1.Cells[row, 20].Text.Trim().ToString();

                                if (!int.TryParse(worksheet1.Cells[row, 23].Text, out int users))
                                {
                                    users = -1;
                                }
                                newRow[14] = users.ToString();

                                if (!float.TryParse(worksheet1.Cells[row, 22].Text, out float dnaKwh))
                                {
                                    dnaKwh = -1;
                                }
                                newRow[15] = dnaKwh.ToString();
                                newRow[16] = "1";
                                newRow[17] = worksheet1.Cells[row, 16].Text.Trim().ToString();
                                newRow[18] = fileName;

                                var aniomes = worksheet1.Cells[row, 1].Text.Split('/', ' ');

                                newRow[19] = int.Parse(aniomes[2]).ToString();
                                newRow[20] = int.Parse(aniomes[1]).ToString();

                                if (!Regex.IsMatch(newRow[17].ToString(), @"\d"))
                                {
                                    newRow[17] = "NO DATA";
                                }

                                if (codeSig.StartsWith("S")) newRow[2] = "SWITCH";
                                else if (codeSig.StartsWith("R")) newRow[2] = "RECONECTADOR";

                                dataTable.Rows.Add(newRow);

                                #endregion

                                //tabla complete
                                #region complete

                                var newRowComplete = dataTableComplete.NewRow();
                                var ioCommentTemp = new IoComment();

                                ioCommentTemp.FileName = fileName;
                                ioCommentTemp.FileLine = row;
                                string texto = worksheet1.Cells[row, 7].Text.Trim().ToUpper();
                                int longitud = Math.Min(3072, texto.Length);
                                ioCommentTemp.AffectedSector = string.IsNullOrEmpty(texto) ? "-1" : texto.Substring(0, longitud);

                                string texto2 = worksheet1.Cells[row, 14].Text.Trim().ToUpper();
                                int longitud2 = Math.Min(3072, texto2.Length);
                                ioCommentTemp.Comment = string.IsNullOrEmpty(texto2) ? "-1" : texto2.Substring(0, longitud2);

                                var idResult = ioCommentsDataAccess.CreateRegister(ioCommentTemp);

                                newRowComplete[0] = ParseDate(worksheet1.Cells[row, 1].Text).ToString();
                                newRowComplete[1] = codeSig;

                                string textoLoc = worksheet1.Cells[row, 3].Text.Trim().ToUpper().ToString().Replace("\n"," ");
                                int longitudLoc = Math.Min(50, textoLoc.Length);
                                newRowComplete[2] = string.IsNullOrEmpty(textoLoc) ? "-1" : textoLoc.Substring(0, longitudLoc);

                                newRowComplete[3] = worksheet1.Cells[row, 4].Text.Replace("\n", "").Replace("\r", "").Replace(" ", "").Trim().ToUpper();
                                newRowComplete[4] = element;
                                newRowComplete[5] = worksheet1.Cells[row, 6].Text.Trim().ToUpper();
                                newRowComplete[6] = idResult.ToString(); // affectsector

                                newRowComplete[7] = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 8].Text}");
                                newRowComplete[8] = DateTime.Parse($"{worksheet1.Cells[row, 1].Text} {worksheet1.Cells[row, 9].Text}");
                                if (!float.TryParse(worksheet1.Cells[row, 10].Text, out float minInterruptionComplete))
                                {
                                    minInterruptionComplete = -1;
                                }
                                newRowComplete[9] = minInterruptionComplete;
                                newRowComplete[10] = minInterruptionComplete == -1 ? "-1" : (minInterruptionComplete/60).ToString();

                                string textoDesc = worksheet1.Cells[row, 11].Text.Trim().ToUpper().ToString().Replace("\n", " ");
                                int longitudDesc = Math.Min(50, textoDesc.Length);
                                newRowComplete[11] = string.IsNullOrEmpty(textoDesc) ? "-1" : textoDesc.Substring(0, longitudDesc);
                                                        
                                if (!int.TryParse(worksheet1.Cells[row, 12].Text, out int CodCauseEepComplete))
                                {
                                    CodCauseEepComplete = -1;
                                }
                                newRowComplete[12] = CodCauseEepComplete.ToString();

                                if (!int.TryParse(worksheet1.Cells[row, 13].Text, out int CodCauseComplete))
                                {
                                    CodCauseComplete = -1;
                                }
                                newRowComplete[13] = CodCauseComplete.ToString();                                
                                newRowComplete[14] = idResult.ToString(); // comment

                                newRowComplete[15] = worksheet1.Cells[row, 15].Text.Trim().ToUpper();
                                newRowComplete[16] = string.IsNullOrEmpty(worksheet1.Cells[row, 16].Text) ? "-1" : worksheet1.Cells[row, 16].Text.Trim().ToUpper();
                                newRowComplete[17] = string.IsNullOrEmpty(worksheet1.Cells[row, 17].Text) ? "-1" : worksheet1.Cells[row, 17].Text.Trim().ToUpper();
                                if (!int.TryParse(worksheet1.Cells[row, 18].Text, out int CodConsigComplete))
                                {
                                    CodConsigComplete = -1;
                                }
                                newRowComplete[18] = CodConsigComplete;
                                newRowComplete[19] = worksheet1.Cells[row, 19].Text.Trim().ToUpper();
                                newRowComplete[20] = worksheet1.Cells[row, 20].Text.Trim().ToUpper();
                                if (!float.TryParse(worksheet1.Cells[row, 21].Text, out float outPowerComplete))
                                {
                                    outPowerComplete = -1;
                                }
                                newRowComplete[21] = outPowerComplete;
                                if (!float.TryParse(worksheet1.Cells[row, 22].Text, out float DnaComplete))
                                {
                                    DnaComplete = -1;
                                }
                                newRowComplete[22] = DnaComplete;
                                if (!int.TryParse(worksheet1.Cells[row, 23].Text, out int usersComplete))
                                {
                                    usersComplete = -1;
                                }
                                newRowComplete[23] = usersComplete;
                                newRowComplete[24] = string.IsNullOrEmpty(worksheet1.Cells[row, 24].Text) ? "-1" : worksheet1.Cells[row, 24].Text.Trim().ToUpper();
                                newRowComplete[25] = "-1";
                                newRowComplete[26] = "-1";
                                newRowComplete[27] = "-1";

                                dataTableComplete.Rows.Add(newRowComplete);
                                #endregion


                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} de la hoja '{worksheet1}' y columna Fecha, la fecha no puede ser nula";
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
                                newRowError[0] = $"Error en la data en la línea {row} de la hoja '{worksheet2}', uno o más valores están vacíos y estos son Requeridos";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else if (date != "")
                            {
                                var date2 = ParseDate(date);
                                if (date2 == ParseDate("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $" En la línea {row} de la hoja '{worksheet2}' y columna Fecha";
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
                                    var newRow = dataTable.NewRow();

                                    newRow[0] = ParseDate(worksheet2.Cells[row, 1].Text).ToString();
                                    newRow[1] = ConvertString(worksheet2.Cells[row, 2].Text.Trim());
                                    newRow[2] = "TRANSFORMADOR";
                                    newRow[3] = worksheet2.Cells[row, 3].Text.Trim().Replace(" ", "");
                                    newRow[4] = "NO DATA";
                                    newRow[5] = "NO DATA";
                                    newRow[6] = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 7].Text}").ToString();
                                    newRow[7] = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 8].Text}").ToString();
                                    if (!float.TryParse(worksheet2.Cells[row, 9].Text, out float minInterruption))
                                    {
                                        minInterruption = -1;
                                    }
                                    newRow[8] = minInterruption;
                                    newRow[9] = minInterruption == -1 ? "-1" : (minInterruption/60).ToString();

                                    if (!int.TryParse(worksheet2.Cells[row, 14].Text, out int cregCause))
                                    {
                                        cregCause = -1;
                                    }
                                    newRow[10] = cregCause.ToString();

                                    if (!int.TryParse(worksheet2.Cells[row, 13].Text, out int cause))
                                    {
                                        cause = -1;
                                    }
                                    newRow[11] = cause.ToString();

                                    newRow[12] = worksheet2.Cells[row, 16].Text;
                                    newRow[13] = worksheet2.Cells[row, 17].Text;

                                    if (!int.TryParse(worksheet2.Cells[row, 19].Text, out int users))
                                    {
                                        users = -1;
                                    }
                                    newRow[14] = users;

                                    newRow[15] = "-1";
                                    newRow[16] = "-1";
                                    newRow[17] = worksheet2.Cells[row, 18].Text;
                                    newRow[18] = fileName;
                                    var aniomes2 = worksheet2.Cells[row, 1].Text.Split('/', ' ');
                                    newRow[19] = int.Parse(aniomes2[2]);
                                    newRow[20] = int.Parse(aniomes2[1]);                                    

                                    if (!Regex.IsMatch(newRow[16].ToString(), @"\d"))
                                    {
                                        newRow[16] = "NO DATA";
                                    }

                                    dataTable.Rows.Add(newRow);

                                    #endregion

                                    #region complete                                    

                                    var newRowComplete = dataTableComplete.NewRow();

                                    var ioCommentTemp = new IoComment();

                                    ioCommentTemp.FileName = fileName;
                                    ioCommentTemp.FileLine = row;                                    
                                    ioCommentTemp.AffectedSector = "-1";

                                    string texto2 = worksheet2.Cells[row, 10].Text.Trim().ToUpper();
                                    int longitud2 = Math.Min(3072, texto2.Length);
                                    ioCommentTemp.Comment = string.IsNullOrEmpty(texto2) ? "-1" : texto2.Substring(0, longitud2);

                                    var idResult = ioCommentsDataAccess.CreateRegister(ioCommentTemp);

                                    newRowComplete[0] = ParseDate(worksheet2.Cells[row, 1].Text);
                                    newRowComplete[1] = ConvertString(worksheet2.Cells[row, 2].Text.Trim());
                                    
                                    string textoLocation = worksheet2.Cells[row, 5].Text.Trim().ToUpper().Replace(',',';').ToString().Replace("\n", " ");
                                    int longitudLocation = Math.Min(50, textoLocation.Length);
                                    newRowComplete[2] = string.IsNullOrEmpty(textoLocation) ? "-1" : textoLocation.Substring(0, longitudLocation);

                                    newRowComplete[3] = worksheet2.Cells[row, 3].Text.Trim().ToUpper();

                                    newRowComplete[4] = "NO DATA";
                                    newRowComplete[5] = "NO DATA";
                                    newRowComplete[6] = "NO DATA";

                                    newRowComplete[7] = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 7].Text}").ToString();
                                    newRowComplete[8] = DateTime.Parse($"{worksheet2.Cells[row, 1].Text} {worksheet2.Cells[row, 8].Text}").ToString();
                                    if (!float.TryParse(worksheet2.Cells[row, 10].Text, out float minInterruptionComplete))
                                    {
                                        minInterruptionComplete = -1;
                                    }
                                    newRowComplete[9] = minInterruptionComplete;

                                    newRowComplete[10] = minInterruptionComplete == -1 ? "-1-" : (minInterruptionComplete/60).ToString();

                                    string textoDesc = worksheet2.Cells[row, 12].Text.Trim().ToUpper().Replace(',',';').ToString().Replace("\n", " ");
                                    int longitudDesc = Math.Min(50, textoDesc.Length);
                                    newRowComplete[11] = string.IsNullOrEmpty(textoDesc) ? "-1" : textoDesc.Substring(0, longitudDesc);

                                    if (!int.TryParse(worksheet2.Cells[row, 13].Text, out int CodCauseEepComplete))
                                    {
                                        CodCauseEepComplete = -1;
                                    }
                                    newRowComplete[12] = CodCauseEepComplete.ToString();

                                    if (!int.TryParse(worksheet2.Cells[row, 14].Text, out int CodCauseComplete))
                                    {
                                        CodCauseComplete = -1;
                                    }
                                    newRowComplete[13] = CodCauseComplete.ToString();
                                    
                                    newRowComplete[14] = idResult;

                                    newRowComplete[15] = worksheet2.Cells[row, 18].Text.Trim().ToUpper();
                                    newRowComplete[16] = "-1";
                                    newRowComplete[17] = "-1";

                                    if (!int.TryParse(worksheet2.Cells[row, 15].Text, out int CodConsigComplete))
                                    {
                                        CodConsigComplete = -1;
                                    }
                                    newRowComplete[18] = CodConsigComplete.ToString();

                                    newRowComplete[19] = worksheet2.Cells[row, 16].Text.Trim().ToUpper();
                                    newRowComplete[20] = worksheet2.Cells[row, 17].Text.Trim().ToUpper();
                                    newRowComplete[21] = "-1";
                                    newRowComplete[22] = "-1";

                                    if (!int.TryParse(worksheet2.Cells[row, 19].Text, out int usersComplete))
                                    {
                                        usersComplete = -1;
                                    }
                                    newRowComplete[23] = usersComplete.ToString();
                                    newRowComplete[24] = worksheet2.Cells[row, 20].Text.Trim().ToUpper();
                                    
                                    if (!float.TryParse(worksheet2.Cells[row, 4].Text, out float capacityKVAComplete))
                                    {
                                        capacityKVAComplete = -1;
                                    }
                                    newRowComplete[25] = capacityKVAComplete.ToString();
                                    newRowComplete[26] = worksheet2.Cells[row, 6].Text.Trim().ToUpper();
                                    newRowComplete[27] = worksheet2.Cells[row, 11].Text.Trim().ToUpper();

                                    dataTableComplete.Rows.Add(newRowComplete);

                                    #endregion


                                }

                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} de la hoja '{worksheet2}' y columna Fecha, la fecha no puede ser nula";
                                dataTableError.Rows.Add(newRowError);
                            }

                        }

                        statusFilesingle.Status = 1;
                        if (dataTableError.Rows.Count > 0)
                        {
                            errorFlag = true;
                            statusFilesingle.Status = 2;
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        var subgroupMap = mapper.Map<QueueStatusIo>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataIo(subgroupMap);

                        if (dataTable.Rows.Count > 0)
                        {                            
                            RegisterTable(dataTable, inputFolder, filePath);
                        }

                        if (dataTableComplete.Rows.Count > 0)
                        {
                            RegisterTableComplete(dataTableComplete, inputFolder, filePath);
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

        private static void RegisterTable(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < 21; i++)
                    {
                        csv.WriteField(row[i]);
                    }                    
                    csv.NextRecord();
                }
            }
        }

        private static void RegisterTableComplete(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_CorrectComplete.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < 28; i++)
                    {
                        csv.WriteField(row[i]);
                    }
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

        private DateOnly ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format, _spanishCultureOnly, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate; // o .ToUniversalTime() si tu columna es timestamptz
                }
            }
            return DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", _spanishCultureOnly);
        }        
    }
}
