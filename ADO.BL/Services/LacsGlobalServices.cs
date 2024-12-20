
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class LacsGlobalServices : ILacsGlobalServices
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _lacDirectoryPath;
        private readonly string[] _timeFormats;

        public LacsGlobalServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PgDbConnection");
            _lacDirectoryPath = configuration["LacDirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
        }

        public async Task<ResponseQuery<List<string>>> ReadFileLacOrginal(ResponseQuery<List<string>> response)
        {
            try
            {
                var completed1 = await BeginProcess();
                Console.WriteLine(completed1);
                var completed2 = await ReadSspdUnchanged();
                Console.WriteLine(completed2);
                var completed3 = await ReadSSpdContinues();
                Console.WriteLine(completed3);
                var completed4 = await ReadSspdUpdate();
                Console.WriteLine(completed4);

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

        private async Task<string> BeginProcess()
        {
            try
            {
                Console.WriteLine("BeginProcess");
                // Obtener todos los archivos CSV en la carpeta que terminan en _withN.csv
                var files = Directory.GetFiles(_lacDirectoryPath, "*.csv")
                    .Where(file => !file.EndsWith("_unchanged.csv")
                                   && !file.EndsWith("_continues.csv")
                                   && !file.EndsWith("_continuesInvalid.csv")
                                   && !file.EndsWith("_closed.csv")
                                   && !file.EndsWith("_closedInvalid.csv"))
                    .ToList();

                foreach (var filePath in files)
                {
                   await CreateLacFiles(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }
                Console.WriteLine("EndBeginProcess");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        public async Task<string> ReadSspdUnchanged()
        {
            try
            {
                Console.WriteLine("ReadSspdUnchanged");
                var files = Directory.GetFiles(_lacDirectoryPath, "*_unchanged.csv");

                foreach (var filePath in files)
                {
                   await UploadLac(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }

                Console.WriteLine("EndReadSspdUnchanged");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
            
        }

        public async Task<string> ReadSSpdContinues()
        {
            try
            {
                Console.WriteLine("ReadSSpdContinues");
                var files = Directory.GetFiles(_lacDirectoryPath, "*_continues.csv");

                foreach (var filePath in files)
                {
                   await UploadLac(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }

                Console.WriteLine("EndReadSSpdContinues");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }            
        }

        public async Task<string> ReadSspdUpdate()
        {
            try
            {
                Console.WriteLine("ReadSspdUpdate");
                // Obtener todos los archivos CSV en la carpeta que terminan en _update.csv
                var files = Directory.GetFiles(_lacDirectoryPath, "*_closed.csv");

                foreach (var filePath in files)
                {
                   await UpdateLACbyLAC(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente.");
                }

                Console.WriteLine("EndReadSspdUpdate");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }            
        }

        private async Task UpdateLACbyLAC(string filePath)
        {
            const int batchSize = 10000; // Tamaño del lote
            var updates = new List<FilesLacDTO>();

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
                            var update = new FilesLacDTO
                            {
                                EventCode = values[0],
                                StartDate = !string.IsNullOrEmpty(values[1])
                                               ? ParseDate(values[1])
                                               : (!string.IsNullOrEmpty(values[2]) ? ParseDate($"{values[2].Split(' ')[0]} 00:00:00") : (DateTime?)null),

                                EndDate = !string.IsNullOrEmpty(values[2])
                                         ? ParseDate(values[2])
                                         : (!string.IsNullOrEmpty(values[1]) ? ParseDate($"{values[1].Split(' ')[0]} 23:59:59") : (DateTime?)null),
                                Uia = values[3],
                                ElementType = int.Parse(values[4]),
                                EventCause = int.Parse(values[5]),
                                EventContinues = values[6],
                                EventExcluidZin = int.Parse(values[7]),
                                AffectsConnection = int.Parse(values[8]),
                                LightingUsers = int.Parse(values[9])
                            };

                            updates.Add(update);

                            if (updates.Count >= batchSize)
                            {
                                await UpdateBatchLACbyLAC(updates, connection);
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
                        await UpdateBatchLACbyLAC(updates, connection);
                    }
                }
            }
        }

        private async Task UpdateBatchLACbyLAC(List<FilesLacDTO> updates, NpgsqlConnection connection)
        {
            try
            {
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var updateQuery = new StringBuilder();
                    updateQuery.Append("WITH update_values (event_code, end_date, uia, element_type, event_cause, event_continues, event_excluid_zin, affects_connection, lighting_users) AS (VALUES ");

                    for (int i = 0; i < updates.Count; i++)
                    {
                        if (i > 0) updateQuery.Append(",");
                        updateQuery.Append($"(@eventCode{i}, @endDate{i}, @uia{i}, @elementType{i}, @eventCause{i}, @eventContinues{i}, @eventExcluidZin{i}, @affectsConnection{i}, @lightingUsers{i})");
                    }

                    updateQuery.Append(") ");
                    updateQuery.Append("UPDATE public.files_lac SET end_date = uv.end_date, element_type = uv.element_type, event_cause = uv.event_cause, event_continues = uv.event_continues, event_excluid_zin = uv.event_excluid_zin, affects_connection = uv.affects_connection, lighting_users = uv.lighting_users ");
                    updateQuery.Append("FROM update_values uv ");
                    updateQuery.Append("WHERE public.files_lac.event_code = uv.event_code AND public.files_lac.uia = uv.uia;");

                    var updateCommand = new NpgsqlCommand(updateQuery.ToString(), connection);

                    for (int i = 0; i < updates.Count; i++)
                    {
                        updateCommand.Parameters.AddWithValue($"@eventCode{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].EventCode);
                        updateCommand.Parameters.AddWithValue($"@endDate{i}", NpgsqlTypes.NpgsqlDbType.Timestamp, updates[i].EndDate ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue($"@uia{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].Uia);
                        updateCommand.Parameters.AddWithValue($"@elementType{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].ElementType);
                        updateCommand.Parameters.AddWithValue($"@eventCause{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].EventCause);
                        updateCommand.Parameters.AddWithValue($"@eventContinues{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].EventContinues);
                        updateCommand.Parameters.AddWithValue($"@eventExcluidZin{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].EventExcluidZin);
                        updateCommand.Parameters.AddWithValue($"@affectsConnection{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].AffectsConnection);
                        updateCommand.Parameters.AddWithValue($"@lightingUsers{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].LightingUsers);
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

        private async Task CreateLacFiles(string filePath)
        {

            List<string> lines = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileUnchanged = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_unchanged.csv");
            string fileContinues = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_continues.csv");
            string fileContinuesInvalid = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_continuesInvalid.csv");
            string fileClosed = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_closed.csv");
            string fileClosedInvalid = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_closedInvalid.csv");

            var linesUnchanged = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool notEmptyFields = !(string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1])) &&
                                       !(string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]));
                return conditionN && notEmptyFields;
            }).ToList();

            var linesContinues = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionS && emptyFields;
            }).ToList();

            var linesContinuesInvalid = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionN && emptyFields;
            }).ToList();

            var linesClosed = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionN && emptyFields;
            }).ToList();

            var linesClosedInvalid = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionS && emptyFields;
            }).ToList();

            if (linesUnchanged.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileUnchanged, linesUnchanged);
            }

            if (linesContinues.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinues, linesContinues);
            }

            if (linesContinuesInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesInvalid, linesContinuesInvalid);
            }

            if (linesClosed.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosed, linesClosed);
            }

            if (linesClosedInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosedInvalid, linesClosedInvalid);
            }


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

        private void WriteAllLinesWithoutTrailingNewline(string path, List<string> lines)
        {
            using (var writer = new StreamWriter(path))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i == lines.Count - 1)
                    {
                        writer.Write(lines[i]);
                    }
                    else
                    {
                        writer.WriteLine(lines[i]);
                    }
                }
            }
        }

        private async Task UploadLac(string filePath)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var writer = connection.BeginBinaryImport(
                    $"COPY public.files_lac (event_code, start_date, end_date, uia, element_type, event_cause, " +
                    $"event_continues, event_excluid_zin, affects_connection, lighting_users, year, month ) " +
                    $"FROM STDIN (FORMAT BINARY)"
                ))
                {
                    using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
                    {
                        int lineNumber = 0;
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            lineNumber++;
                            var values = line.Split(new char[] { ',', ';' });

                            try
                            {
                                writer.StartRow();
                                writer.Write(values[0], NpgsqlTypes.NpgsqlDbType.Varchar); // event_code

                                var startDate = !string.IsNullOrEmpty(values[1])
                                               ? ParseDate(values[1])
                                               : (!string.IsNullOrEmpty(values[2]) ? ParseDate($"{values[2].Split(' ')[0]} 00:00:00") : (DateTime?)null);

                                var endDate = !string.IsNullOrEmpty(values[2])
                                             ? ParseDate(values[2])
                                             : (!string.IsNullOrEmpty(values[1]) ? ParseDate($"{values[1].Split(' ')[0]} 23:59:59") : (DateTime?)null);


                                writer.Write(startDate, NpgsqlTypes.NpgsqlDbType.Timestamp); // start_date
                                writer.Write(endDate, NpgsqlTypes.NpgsqlDbType.Timestamp); // end_date
                                writer.Write(values[3], NpgsqlTypes.NpgsqlDbType.Varchar); // uia
                                writer.Write(int.Parse(values[4]), NpgsqlTypes.NpgsqlDbType.Integer); // element_type
                                writer.Write(int.Parse(values[5]), NpgsqlTypes.NpgsqlDbType.Integer); // event_cause
                                writer.Write(values[6], NpgsqlTypes.NpgsqlDbType.Varchar); // event_continues
                                writer.Write(int.Parse(values[7]), NpgsqlTypes.NpgsqlDbType.Integer); // event_excluid_zin
                                writer.Write(int.Parse(values[8]), NpgsqlTypes.NpgsqlDbType.Integer); // affects_connection
                                writer.Write(int.Parse(values[9]), NpgsqlTypes.NpgsqlDbType.Integer); // lighting_users
                                writer.Write(startDate.Value.Year, NpgsqlTypes.NpgsqlDbType.Integer); // year
                                writer.Write(startDate.Value.Month, NpgsqlTypes.NpgsqlDbType.Integer); // month
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error procesando el archivo {filePath} en la línea {lineNumber}: {ex.Message}");
                            }
                        }
                    }

                    writer.Complete();
                }
            }
        }
    }
}
