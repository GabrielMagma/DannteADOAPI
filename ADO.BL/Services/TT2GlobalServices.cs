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
    public class TT2GlobalServices : ITT2GlobalServices
    {
        private string _connectionString;
        private readonly string _connectionStringEssa;
        private readonly string _connectionStringEep;
        private readonly string _tt2DirectoryPath;
        private readonly string[] _timeFormats;
        private const int _batchSize = 10000;
        private readonly ITT2ValidationServices _Itt2ValidationServices;        
        private readonly IStatusFileEssaDataAccess statusFileEssaDataAccess;
        private readonly IMapper mapper;

        public TT2GlobalServices(IConfiguration configuration, 
            ITT2ValidationServices Itt2ValidationServices,            
            IStatusFileEssaDataAccess _statusFileEssaDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbConnection");
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
                _connectionString = _connectionStringEssa;
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
                    var completed2 = await ReadFileTT2Orginal();
                    Console.WriteLine(completed2);
                    var completed3 = await BulkInsertAllAsset();
                    Console.WriteLine(completed3);
                    var completed4 = await ReadTt2Update();
                    Console.WriteLine(completed4);

                    var subgroupMap = mapper.Map<List<StatusFile>>(errorFile.Data);
                    
                    var resultSave = await statusFileEssaDataAccess.SaveDataList(subgroupMap);
                    

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

        private async Task<string> ReadFileTT2Orginal()
        {
            try
            {
                // Obtener todos los archivos *_TT2_completed.csv
                var originalFiles = Directory.GetFiles(_tt2DirectoryPath, "*_completed.csv")
                    .Where(file => !file.EndsWith("_insert.csv") &&
                                   !file.EndsWith("_check.csv") &&
                                   !file.EndsWith("_update.csv"))
                    .ToList();

                if (!originalFiles.Any())
                {
                    return ("No se encontraron archivos *_completed.csv para procesar.");
                }

                foreach (var filePath in originalFiles)
                {
                    await CreateTT2Files(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente para particiones.");
                }

                // Procesar archivos *_check.csv
                var checkFiles = Directory.GetFiles(_tt2DirectoryPath, "*_check.csv").ToList();
                if (!checkFiles.Any())
                {
                    return ("No se encontraron archivos *_check.csv para verificar.");
                }

                foreach (var filePath in checkFiles)
                {
                    await CheckByInsertFile(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente para verificación.");
                }

                // Procesar archivos *_insert.csv
                var insertFiles = Directory.GetFiles(_tt2DirectoryPath, "*_insert.csv").ToList();
                if (!insertFiles.Any())
                {
                    return ("No se encontraron archivos *_insert.csv para insertar.");
                }

                foreach (var filePath in insertFiles)
                {
                    await CreateNewFileFromInsert(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente para inserción.");
                }

                return ("Proceso completado para todos los archivos.");
            }
            catch (Exception ex)
            {
                return  ($"Error al subir los archivos: {ex.Message}");
            }           
        }

        public async Task<string> BulkInsertAllAsset()
        {
            try
            {
                // Obtener todos los archivos *_create.csv
                var createFiles = Directory.GetFiles(_tt2DirectoryPath, "*_create.csv");
                if (!createFiles.Any())
                {
                    return ("No se encontraron archivos *_create.csv para procesar.");
                }

                foreach (var filePath in createFiles)
                {
                    await BulkInsertUsingCopy(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente.");
                }

                return ("Archivos procesados e insertados correctamente.");
            }
            catch (Exception ex)
            {
                return ( $"Error al insertar los registros: {ex.Message}");
            }
        }

        public async Task<string> ReadTt2Update()
        {
            try
            {
                // Obtener todos los archivos CSV en la carpeta que terminan en _update.csv
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_update.csv");

                foreach (var filePath in files)
                {
                    await UpdateAllAssetbyTT2(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente.");
                }

                return ("Proceso completado para todos los archivos.");
            }
            catch (Exception ex)
            {
                return ( $"Error al procesar los archivos: {ex.Message}");
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
                    State = values.ElementAtOrDefault(2) ?? "2",
                    DateInst = values.ElementAtOrDefault(10) ?? null,
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
                                    Address = reader["address"]?.ToString() ?? "-1"
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
                $"{asset.Address}"
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

                                if (columns.Length != 23)
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
            using (var writer = connection.BeginTextImport(@"
            COPY public.all_asset (
                type_asset, code_sig, uia, codetaxo, fparent, latitude, longitude, poblation, group015,
                date_inst, date_unin, state, uccap14, id_zone, name_zone, id_region, name_region,
                id_locality, name_locality, id_sector, name_sector, geographical_code, address
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
                if (DateOnly.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }

    }
}
