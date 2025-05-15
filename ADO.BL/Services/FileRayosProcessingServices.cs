using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileRayosProcessingServices : IFileRayosProcessingServices
    {   
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IRayosCSVDataAccess rayosCSVDataAccess;
        private readonly string _RayosDirectoryPath;
        private readonly string _connectionString;
        private readonly string[] _timeFormats;
        private readonly IMapper mapper;
        public FileRayosProcessingServices(IConfiguration configuration, 
            IRayosCSVDataAccess _rayosCSVDataAccess,            
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _RayosDirectoryPath = configuration["RayosPath"];
            statusFileDataAccess = _statuFileDataAccess;
            rayosCSVDataAccess = _rayosCSVDataAccess;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesRayos(RayosValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _RayosDirectoryPath;

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
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_lightning where date_register in ({listDatesDef})";
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

                foreach (var filePath in Directory.GetFiles(inputFolder, "*_Correct.csv")
                                        .ToList().OrderBy(f => f)
                                        .ToArray())
                {
                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = fileName.Substring(0, 12);
                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileNameTemp} está Procesando para guardado de registros");

                    var statusFileList = new List<StatusFileDTO>();

                    string[] fileLines = File.ReadAllLines(filePath);

                    var statusFilesingle = new StatusFileDTO();                    

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileNameTemp;
                    statusFilesingle.FileType = "RAYOS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    statusFileList.Add(statusFilesingle);


                    var listDTOMpLightning = new List<MpLightningDTO>();

                    // columnas tabla datos correctos
                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(',',';');
                        string message = string.Empty;

                        var dateDef = ParseDate(valueLines[0]);

                        var newEntity = new MpLightningDTO();

                        newEntity.NameRegion = valueLines[1].Trim().ToUpper();
                        newEntity.Fparent = valueLines[2].Trim().Replace(" ", "");
                        newEntity.DateEvent = dateDef;
                        newEntity.Latitude = float.Parse(valueLines[3].Replace(',', '.').Trim());
                        newEntity.Longitude = float.Parse(valueLines[4].Replace(',', '.').Trim());
                        newEntity.Amperage = float.Parse(valueLines[5].Replace(',', '.').Trim());
                        newEntity.Error = float.Parse(valueLines[6].Replace(',', '.').Trim());
                        newEntity.Type = 1;
                        newEntity.Year = dateDef.Year;
                        newEntity.Month = dateDef.Month;

                        listDTOMpLightning.Add(newEntity);


                    }                    
                    
                    statusFilesingle.Status = 4;

                    if (listDTOMpLightning.Count > 0)
                    {
                        SaveDataRayos(listDTOMpLightning);
                    }

                    var subgroupMap = mapper.Map<QueueStatusLightning>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataRayos(subgroupMap);

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Proceso de creación de registros completado para todos los archivos");
                }
        
                response.Message = "Todos los archivos procesados";
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

        private void SaveDataRayos(List<MpLightningDTO> listRayos)
        {
            int i = 0;
            while ((i * 10000) < listRayos.Count())
            {
                var subgroup = listRayos.Skip(i * 10000).Take(10000).ToList();
                var rayosMapped = mapper.Map<List<MpLightning>>(subgroup);
                rayosCSVDataAccess.SaveData(rayosMapped);
                i++;
                Console.WriteLine(i * 10000);
            }

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
