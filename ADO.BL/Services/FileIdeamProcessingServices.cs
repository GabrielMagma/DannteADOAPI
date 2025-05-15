using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileIdeamProcessingServices : IFileIdeamProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _connectionString;
        private readonly string _IdeamDirectoryPath;        
        private readonly IConfiguration _configuration;
        private readonly IFileDataAccess ideamDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        public FileIdeamProcessingServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext,
            IFileDataAccess _ideamDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _IdeamDirectoryPath = configuration["FilesIdeamPath"];
            statusFileDataAccess = _statuFileDataAccess;
            ideamDataAccess = _ideamDataAccess;
            _configuration = configuration;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesIdeam(RayosValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _IdeamDirectoryPath;
                var errorFlag = false;

                var listStatusFiles = new List<StatusFileDTO>();
                var lacQueueList = new List<LacQueueDTO>();

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv")
                                        .Where(file => !file.EndsWith("_Correct.csv")
                                        && !file.EndsWith("_Error.csv"))
                                        .ToList().OrderBy(f => f).ToArray())
                {
                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    var UnitStatus = new StatusFileDTO()
                    {
                        FileName = fileName
                    };
                    var exist = listStatusFiles.FirstOrDefault(x => x.FileName == UnitStatus.FileName);

                    if (exist == null)
                    {
                        listStatusFiles.Add(UnitStatus);
                    }

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    var beginDate = ParseDate($"01/{month}/{year}");
                    var endDate = beginDate.AddMonths(-2);
                    var listDates = new StringBuilder();
                    var listFilesError = new StringBuilder();


                    while (endDate <= beginDate)
                    {
                        listDates.Append($"'{endDate.Day}-{endDate.Month}-{endDate.Year}',");
                        endDate = endDate.AddMonths(1);
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDatesDef = listDates.ToString().Remove(listDates.Length - 1, 1);
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_precipitation where date_register in ({listDatesDef})";
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

                                        var existEntity = lacQueueList.FirstOrDefault(x => x.file_name == temp.file_name);
                                        if (existEntity != null)
                                        {
                                            continue;
                                        }
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
                }

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*_Correct.csv"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = fileName.Substring(0,12);

                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileNameTemp} está procesando los registros");
                    
                    var statusFileList = new List<StatusFileDTO>();
                    var statusFilesingle = new StatusFileDTO();
                    var listDTOIdeam = new List<IdeamDTO>();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileNameTemp;
                    statusFilesingle.FileType = "IDEAM";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    string[] fileLines = File.ReadAllLines(filePath);

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(',');


                        var newEntity = new IdeamDTO();

                        newEntity.Stationcode = valueLines[0].Trim();
                        newEntity.Stationname = valueLines[1].Trim();
                        newEntity.Latitude = double.Parse(valueLines[2].Trim());
                        newEntity.Longitude = double.Parse(valueLines[3].Trim());
                        newEntity.Altitude = double.Parse(valueLines[4].Trim());
                        newEntity.Department = valueLines[5].Trim();
                        newEntity.Municipality = valueLines[6].Trim();
                        newEntity.Parameterid = valueLines[7].Trim();
                        newEntity.Frequency = valueLines[8].Trim();
                        newEntity.Date = ParseDate(valueLines[9].Trim());
                        newEntity.Precipitation = double.Parse(valueLines[10].Trim());

                        listDTOIdeam.Add(newEntity);

                    }

                    statusFilesingle.Status = 4;

                    statusFileList.Add(statusFilesingle);

                    var EntityListMapped = mapper.Map<List<IaIdeam>>(listDTOIdeam);
                    var respCreate = CreateData(EntityListMapped);

                    var entityMap = mapper.Map<QueueStatusPrecipitation>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataPrecipitacion(entityMap);
                }

                
                response.Message = "Todos los registros procesados";
                response.SuccessData = true;
                response.Success = true;
                return response;
                
            }

            catch (FormatException ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                return response;
            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                return response;
            }

            
        }

        // acciones en bd y mappeo

        public async Task<Boolean> CreateData(List<IaIdeam> request)
        {            
            await ideamDataAccess.CreateFile(request);
            return true;

        }

        private DateOnly ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", _spanishCulture);
        }

    }
}
