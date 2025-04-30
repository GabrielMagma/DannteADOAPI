using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ADO.BL.Services
{
    public class FileIOProcessingServices : IFileIOProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string _connectionString;
        private readonly string[] _timeFormats;
        private readonly string _IOsDirectoryPath;
        private readonly IFileIODataAccess fileIODataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;

        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        private static readonly CultureInfo _spanishCultureOnly = new CultureInfo("es-CO"); // o "es-ES"

        public FileIOProcessingServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IFileIODataAccess _fileIODataAccess)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _IOsDirectoryPath = configuration["IOsPath"];
            fileIODataAccess = _fileIODataAccess;
            statusFileDataAccess = _statuFileDataAccess;
        }

        public async Task<ResponseQuery<bool>> ReadFilesIos(IOsValidationDTO iosValidation, ResponseQuery<bool> response)
        {
            try
            {                

                string inputFolder = _IOsDirectoryPath;
                var errorFlag = false;
                var fileFlag = false;

                var ioList = new List<FileIoDTO>();
                var ioCompleteList = new List<FileIoCompleteDTO>();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv").Where(file => !file.EndsWith("_Error.csv")).ToList().OrderBy(f => f)
                     .ToArray())
                {                                        
                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = fileName.Replace("_Correct", "").Replace("_CorrectComplete", "").Replace("_Error", "");

                    // Obtener los primeros 4 dígitos como el año
                    int yearName = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int monthName = int.Parse(fileName.Substring(4, 2));

                    // Obtener los siguientes 2 dígitos como el día
                    int dayName = int.Parse(fileName.Substring(6, 2));

                    var beginDate = ParseDateTemp($"{dayName}/{monthName}/{yearName}");
                    var endDate = beginDate.AddDays(-30);
                    var listDates = new StringBuilder();
                    var listFilesError = new StringBuilder();
                    var lacQueueList = new List<LacQueueDTO>();

                    while (endDate <= beginDate)
                    {
                        listDates.Append($"'{endDate.Day}-{endDate.Month}-{endDate.Year}',");
                        endDate = endDate.AddDays(1);
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDatesDef = listDates.ToString().Remove(listDates.Length - 1, 1);
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_io where date_register in ({listDatesDef})";
                        using (var reader = new NpgsqlCommand(SelectQuery, connection))
                        {
                            try
                            {

                                using (var result = await reader.ExecuteReaderAsync())
                                {
                                    while (await result.ReadAsync())
                                    {
                                        var temp = new LacQueueDTO();
                                        temp.file_name = result[0].ToString();
                                        temp.year = int.Parse(result[1].ToString());
                                        temp.month = int.Parse(result[2].ToString());
                                        temp.day = int.Parse(result[3].ToString());
                                        temp.status = int.Parse(result[4].ToString());

                                        lacQueueList.Add(temp);
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

                    var flagValidation = false;
                    foreach (var item in lacQueueList)
                    {
                        if (item.status == 0 || item.status == 2 || item.status == 3)
                        {
                            flagValidation = true;
                            listFilesError.Append($"{item.file_name},");
                        }
                    }

                    if (flagValidation)
                    {
                        var listFilesErrorDef = listFilesError.ToString().Remove(listFilesError.Length - 1, 1);
                        response.Message = $"Los archivos {listFilesErrorDef} no han sido procesados correctamente, favor corregirlos";
                        response.SuccessData = false;
                        response.Success = false;
                        return response;
                    }

                    string[] fileLinesTemp = File.ReadAllLines(filePath);
                    var resultYearMonth = getYearMonth(fileLinesTemp);
                    int year = 2099;
                    int month = 12;
                    int day = 31;
                    if (resultYearMonth.Count > 0)
                    {
                        year = int.Parse(resultYearMonth[0]);
                        month = int.Parse(resultYearMonth[1]);
                        day = int.Parse(resultYearMonth[2]);
                    }

                    var fileNameXlsxTemp = Directory.GetFiles(inputFolder, $"{fileNameTemp}.xlsx").FirstOrDefault();
                    var fileNameXlsx = Path.GetFileNameWithoutExtension(fileNameXlsxTemp);
                    if (fileNameXlsx != null && !fileFlag)
                    {
                        statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                        statusFilesingle.UserId = iosValidation.UserId;
                        statusFilesingle.FileName = fileNameXlsx;
                        statusFilesingle.FileType = "IO";
                        statusFilesingle.Year = year;
                        statusFilesingle.Month = month;
                        statusFilesingle.Day = day;
                        statusFilesingle.DateRegister = ParseDateTemp($"{day}/{month}/{year}");
                        statusFilesingle.Status = 4;

                        var subgroupMap = mapper.Map<QueueStatusIo>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataIo(subgroupMap);

                        await DeleteDuplicateIO(fileNameXlsx);
                        fileFlag = true;
                    }                                                                                                

                    //tabla files_io
                    if (fileName.Contains("_Correct") && !fileName.Contains("_CorrectComplete")) {
                        #region tabla io
                        string[] fileLines = File.ReadAllLines(filePath);
                        foreach (var item in fileLines)
                        {
                            var valueLines = item.Split(',');
                            var newEntity = new FileIoDTO();
                            newEntity.DateIo = ParseDateTemp(valueLines[0]);
                            newEntity.CodeSig = valueLines[1];
                            newEntity.TypeAsset = valueLines[2];
                            newEntity.Fparent = valueLines[3];
                            newEntity.Element = valueLines[4];
                            newEntity.Component = valueLines[5];
                            newEntity.HourOut = DateTime.Parse(valueLines[6]);
                            newEntity.HourIn = DateTime.Parse(valueLines[7]);
                            newEntity.MinInterruption = float.Parse(valueLines[8]);
                            newEntity.HourInterruption = float.Parse(valueLines[9]);
                            newEntity.CregCause = int.Parse(valueLines[10]);
                            newEntity.Cause = int.Parse(valueLines[11]);
                            newEntity.EventType = valueLines[12];
                            newEntity.Dependence = valueLines[13];
                            newEntity.Users = int.Parse(valueLines[14]);
                            newEntity.DnaKwh = float.Parse(valueLines[15]);
                            newEntity.Failure = int.Parse(valueLines[16]);
                            newEntity.Maneuver = valueLines[17];
                            newEntity.FileIo = valueLines[18];
                            newEntity.Year = int.Parse(valueLines[19]);
                            newEntity.Month = int.Parse(valueLines[20]);

                            if (newEntity.CodeSig.StartsWith("S")) newEntity.TypeAsset = "SWITCH";
                            else if (newEntity.CodeSig.StartsWith("R")) newEntity.TypeAsset = "RECONECTADOR";                            

                            ioList.Add(newEntity);
                        }
                        #endregion
                    }

                    //tabla complete
                    if (fileName.Contains("_CorrectComplete")) {
                        #region complete
                        string[] fileLines = File.ReadAllLines(filePath);
                        foreach (var item in fileLines)
                        {
                            var valueLines = item.Split(',');
                            var newEntityComplete = new FileIoCompleteDTO();
                            newEntityComplete.DateIo = ParseDateTemp(valueLines[0]);
                            newEntityComplete.CodeGis = valueLines[1];
                            newEntityComplete.Location = valueLines[2];
                            newEntityComplete.Ubication = valueLines[3];
                            newEntityComplete.Element = valueLines[4];
                            newEntityComplete.Component = valueLines[5];
                            newEntityComplete.AffectedSector = int.Parse(valueLines[6]);
                            newEntityComplete.HourOut = DateTime.Parse(valueLines[7]);
                            newEntityComplete.HourIn = DateTime.Parse(valueLines[8]);
                            newEntityComplete.MinInterruption = float.Parse(valueLines[9]);
                            newEntityComplete.HourInterruption = float.Parse(valueLines[10]);
                            newEntityComplete.DescCause = valueLines[11];
                            newEntityComplete.CodCauseEvent = int.Parse(valueLines[12]);
                            newEntityComplete.Cause = int.Parse(valueLines[13]);
                            newEntityComplete.Observation = int.Parse(valueLines[14]);
                            newEntityComplete.Maneuver = valueLines[15];
                            newEntityComplete.FuseQuant = valueLines[16];
                            newEntityComplete.FuseCap = valueLines[17];
                            newEntityComplete.CodeConsig = int.Parse(valueLines[18]);
                            newEntityComplete.TypeEvent = valueLines[19];
                            newEntityComplete.Dependency = valueLines[20];
                            newEntityComplete.OutPower = float.Parse(valueLines[21]);
                            newEntityComplete.DnaKwh = float.Parse(valueLines[22]);
                            newEntityComplete.Users = int.Parse(valueLines[23]);
                            newEntityComplete.ApplicationId = valueLines[24];
                            newEntityComplete.CapacityKva = float.Parse(valueLines[25]);
                            newEntityComplete.Type = valueLines[26];
                            newEntityComplete.Ownership = valueLines[27];

                            ioCompleteList.Add(newEntityComplete);
                        }
                        #endregion
                    }                                                                                                              

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

                response.Message = "All files created";
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
                var resultDate = ParseDate(valueLines[0]);
                if (resultDate != DateTime.Parse("31/12/2099 00:00:00"))
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

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate; // o .ToUniversalTime() si tu columna es timestamptz
                }
            }
            return DateTime.ParseExact("31/12/2099 00:00:00", "dd/MM/yyyy HH:mm:ss", _spanishCulture);
        }

        private DateOnly ParseDateTemp(string dateString)
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
