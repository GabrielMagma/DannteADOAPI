using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace ADO.BL.Services
{
    public class FileTrafosQProcessingServices : IFileTrafosQProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _connectionString;
        private readonly string _TrafosDirectoryPath;        
        private readonly IConfiguration _configuration;
        private readonly ITrafosDataAccess trafosDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        public FileTrafosQProcessingServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext,
            ITrafosDataAccess _trafosDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _TrafosDirectoryPath = configuration["TrafosPath"];
            statusFileDataAccess = _statuFileDataAccess;
            trafosDataAccess = _trafosDataAccess;
            _configuration = configuration;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFileTrafos(TrafosValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _TrafosDirectoryPath;
                var errorFlag = false;

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
                    
                    var statusFileList = new List<StatusFileDTO>();
                    var statusFilesingle = new StatusFileDTO();
                    var listDTOTrafos = new List<MpTransformerBurnedDTO>();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "TRANSFORMADORES QUEMADOS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    string[] fileLines = File.ReadAllLines(filePath);

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(',');


                        var newEntity = new MpTransformerBurnedDTO();

                        
                        newEntity.CodeSig = valueLines[0].Trim();
                        newEntity.Year = int.Parse(valueLines[1].Trim());
                        newEntity.Month = int.Parse(valueLines[2].Trim());
                        newEntity.Total = int.Parse(valueLines[3].Trim());
                        newEntity.Fparent = valueLines[4].Trim();
                        newEntity.FailureDate = ParseDate(valueLines[5].Trim());
                        newEntity.RetireDate = ParseDate(valueLines[6].Trim());
                        newEntity.ChangeDate = ParseDate(valueLines[7].Trim());
                        newEntity.Latitude = float.Parse(valueLines[8].Trim());
                        newEntity.Longitude = float.Parse(valueLines[9].Trim());


                        listDTOTrafos.Add(newEntity);

                    }

                    statusFilesingle.Status = 4;

                    statusFileList.Add(statusFilesingle);

                    var trafosMapped = mapper.Map<List<MpTransformerBurned>>(listDTOTrafos);
                    var respCreate = CreateData(trafosMapped);

                    var entityMap = mapper.Map<QueueStatusTransformerBurned>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataTrafosQuemados(entityMap);
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

        public async Task<Boolean> CreateData(List<MpTransformerBurned> request)
        {            
            await trafosDataAccess.SaveData(request);
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
