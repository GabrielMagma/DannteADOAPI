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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text;

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
                var listStatusFiles = new List<StatusFileDTO>();
                var lacQueueList = new List<LacQueueDTO>();

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx")
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
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_transformer_burned where date_register in ({listDatesDef})";
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

                    var TrafosList = new List<UpdateTrafoDTO>();

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();

                        #region update total trafos
                        var SelectQuery = $@"SELECT code_sig, year, month, count(*) as count FROM maps.mp_transformer_burned
                                             group by code_sig, year, month";
                        using (var reader = new NpgsqlCommand(SelectQuery, connection))
                        {
                            try
                            {

                                using (var result = await reader.ExecuteReaderAsync())
                                {
                                    while (await result.ReadAsync())
                                    {
                                        var temp = new UpdateTrafoDTO();

                                        temp.code_sig = result[0].ToString();
                                        temp.year = int.Parse(result[1].ToString());
                                        temp.month = int.Parse(result[2].ToString());
                                        temp.count = long.Parse(result[3].ToString());

                                        TrafosList.Add(temp);
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
                        #endregion

                    }

                    var connectionUpdate = new NpgsqlConnection(_connectionString);

                    await connectionUpdate.OpenAsync();

                    using (var transaction = await connectionUpdate.BeginTransactionAsync())
                    {
                        var updateQuery = new StringBuilder();
                        updateQuery.Append("WITH update_values (code_sig, year, month, count) AS (VALUES ");

                        for (int i = 0; i < TrafosList.Count; i++)
                        {
                            if (i > 0) updateQuery.Append(",");
                            updateQuery.Append($"(@code_sig{i}, @year{i}, @month{i}, @count{i})");
                        }

                        updateQuery.Append(") ");
                        updateQuery.Append("UPDATE maps.mp_transformer_burned SET total = uv.count ");
                        updateQuery.Append("FROM update_values uv ");
                        updateQuery.Append("WHERE maps.mp_transformer_burned.code_sig = uv.code_sig " +
                            "AND maps.mp_transformer_burned.year = uv.year " +
                            "and maps.mp_transformer_burned.month = uv.month;");

                        var updateCommand = new NpgsqlCommand(updateQuery.ToString(), connectionUpdate);

                        for (int i = 0; i < TrafosList.Count; i++)
                        {
                            updateCommand.Parameters.AddWithValue($"code_sig{i}", NpgsqlTypes.NpgsqlDbType.Varchar, TrafosList[i].code_sig);
                            updateCommand.Parameters.AddWithValue($"year{i}", NpgsqlTypes.NpgsqlDbType.Integer, TrafosList[i].year);
                            updateCommand.Parameters.AddWithValue($"month{i}", NpgsqlTypes.NpgsqlDbType.Integer, TrafosList[i].month);
                            updateCommand.Parameters.AddWithValue($"count{i}", NpgsqlTypes.NpgsqlDbType.Bigint, TrafosList[i].count);
                        }

                        await updateCommand.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                    }

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
