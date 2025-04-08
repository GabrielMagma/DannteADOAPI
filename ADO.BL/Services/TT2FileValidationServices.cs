using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
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

        public TT2FileValidationServices(IConfiguration configuration, 
            ITT2ValidationServices Itt2ValidationServices,            
            IStatusFileDataAccess _statusFileEssaDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _tt2DirectoryPath = configuration["TT2DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _Itt2ValidationServices = Itt2ValidationServices;            
            statusFileEssaDataAccess = _statusFileEssaDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<List<string>>> CompleteTT2Originals(TT2ValidationDTO request, ResponseQuery<List<string>> response)
        {
            try
            {                
                var errorResponse = new ResponseEntity<List<StatusFileDTO>>();
                var errorFile = await _Itt2ValidationServices.ValidationTT2(request, errorResponse);
                if (errorFile.Success == false)
                {
                    response.Message = "Archivo con errores";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    var completed1 = await BeginProcess();
                    Console.WriteLine(completed1);
                    

                    var subgroupMap = mapper.Map<List<QueueStatusTt2>>(errorFile.Data);
                    
                    if (completed1 != "Completed")
                    {

                        foreach (var item in subgroupMap)
                        {
                            item.Status = 3;
                        }
                        response.Message = "Proceso Con errores, favor validar y volver a lanzar el proceso";
                        response.SuccessData = false;
                        response.Success = false;
                    }
                    else
                    {
                        response.Message = "Proceso completado con éxito";
                        response.SuccessData = true;
                        response.Success = true;
                    }                    
                    var resultSave = await statusFileEssaDataAccess.UpdateDataTT2List(subgroupMap);
                    

                    response.Message = "Proceso completado para todos los archivos";
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

        private async Task<string> BeginProcess()
        {
            try
            {
                Console.WriteLine("BeginProcess");
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_Correct.csv")
                    .Where(file => !file.EndsWith("_insert.csv") && !file.EndsWith("_check.csv") && !file.EndsWith("_update.csv"))
                    .ToList();

                foreach (var filePath in files)
                {
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
                                var codeSig = codeSigMap.ContainsKey(uia) ? codeSigMap[uia] : "N/A";
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
