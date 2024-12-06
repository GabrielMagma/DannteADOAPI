using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;

namespace ADO.BL.Services
{
    public class TT2Services : ITT2Services
    {
        private readonly string _connectionString;
        private readonly string _tt2DirectoryPath;
        private readonly string[] _timeFormats;
        private const int BatchSize = 5000;

        public TT2Services(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PgDbConnection");
            _tt2DirectoryPath = configuration["TT2DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
        }

        public ResponseQuery<List<string>> CompleteTT2Originals(ResponseQuery<List<string>> response)
        {
            try
            {
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_TT2.csv")
                    .Where(file => !file.EndsWith("_insert.csv") && !file.EndsWith("_check.csv") && !file.EndsWith("_update.csv"))
                    .ToList();

                foreach (var filePath in files)
                {
                    ProcessAndCompleteFile(filePath);
                }

                response.Message = "Proceso completado para todos los archivos";
                response.SuccessData = true;
                response.Success = true;
                return response;

            }
            //catch (SqliteException ex)
            //{
            //    response.Message = ex.Message;
            //    response.Success = false;
            //    response.SuccessData = false;
            //}
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

        public ResponseQuery<List<string>> CreateTT2Files(ResponseQuery<List<string>> response)
        {
            try
            {
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_TT2_completed.csv")
                    .Where(file => !file.EndsWith("_insert.csv") && !file.EndsWith("_check.csv") && !file.EndsWith("_update.csv"))
                    .ToList();

                foreach (var filePath in files)
                {
                    ProcessFileAndExecuteOperations(filePath);
                }

                response.Message = "Proceso completado para todos los archivos";
                response.SuccessData = true;
                response.Success = true;
                return response;

            }
            //catch (SqliteException ex)
            //{
            //    response.Message = ex.Message;
            //    response.Success = false;
            //    response.SuccessData = false;
            //}
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

        public ResponseQuery<List<string>> UpdateAllAssetByTT2(ResponseQuery<List<string>> response)
        {
            try
            {
                var files = Directory.GetFiles(_tt2DirectoryPath, "*_update.csv");

                foreach (var filePath in files)
                {
                    UpdateAllAssetbyTT2(filePath);
                }

                response.Message = "Proceso completado para todos los archivos";
                response.SuccessData = true;
                response.Success = true;
                return response;

            }
            //catch (SqliteException ex)
            //{
            //    response.Message = ex.Message;
            //    response.Success = false;
            //    response.SuccessData = false;
            //}
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

        private async Task UpdateAllAssetbyTT2(string filePath)
        {
            // Implementación detallada del proceso de actualización
        }

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }

        private async Task ProcessFileAndExecuteOperations(string filePath)
        {
            // Leer el archivo
            var lines = new List<string>();
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
            }

            // Crear archivos *_check.csv y *_update.csv
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string checkFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_check.csv");
            string updateFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_update.csv");

            var linesToCheck = lines.Where(line =>
            {
                var columns = line.Split(new char[] { ',', ';' });
                return columns.Length > 10 && columns[10] == "2"; // Estado 2
            }).ToList();

            var linesToUpdate = lines.Where(line =>
            {
                var columns = line.Split(new char[] { ',', ';' });
                return columns.Length > 10 && columns[10] == "3"; // Estado 3
            }).ToList();

            if (linesToCheck.Any())
            {
                await WriteAllLinesWithoutTrailingNewline(checkFilePath, linesToCheck);

                // Ejecutar funcionalidad check-by-insert automáticamente
                await ProcessCheckFile(checkFilePath);
            }

            if (linesToUpdate.Any())
            {
                await WriteAllLinesWithoutTrailingNewline(updateFilePath, linesToUpdate);
            }
        }

        private async Task ProcessCheckFile(string filePath)
        {
            var checkLines = new List<string>();
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    checkLines.Add(await reader.ReadLineAsync());
                }
            }

            var assetsToCheck = checkLines.Select(line =>
            {
                var values = line.Split(new char[] { ',', ';' });
                return new { CodeSig = values[0], Uia = values[1], OriginalLine = line };
            }).ToList();

            var missingAssets = new List<string>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var codeSigUiaPairs = assetsToCheck.Select(a => $"('{a.CodeSig}', '{a.Uia}')").ToList();
                var codeSigUiaPairsString = string.Join(",", codeSigUiaPairs);

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
                                .FirstOrDefault(a => a.CodeSig == codeSig && a.Uia == uia)?.OriginalLine;

                            if (originalLine != null)
                            {
                                missingAssets.Add(originalLine);
                            }
                        }
                    }
                }
            }

            if (missingAssets.Any())
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string insertFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_insert.csv");
                await WriteAllLinesWithoutTrailingNewline(insertFilePath, missingAssets);

                // Ejecutar funcionalidad create-from-insert automáticamente
                await CreateNewFileFromInsert(insertFilePath);
            }
        }

        private async Task CreateNewFileFromInsert(string filePath)
        {
            var insertLines = new List<string>();
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    insertLines.Add(await reader.ReadLineAsync());
                }
            }

            var createLines = new List<string>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var line in insertLines)
                {
                    var values = line.Split(new char[] { ',', ';' });

                    var query = "SELECT * FROM public.all_asset WHERE code_sig = @CodeSig";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CodeSig", values[0]);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Completa datos para la nueva línea
                                var newLine = $"{reader["type_asset"]},{reader["code_sig"]},{values[1]},...";
                                createLines.Add(newLine);
                            }
                        }
                    }
                }
            }

            if (createLines.Any())
            {
                string createFilePath = filePath.Replace("_insert.csv", "_create.csv");
                await WriteAllLinesWithoutTrailingNewline(createFilePath, createLines);
            }
        }

        private async Task WriteAllLinesWithoutTrailingNewline(string filePath, IEnumerable<string> lines)
        {
            using (var writer = new StreamWriter(filePath, false))
            {
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }
        }

        private async Task ProcessAndCompleteFile(string filePath)
        {
            // Implementación detallada del proceso completo
        }
    }
}
