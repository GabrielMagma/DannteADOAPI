using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FilePolesProcessingServices : IFilePolesProcessingServices
    {
        private readonly IMapper mapper;
        private readonly string _connectionString;
        private readonly string _PolesDirectoryPath;
        private readonly IConfiguration _configuration;
        private readonly IPolesDataAccess polesDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        public FilePolesProcessingServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext,
            IPolesDataAccess _polesDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
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

    }
}
