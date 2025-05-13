using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace ADO.BL.Services
{
    public class FileCompensacionesProcessingServices : IFileCompensacionesProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string _connectionString;
        private readonly string _CompsDirectoryPath;
        private readonly IConfiguration _configuration;
        private readonly ICompsDataAccess compsDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        public FileCompensacionesProcessingServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext,
            ICompsDataAccess _compsDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _CompsDirectoryPath = configuration["CompensationsPath"];
            statusFileDataAccess = _statuFileDataAccess;
            compsDataAccess = _compsDataAccess;
            _configuration = configuration;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesComp(CompsValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _CompsDirectoryPath;
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
                    var listEntityCompDTO = new List<MpCompensacionesDTO>();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "COMPENSACIONES";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    string[] fileLines = File.ReadAllLines(filePath);

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');


                        var entityComp = new MpCompensacionesDTO();

                        entityComp.Month = int.Parse(valueLines[0].Trim());
                        entityComp.Year = int.Parse(valueLines[1].Trim());
                        entityComp.Fparent = valueLines[2].Trim();
                        entityComp.CodeSig = valueLines[3].Trim();
                        entityComp.QualityGroup = valueLines[4].Trim();
                        entityComp.TensionLevel = valueLines[5].Trim();
                        entityComp.Nui = valueLines[6].Trim();
                        entityComp.Vcf = float.Parse(valueLines[7].ToString());
                        entityComp.Vcd = float.Parse(valueLines[8].ToString());
                        entityComp.Vc = float.Parse(valueLines[9].ToString());
                        entityComp.Longitude = float.Parse(valueLines[10].ToString());
                        entityComp.Latitude = float.Parse(valueLines[11].ToString());

                        listEntityCompDTO.Add(entityComp);

                    }

                    statusFilesingle.Status = 4;

                    statusFileList.Add(statusFilesingle);

                    var compsMapped = mapper.Map<List<MpCompensation>>(listEntityCompDTO);
                    var respCreate = CreateData(compsMapped);

                    var entityMap = mapper.Map<QueueStatusCompensation>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataCompensations(entityMap);
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

        public async Task<Boolean> CreateData(List<MpCompensation> request)
        {
            await compsDataAccess.CreateFile(request);
            return true;

        }

    }
}
