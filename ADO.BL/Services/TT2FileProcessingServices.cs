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
    public class TT2FileProcessingServices : ITT2FileProcessingServices
    {
        private string _connectionString;        
        private readonly string _tt2DirectoryPath;
        private readonly string[] _timeFormats;
        private const int _batchSize = 10000;
        private readonly ITT2ValidationServices _Itt2ValidationServices;        
        private readonly IStatusFileDataAccess statusFileEssaDataAccess;
        private readonly IMapper mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public TT2FileProcessingServices(IConfiguration configuration, 
            ITT2ValidationServices Itt2ValidationServices,            
            IStatusFileDataAccess _statusFileEssaDataAccess,
            IMapper _mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _tt2DirectoryPath = configuration["TT2DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _Itt2ValidationServices = Itt2ValidationServices;            
            statusFileEssaDataAccess = _statusFileEssaDataAccess;
            mapper = _mapper;
            _hubContext = hubContext;
        }

        public async Task<ResponseQuery<bool>> ReadFilesTT2(TT2ValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var lacQueueList = new List<LacQueueDTO>();
                var listStatusTt2 = new List<StatusFileDTO>();

                var listEnds = new List<string>()
                {
                    "_Fixed",
                    "_Error",
                    "_Correct",                    
                    "_completed",
                    "_insert",
                    "_create",
                    "_check",
                    "_update",
                    "_errorCodeSig",
                };

                foreach (var filePath in Directory.GetFiles(_tt2DirectoryPath, "*.csv")
                    .Where(file => !file.EndsWith("_Correct.csv")
                    && !file.EndsWith("_Error.csv")
                    && !file.EndsWith("_create.csv")
                    && !file.EndsWith("_completed.csv")
                    && !file.EndsWith("_insert.csv")
                    && !file.EndsWith("_check.csv")
                    && !file.EndsWith("_errorCodeSig.csv")
                    && !file.EndsWith("_update.csv"))
                    .ToList().OrderBy(f => f)
                     .ToArray()
                    )
                {

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);                    
                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    var nameTemp = fileName;

                    foreach (var item1 in listEnds)
                    {
                        nameTemp = nameTemp.Replace(item1, "");
                    }

                    var UnitStatusTt2 = new StatusFileDTO()
                    {
                        FileName = fileName                        
                    };
                    var exist = listStatusTt2.FirstOrDefault(x => x.FileName == UnitStatusTt2.FileName);
                    if (exist == null)
                    {
                        listStatusTt2.Add(UnitStatusTt2);
                    }

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    var beginDate = ParseDateTemp($"1/{month}/{year}");
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
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_tt2 where date_register in ({listDatesDef})";
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
                var completed2 = await ReadFileTT2Orginal(request);
                Console.WriteLine(completed2);
                var completed3 = await BulkInsertAllAsset(request);
                Console.WriteLine(completed3);
                var completed4 = await ReadTt2Update(request);
                Console.WriteLine(completed4);
                var completed5 = await ReadTt2UpdateCheck(request);
                Console.WriteLine(completed5);

                var subgroupMap = mapper.Map<List<QueueStatusTt2>>(listStatusTt2);
                foreach (var item in subgroupMap)
                {
                    item.Status = 4;
                }
                var resultSave = await statusFileEssaDataAccess.UpdateDataTT2List(subgroupMap);

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var UpdateRegionQuery = $@"UPDATE public.all_asset a
                                                SET 
                                                    id_region = r.id_region,
                                                    name_region = r.name_region
                                                FROM (
                                                    SELECT 
                                                        f.fparent::varchar AS fparent,
                                                        r.id AS id_region,
                                                        r.name_region
                                                    FROM maps.mp_fparent f
                                                    JOIN maps.mp_region r ON f.id_region = r.id
                                                ) r
                                                WHERE a.fparent = r.fparent;";
                    using (var reader = new NpgsqlCommand(UpdateRegionQuery, connection))
                    {
                        try
                        {
                            await reader.ExecuteReaderAsync();
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

                response.Message = "All records created and/or updated";
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

        private async Task<string> ReadFileTT2Orginal(TT2ValidationDTO request)
        {
            try
            {
                // Obtener todos los archivos *_TT2_completed.csv
                var originalFiles = Directory.GetFiles(_tt2DirectoryPath, "*_completed.csv")
                    .Where(file => !file.EndsWith("_insert.csv") &&
                                   !file.EndsWith("_check.csv") &&
                                   !file.EndsWith("_update.csv"))
                    .ToList().OrderBy(f => f)
                     .ToArray();

                if (!originalFiles.Any())
                {
                    return ("No se encontraron archivos *_completed.csv para procesar.");
                }

                foreach (var filePath in originalFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }
                    
                    await CreateTT2Files(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} procesado exitosamente para particiones.");
                }

                // Procesar archivos *_check.csv
                var checkFiles = Directory.GetFiles(_tt2DirectoryPath, "*_check.csv").ToList().OrderBy(f => f)
                     .ToArray();
                if (!checkFiles.Any())
                {
                    return ("No se encontraron archivos *_check.csv para verificar.");
                }

                foreach (var filePath in checkFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }
                    await CheckByInsertFile(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} procesado exitosamente para verificación.");
                }

                // Procesar archivos *_insert.csv
                var insertFiles = Directory.GetFiles(_tt2DirectoryPath, "*_insert.csv").ToList().OrderBy(f => f)
                     .ToArray();
                if (!insertFiles.Any())
                {
                    return ("No se encontraron archivos *_insert.csv para insertar.");
                }

                foreach (var filePath in insertFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }
                    await CreateNewFileFromInsert(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} procesado exitosamente para inserción.");
                }

                return ("Proceso completado para todos los archivos.");
            }
            catch (Exception ex)
            {
                return  ($"Error al subir los archivos: {ex.Message}");
            }           
        }

        public async Task<string> BulkInsertAllAsset(TT2ValidationDTO request)
        {
            try
            {
                // Obtener todos los archivos *_create.csv
                var createFiles = Directory.GetFiles(_tt2DirectoryPath, "*_create.csv").OrderBy(f => f)
                     .ToArray();
                if (!createFiles.Any())
                {
                    return ("No se encontraron archivos *_create.csv para procesar.");
                }

                foreach (var filePath in createFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }
                    await BulkInsertUsingCopy(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} procesado exitosamente.");
                }

                return ("Archivos procesados e insertados correctamente.");
            }
            catch (Exception ex)
            {
                return ( $"Error al insertar los registros: {ex.Message}");
            }
        }

        public async Task<string> ReadTt2Update(TT2ValidationDTO request)
        {
            try
            {
                // Obtener todos los archivos CSV en la carpeta que terminan en _update.csv
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_update.csv").OrderBy(f => f)
                     .ToArray();

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }
                    await UpdateAllAssetbyTT2(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} procesado exitosamente.");
                }

                return ("Proceso completado para todos los archivos.");
            }
            catch (Exception ex)
            {
                return ( $"Error al procesar los archivos: {ex.Message}");
            }
        }

        public async Task<string> ReadTt2UpdateCheck(TT2ValidationDTO request)
        {
            try
            {
                // Obtener todos los archivos CSV en la carpeta que terminan en _update.csv
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_check.csv").OrderBy(f => f)
                     .ToArray();

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 10)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!filePath.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }
                    await UpdateAllAssetbyTT2Check(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} procesado exitosamente.");
                }

                return ("Proceso completado para todos los archivos.");
            }
            catch (Exception ex)
            {
                return ($"Error al procesar los archivos: {ex.Message}");
            }
        }

        private async Task UpdateAllAssetbyTT2(string filePath)
        {

            var updates = new List<AllAsset>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, true))
                {
                    int lineNumber = 0;
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        lineNumber++;
                        var values = line.Split(new char[] { ',', ';' });

                        try
                        {
                            //ESSA
                            var update = new AllAsset
                            {
                                CodeSig = values[0],
                                Uia = values[1],
                                Longitude = float.Parse(values[8]),
                                Latitude = float.Parse(values[9]),

                                Group015 = values[3],
                                State = int.Parse(values[11]),
                                DateUnin = !string.IsNullOrEmpty(values[12]) ? ParseDate(values[12]) : (DateOnly?)null
                            };



                            updates.Add(update);

                            if (updates.Count >= _batchSize)
                            {
                                await UpdateBatchAllAssetbyTT2(updates, connection);
                                updates.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error procesando el archivo {filePath} en la línea {lineNumber}: {ex.Message}");
                        }
                    }

                    // Actualizar cualquier remanente que no alcanzó el tamaño del lote
                    if (updates.Count > 0)
                    {
                        await UpdateBatchAllAssetbyTT2(updates, connection);
                    }
                }
            }
        }

        private async Task UpdateBatchAllAssetbyTT2(List<AllAsset> updates, NpgsqlConnection connection)
        {
            try
            {
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var updateQuery = new StringBuilder();
                    updateQuery.Append("WITH update_values (code_sig, uia, latitude, longitude, group015, state, date_unin) AS (VALUES ");

                    for (int i = 0; i < updates.Count; i++)
                    {
                        if (i > 0) updateQuery.Append(",");
                        updateQuery.Append($"(@codeSig{i}, @uia{i}, @latitude{i}, @longitude{i}, @group015{i}, @state{i}, @dateUnin{i})");
                    }

                    updateQuery.Append(") ");
                    updateQuery.Append("UPDATE public.all_asset SET latitude = uv.latitude, longitude = uv.longitude, group015 = uv.group015, state = uv.state, date_unin = uv.date_unin ");
                    updateQuery.Append("FROM update_values uv ");
                    updateQuery.Append("WHERE public.all_asset.code_sig = uv.code_sig AND public.all_asset.uia = uv.uia;");

                    var updateCommand = new NpgsqlCommand(updateQuery.ToString(), connection);

                    for (int i = 0; i < updates.Count; i++)
                    {
                        updateCommand.Parameters.AddWithValue($"@codeSig{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].CodeSig);
                        updateCommand.Parameters.AddWithValue($"@uia{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].Uia);
                        updateCommand.Parameters.AddWithValue($"@latitude{i}", NpgsqlTypes.NpgsqlDbType.Real, updates[i].Latitude);
                        updateCommand.Parameters.AddWithValue($"@longitude{i}", NpgsqlTypes.NpgsqlDbType.Real, updates[i].Longitude);
                        updateCommand.Parameters.AddWithValue($"@group015{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].Group015);
                        updateCommand.Parameters.AddWithValue($"@state{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].State);
                        updateCommand.Parameters.AddWithValue($"@dateUnin{i}", NpgsqlTypes.NpgsqlDbType.Date, updates[i].DateUnin ?? (object)DBNull.Value);
                    }

                    await updateCommand.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar lote: {ex.Message}");
            }
        }        

        private async Task UpdateAllAssetbyTT2Check(string filePath)
        {

            var updates = new List<AllAsset>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, true))
                {
                    int lineNumber = 0;
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        lineNumber++;
                        var values = line.Split(new char[] { ',', ';' });

                        try
                        {
                            //ESSA
                            var update = new AllAsset
                            {
                                
                                Uia = values[1],                                
                                DateInst = !string.IsNullOrEmpty(values[12]) ? ParseDate(values[12]) : (DateOnly?)null
                            };



                            updates.Add(update);

                            if (updates.Count >= _batchSize)
                            {
                                await UpdateBatchAllAssetbyTT2Check(updates, connection);
                                updates.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error procesando el archivo {filePath} en la línea {lineNumber}: {ex.Message}");
                        }
                    }

                    // Actualizar cualquier remanente que no alcanzó el tamaño del lote
                    if (updates.Count > 0)
                    {
                        await UpdateBatchAllAssetbyTT2Check(updates, connection);
                    }
                }
            }
        }

        private async Task UpdateBatchAllAssetbyTT2Check(List<AllAsset> updates, NpgsqlConnection connection)
        {
            try
            {
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var updateQuery = new StringBuilder();
                    updateQuery.Append("WITH update_values_check (uia, date_inst) AS (VALUES ");

                    for (int i = 0; i < updates.Count; i++)
                    {
                        if (i > 0) updateQuery.Append(",");
                        updateQuery.Append($"(@uia{i}, @dateInst{i})");
                    }

                    updateQuery.Append(") ");
                    updateQuery.Append("UPDATE public.all_asset SET date_inst = uv.date_inst ");
                    updateQuery.Append("FROM update_values_check uv ");
                    updateQuery.Append("WHERE public.all_asset.uia = uv.uia;");

                    var updateCommand = new NpgsqlCommand(updateQuery.ToString(), connection);

                    for (int i = 0; i < updates.Count; i++)
                    {                        
                        updateCommand.Parameters.AddWithValue($"@uia{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].Uia);                        
                        updateCommand.Parameters.AddWithValue($"@dateInst{i}", NpgsqlTypes.NpgsqlDbType.Date, updates[i].DateInst ?? (object)DBNull.Value);
                    }

                    await updateCommand.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar lote: {ex.Message}");
            }
        }

        private async Task CreateTT2Files(string filePath)
        {
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    lines.Add(line.Replace(",", ";"));
                }
            }

            if (!lines.Any())
            {
                Console.WriteLine($"El archivo {filePath} está vacío. No se encontraron registros para verificar.");
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string checkFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_check.csv");            
            string updateFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_update.csv");

            var linesToCheck = lines.Where(line =>
            {
                var columns = line.Split(';');
                return columns.Length > 10 && columns[11] == "2";
            }).ToList();

            var linesToUpdate = lines.Where(line =>
            {
                var columns = line.Split(';');
                return columns.Length > 10 && columns[11] == "3";
            }).ToList();

            if (linesToCheck.Any())
            {
                await WriteAllLinesWithoutTrailingNewline(checkFilePath, linesToCheck);
            }

            if (linesToUpdate.Any())
            {
                await WriteAllLinesWithoutTrailingNewline(updateFilePath, linesToUpdate);
            }
        }

        private async Task WriteAllLinesWithoutTrailingNewline(string path, List<string> lines)
        {
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    // Asegurar que los separadores sean siempre ';'
                    var formattedLine = lines[i].Replace(",", ";");

                    if (i == lines.Count - 1)
                    {
                        await writer.WriteAsync(formattedLine);
                    }
                    else
                    {
                        await writer.WriteLineAsync(formattedLine);
                    }
                }
            }
        }

        private async Task CheckByInsertFile(string filePath)
        {
            var checkLines = new List<string>();

            // Leer el archivo _check.csv
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    checkLines.Add(await reader.ReadLineAsync());
                }
            }

            if (!checkLines.Any())
            {
                Console.WriteLine($"El archivo {filePath} está vacío. No se encontraron registros para procesar.");
                return;
            }

            var assetsToCheck = checkLines.Select(line =>
            {
                var values = line.Split(new char[] { ',', ';' });
                return new { CodeSig = values[0], Uia = values[1], OriginalLine = line };
            }).ToList();

            var codeSigUiaPairs = assetsToCheck.Select(a => $"('{a.CodeSig}', '{a.Uia}')").ToList();
            var codeSigUiaPairsString = string.Join(",", codeSigUiaPairs);

            var missingAssets = new List<string>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = $@"
                            SELECT check_assets.code_sig, check_assets.uia
                            FROM (VALUES {codeSigUiaPairsString}) AS check_assets (code_sig, uia)
                            LEFT JOIN public.all_asset AS aa
                            ON check_assets.code_sig = aa.code_sig AND check_assets.uia = aa.uia
                            WHERE aa.code_sig IS NULL AND aa.uia IS NULL";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string codeSig = reader.GetString(0);
                            string uia = reader.GetString(1);

                            var originalLine = assetsToCheck
                                .FirstOrDefault(a => a.CodeSig == codeSig && a.CodeSig != "N/A" && a.Uia == uia)?.OriginalLine;

                            if (originalLine != null)
                            {
                                missingAssets.Add(originalLine);
                            }
                        }
                    }
                }

                if (missingAssets.Any())
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    string insertFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension.Replace("_check", "_insert")}.csv");
                    await WriteAllLinesWithoutTrailingNewline(insertFilePath, missingAssets);
                }
                else
                {
                    Console.WriteLine("No se encontraron activos faltantes. No se creó ningún archivo.");
                }
            }

        }

        private async Task CreateNewFileFromInsert(string insertFilePath)
        {
            var createLines = new List<string>();
            var missingCodeSigLines = new List<string>();

            // Extraer el nombre del archivo sin la extensión
            var fileName = Path.GetFileNameWithoutExtension(insertFilePath);            

            // Obtener los primeros 4 dígitos como el año
            int yearTemp = int.Parse(fileName.Substring(0, 4));

            // Obtener los siguientes 2 dígitos como el mes
            int monthTemp = int.Parse(fileName.Substring(4, 2));

            // Leer el archivo _insert.csv
            var insertLines = new List<string>();
            using (var reader = new StreamReader(insertFilePath))
            {
                while (!reader.EndOfStream)
                {
                    insertLines.Add(await reader.ReadLineAsync());
                }
            }

            if (!insertLines.Any())
            {
                Console.WriteLine($"El archivo {insertFilePath} está vacío. No se encontraron registros para procesar.");
                return;
            }

            var assetsToProcess = insertLines.Select(line =>
            {
                var values = line.Split(new char[] { ',', ';' });
                return new
                {
                    CodeSig = values.ElementAtOrDefault(0) ?? "-1",
                    Uia = values.ElementAtOrDefault(1) ?? "-1",
                    State = values.ElementAtOrDefault(11) ?? "2",
                    DateInst = values.ElementAtOrDefault(12) ?? null,
                    OriginalLine = line
                };
            }).ToList();

            var createAssets = new List<AllAsset>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var asset in assetsToProcess)
                {
                    var query = @"
                        SELECT * 
                        FROM public.all_asset
                        WHERE code_sig = @CodeSig";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodeSig", asset.CodeSig);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var newAsset = new AllAsset
                                {
                                    TypeAsset = reader["type_asset"]?.ToString() ?? "-1",
                                    CodeSig = reader["code_sig"]?.ToString() ?? "-1",
                                    Uia = asset.Uia,
                                    Codetaxo = reader["codetaxo"]?.ToString() ?? "-1",
                                    Fparent = reader["fparent"]?.ToString() ?? "-1",
                                    Latitude = reader["latitude"] as float? ?? 0,
                                    Longitude = reader["longitude"] as float? ?? 0,
                                    Poblation = reader["poblation"]?.ToString() ?? "-1",
                                    Group015 = reader["group015"]?.ToString() ?? "-1",
                                    DateInst = DateOnly.TryParse(asset.DateInst, out DateOnly parsedDateInst) ? parsedDateInst : null,
                                    DateUnin = new DateOnly(2099, 12, 31),
                                    State = int.TryParse(asset.State, out int parsedState) ? parsedState : 2,
                                    Uccap14 = reader["uccap14"]?.ToString() ?? "-1",                                    
                                    IdRegion = reader["id_region"] as long? ?? -1,
                                    NameRegion = reader["name_region"]?.ToString() ?? "NO DATA",
                                    Address = reader["address"]?.ToString() ?? "-1",
                                    Year = yearTemp,
                                    Month = monthTemp,
                                };

                                createAssets.Add(newAsset);
                            }
                            else
                            {
                                // Agregar la línea al archivo _noCodeSig si no se encuentra el CodeSig
                                missingCodeSigLines.Add(asset.OriginalLine);
                            }
                        }
                    }
                }
            }

            // Verificar si se obtuvieron registros de tipo AllAsset
            if (!createAssets.Any() && !missingCodeSigLines.Any())
            {
                Console.WriteLine("No se generaron registros ni se encontraron CodeSig faltantes para el archivo proporcionado.");
                return;
            }

            // Generar líneas para el archivo _create.csv
            createLines = createAssets.Select(asset =>
                $"{asset.TypeAsset},{asset.CodeSig},{asset.Uia},{asset.Codetaxo},{asset.Fparent}," +
                $"{asset.Latitude},{asset.Longitude},{asset.Poblation},{asset.Group015},{asset.DateInst?.ToString("yyyy-MM-dd")}," +
                $"{asset.DateUnin:yyyy-MM-dd},{asset.State},{asset.Uccap14}," +
                $"{asset.IdRegion},{asset.NameRegion}," +
                $"{asset.Address}, {asset.Year}, {asset.Month}"
            ).ToList();

            // Crear el archivo _create.csv
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(insertFilePath);
            string createFilePath = Path.Combine(Path.GetDirectoryName(insertFilePath), $"{fileNameWithoutExtension.Replace("_insert", "_create")}.csv");

            await WriteAllLinesWithoutTrailingNewline(createFilePath, createLines);

            // Crear el archivo _noCodeSig.csv si hay registros faltantes
            if (missingCodeSigLines.Any())
            {
                string noCodeSigFilePath = Path.Combine(Path.GetDirectoryName(insertFilePath), $"{fileNameWithoutExtension.Replace("_insert", "_errorCodeSig")}.csv");
                await WriteAllLinesWithoutTrailingNewline(noCodeSigFilePath, missingCodeSigLines);
                Console.WriteLine($"Se creó el archivo {noCodeSigFilePath} con los registros cuyo CodeSig no se encontró.");
            }
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

        private async Task BulkInsertUsingCopy(string filePath)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var linesToInsert = new List<string>();
                        var keysToCheck = new List<string>();

                        // Leer el archivo y construir las claves para verificar duplicados
                        using (var reader = new StreamReader(filePath))
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = await reader.ReadLineAsync();
                                var columns = line.Split(new char[] { ';' });

                                if (columns.Length != 18)
                                {
                                    throw new Exception($"Línea mal formada en {filePath}: {line}");
                                }

                                var key = $"{columns[1]};{columns[2]}"; // code_sig y uia como clave única
                                keysToCheck.Add(key);
                                linesToInsert.Add(line);
                            }
                        }

                        // Consultar duplicados existentes en la base de datos
                        var duplicatesInDatabase = new HashSet<string>();
                        if (keysToCheck.Any())
                        {
                            var queryKeys = string.Join(", ", keysToCheck.Select((k, i) => $"(@CodeSig{i}, @Uia{i})"));
                            var query = $@"
                                        SELECT code_sig, uia
                                        FROM public.all_asset
                                        WHERE (code_sig, uia) IN ({queryKeys})";

                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                for (int i = 0; i < keysToCheck.Count; i++)
                                {
                                    var parts = keysToCheck[i].Split(';');
                                    command.Parameters.AddWithValue($"@CodeSig{i}", parts[0]);
                                    command.Parameters.AddWithValue($"@Uia{i}", parts[1]);
                                }

                                using (var dbReader = await command.ExecuteReaderAsync())
                                {
                                    while (await dbReader.ReadAsync())
                                    {
                                        var duplicateKey = $"{dbReader.GetString(0)};{dbReader.GetString(1)}";
                                        duplicatesInDatabase.Add(duplicateKey);
                                    }
                                }
                            }
                        }

                        // Filtrar los registros válidos (no duplicados en la base de datos)
                        var validLines = linesToInsert.Where(line =>
                        {
                            var columns = line.Split(new char[] { ';' });
                            var key = $"{columns[1]};{columns[2]}";
                            return !duplicatesInDatabase.Contains(key);
                        }).ToList();

                        // Insertar los registros válidos en la base de datos
                        if (validLines.Any())
                        {
                            await InsertBlockUsingCopy(validLines, connection);
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Error al procesar el archivo {filePath}: {ex.Message}");
                    }
                }
            }
        }

        private async Task InsertBlockUsingCopy(List<string> lines, NpgsqlConnection connection)
        {
            //using (var writer = connection.BeginTextImport(@"
            //COPY public.all_asset (
            //    type_asset, code_sig, uia, codetaxo, fparent, latitude, longitude, poblation, group015,
            //    date_inst, date_unin, state, uccap14, id_zone, name_zone, id_region, name_region,
            //    id_locality, name_locality, id_sector, name_sector, geographical_code, address
            //) FROM STDIN (FORMAT csv, DELIMITER ';')"))

            using (var writer = connection.BeginTextImport(@"
            COPY public.all_asset (
                type_asset, code_sig, uia, codetaxo, fparent, latitude, longitude, poblation, group015,
                date_inst, date_unin, state, uccap14, id_region, name_region, address, year, month
            ) FROM STDIN (FORMAT csv, DELIMITER ';')"))
            {
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }
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
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }

        private DateOnly ParseDateTemp(string dateString)
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
