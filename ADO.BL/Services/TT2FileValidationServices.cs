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
    public class TT2FileValidationServices : ITT2FileValidationServices
    {
        private readonly string _connectionString;        
        private readonly string _tt2DirectoryPath;
        private readonly string[] _timeFormats;
        private const int _batchSize = 10000;
        private readonly ITT2ValidationServices _Itt2ValidationServices;        
        private readonly IStatusFileDataAccess statusFileEssaDataAccess;
        private readonly IMapper mapper;
        readonly IFileTT2ValidationServices fileTT2Services;
        private readonly IHubContext<NotificationHub> _hubContext;
        public TT2FileValidationServices(IConfiguration configuration, 
            ITT2ValidationServices Itt2ValidationServices,            
            IStatusFileDataAccess _statusFileEssaDataAccess,
            IMapper _mapper,
            IFileTT2ValidationServices _fileTT2Services,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _tt2DirectoryPath = configuration["TT2DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _Itt2ValidationServices = Itt2ValidationServices;            
            statusFileEssaDataAccess = _statusFileEssaDataAccess;
            mapper = _mapper;
            fileTT2Services = _fileTT2Services;
            _hubContext = hubContext;
        }

        public async Task<ResponseQuery<bool>> ReadFilesTT2(TT2ValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                ResponseQuery<bool> responseFileCreate = new ResponseQuery<bool>();
                var errorFileCreate = await fileTT2Services.ValidationTT2(request, responseFileCreate);
                if (errorFileCreate.Success == false)
                {
                    response.Message = errorFileCreate.Message;
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                var errorResponse = new ResponseEntity<List<StatusFileDTO>>();                
                var errorFile = await _Itt2ValidationServices.ValidationTT2(request, errorResponse);
                if (errorFile.Success == false)
                {
                    response.Message = errorFile.Message;
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    var completed1 = await BeginProcess(request);
                    Console.WriteLine(completed1);


                    var subgroupMap = mapper.Map<List<QueueStatusTt2>>(errorFile.Data);
                    
                    if (completed1 != "Completed")
                    {

                        foreach (var item in subgroupMap)
                        {
                            item.Status = 3;
                        }
                        response.Message = "File with errors";
                        response.SuccessData = false;
                        response.Success = false;
                    }
                    else
                    {
                        response.Message = "validation process completed successfully";
                        response.SuccessData = true;
                        response.Success = true;
                    }                    
                    var resultSave = await statusFileEssaDataAccess.UpdateDataTT2List(subgroupMap);
                    

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

        private async Task<string> BeginProcess(TT2ValidationDTO request)
        {
            try
            {
                Console.WriteLine("BeginProcess");
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_Correct.csv")
                    .Where(file => !file.EndsWith("_insert.csv") && !file.EndsWith("_check.csv") && !file.EndsWith("_update.csv"))
                    .ToList().OrderBy(f => f)
                     .ToArray();

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileNameTemp} se está validando");

                    await ProcessAndCompleteFile(filePath);
                }
                Console.WriteLine("EndBeginProcess");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        private async Task ProcessAndCompleteFile(string filePath)
        {
            var completedLines = new List<string>();
            var batchUias = new List<(string uia, string originalLine)>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var columns = line.Split(new char[] { ',', ';' });

                        if (columns.Length > 1)
                        {
                            string uia = columns[0];
                            batchUias.Add((uia, line.Replace(",", ";")));
                        }

                        if (batchUias.Count >= _batchSize || reader.EndOfStream)
                        {
                            var codeSigMap = await GetCodeSigMap(batchUias.Select(b => b.uia).ToList(), connection);

                            foreach (var (uia, originalLine) in batchUias)
                            {
                                var valueLines = originalLine.Split(';', ',');
                                var codeSig = codeSigMap.ContainsKey(uia) ? codeSigMap[uia] : valueLines[1];
                                completedLines.Add($"{codeSig};{originalLine}");
                            }

                            batchUias.Clear();
                        }
                    }
                }
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string completedFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_completed.csv");

            await WriteAllLinesWithoutTrailingNewline(completedFilePath, completedLines);
        }

        private async Task<Dictionary<string, string>> GetCodeSigMap(List<string> uias, NpgsqlConnection connection)
        {
            var codeSigMap = new Dictionary<string, string>();

            if (uias == null || !uias.Any())
                return codeSigMap;

            // Crear una lista de valores para la cláusula IN
            var parameters = string.Join(",", uias.Select((uia, index) => $"@uia{index}"));

            var query = $@"
                        SELECT uia, code_sig
                        FROM public.all_asset
                        WHERE uia IN ({parameters})";

            using (var command = new NpgsqlCommand(query, connection))
            {
                // Añadir parámetros al comando
                for (int i = 0; i < uias.Count; i++)
                {
                    command.Parameters.AddWithValue($"@uia{i}", NpgsqlTypes.NpgsqlDbType.Varchar, uias[i]);
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var uia = reader.GetString(0);
                        var codeSig = reader.GetString(1);
                        codeSigMap[uia] = codeSig;
                    }
                }
            }

            return codeSigMap;
        }

        // Método para escribir líneas en un archivo sin nueva línea final
        private async Task WriteAllLinesWithoutTrailingNewline(string filePath, IEnumerable<string> lines)
        {
            using (var writer = new StreamWriter(filePath, false))
            {
                foreach (var line in lines)
                {
                    var formattedLine = line.Replace(",", ";"); // Cambiar el separador a ;
                    await writer.WriteLineAsync(formattedLine);
                }
            }
        }

    }
}
