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
    public class FilePolesProcessingServices : IFilePolesProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _connectionString;
        private readonly string _PolesDirectoryPath;
        private readonly IConfiguration _configuration;
        private readonly IPolesDataAccess polesDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        public FilePolesProcessingServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext,
            IPolesDataAccess _polesDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _PolesDirectoryPath = configuration["PolesPath"];
            statusFileDataAccess = _statuFileDataAccess;
            polesDataAccess = _polesDataAccess;
            _configuration = configuration;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesPoles(PolesValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _PolesDirectoryPath;
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
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_poles where date_register in ({listDatesDef})";
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

                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} está procesando los registros");
                    var listUtilityPoleDTO = new List<MpUtilityPoleDTO>();
                    
                    var statusFileList = new List<StatusFileDTO>();
                    var statusFilesingle = new StatusFileDTO();
                    var listEntityPoleDTO = new List<MpUtilityPoleDTO>();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "POLES";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    string[] fileLines = File.ReadAllLines(filePath);

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');


                        var entityPole = new MpUtilityPoleDTO();

                        entityPole.InventaryCode = valueLines[0].Trim();
                        entityPole.PaintingCode = valueLines[0].Trim();
                        entityPole.Latitude = float.Parse(valueLines[3].ToString());
                        entityPole.Longitude = float.Parse(valueLines[4].ToString());
                        entityPole.Fparent = valueLines[1].Trim();
                        entityPole.TypePole = int.Parse(valueLines[2].ToString());

                        listEntityPoleDTO.Add(entityPole);

                    }

                    statusFilesingle.Status = 4;

                    statusFileList.Add(statusFilesingle);

                    var polesMapped = mapper.Map<List<MpUtilityPole>>(listEntityPoleDTO);
                    var respCreate = CreateData(polesMapped);

                    var entityMap = mapper.Map<QueueStatusPole>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataPole(entityMap);
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

        public async Task<Boolean> CreateData(List<MpUtilityPole> request)
        {            
            await polesDataAccess.CreateFile(request);
            return true;

        }

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateTime.ParseExact("31/12/2099 00:00:00", "dd/MM/yyyy HH:mm:ss", _spanishCulture);
        }

    }
}
