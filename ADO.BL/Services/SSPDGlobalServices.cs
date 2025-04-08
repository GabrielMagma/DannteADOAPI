using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class SSPDGlobalServices : ISSPDGlobalServices
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;        
        private readonly string _sspdDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ISSPDValidationEepServices SSPDValidationServices;        
        private readonly IStatusFileDataAccess statusFileEssaDataAccess;
        private readonly IMapper mapper;

        public SSPDGlobalServices(IConfiguration configuration, 
            ISSPDValidationEepServices _SSPDValidationServices,            
            IStatusFileDataAccess _statuFileEssaDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");            
            _sspdDirectoryPath = configuration["SspdDirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            SSPDValidationServices = _SSPDValidationServices;            
            statusFileEssaDataAccess = _statuFileEssaDataAccess;
            mapper = _mapper;

        }

        public async Task<ResponseQuery<List<string>>> ReadFileSspdOrginal(LacValidationDTO request, ResponseQuery<List<string>> response)
        {
            try
            {                
                var responseError = new ResponseEntity<List<StatusFileDTO>>();
                var viewErrors = await SSPDValidationServices.ValidationSSPD(request, responseError);
                if (viewErrors.Success == false)
                {
                    response.Message = "el archivo cargado tiene errores, por favor corregir";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    var completed1 = await BeginProcess();
                    Console.WriteLine(completed1);
                    var completed2 = await ReadSspdUnchanged();
                    Console.WriteLine(completed2);
                    var completed3 = await ReadSSpdContinuesInsert();
                    Console.WriteLine(completed3);
                    var completed4 = await ReadSSpdContinuesUpdate();
                    Console.WriteLine(completed4);
                    var completed5 = await ReadSspdUpdate();
                    Console.WriteLine(completed5);
                    var completed6 = await ReadSspdDelete();
                    Console.WriteLine(completed6);

                    var subgroupMap = mapper.Map<List<QueueStatusSspd>>(viewErrors.Data);
                    foreach (var item in subgroupMap)
                    {
                        item.Status = 4;
                    }
                    var resultSave = await statusFileEssaDataAccess.UpdateDataSSPDList(subgroupMap);


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
                // Obtener todos los archivos CSV en la carpeta que terminan en _withN.csv
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_Correct.csv")
                   .Where(file => !file.EndsWith("_unchanged.csv")
                                  && !file.EndsWith("_continuesInsert.csv")
                                  && !file.EndsWith("_continuesUpdate.csv")
                                  && !file.EndsWith("_continuesInvalid.csv")
                                  && !file.EndsWith("_closed.csv")
                                  && !file.EndsWith("_closedInvalid.csv")
                                  && !file.EndsWith("_delete.csv")
                                  && !file.EndsWith("_update.csv"))
                   .ToList();

                foreach (var filePath in files)
                {
                   await CreateSspdFiles(filePath);
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
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_unchanged.csv");

                foreach (var filePath in files)
                {
                   await UploadSSpd(filePath);
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

        public async Task<string> ReadSSpdContinuesInsert()
        {
            try
            {
                Console.WriteLine("ReadSSpdContinuesInsert");
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_continuesInsert.csv");

                foreach (var filePath in files)
                {
                   await UploadSSpd(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }

                Console.WriteLine("EndReadSSpdContinuesInsert");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        public async Task<string> ReadSSpdContinuesUpdate()
        {
            try
            {
                Console.WriteLine("ReadSSpdContinuesUpdate");
                // Obtener todos los archivos CSV en la carpeta que terminan en _update.csv
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_continuesUpdate.csv");

                foreach (var filePath in files)
                {
                   await UpdateLACbySSPD(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }

                Console.WriteLine("EndReadSSpdContinuesUpdate");
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
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_update.csv");

                foreach (var filePath in files)
                {
                   await UpdateLACbySSPD(filePath);
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

        public async Task<string> ReadSspdDelete()
        {
            try
            {
                Console.WriteLine("ReadSspdDelete");
                // Obtener todos los archivos CSV en la carpeta que terminan en _update.csv
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_delete.csv");

                foreach (var filePath in files)
                {
                   await DeleteLACbySSPD(filePath);
                    Console.WriteLine($"Archivo {filePath} procesado exitosamente.");
                }

                Console.WriteLine("EndReadSspdDelete");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        private async Task DeleteLACbySSPD(string filePath)
        {
            const int batchSize = 10000; // Tamaño del lote
            var eventCodes = new List<string>();
            var uias = new List<string>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
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
                                var eventCode = values[0];
                                var uia = values[3];
                                eventCodes.Add(eventCode);
                                uias.Add(uia);

                                if (eventCodes.Count >= batchSize)
                                {
                                    await DeleteBatch(eventCodes, uias, connection, transaction);
                                    eventCodes.Clear();
                                    uias.Clear();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error procesando el archivo {filePath} en la línea {lineNumber}: {ex.Message}");
                            }
                        }

                        // Eliminar cualquier remanente que no alcanzó el tamaño del lote
                        if (eventCodes.Count > 0)
                        {
                            await DeleteBatch(eventCodes, uias, connection, transaction);
                        }
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        private async Task DeleteBatch(List<string> eventCodes, List<string> uias, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            try
            {
                // Crear una tabla temporal para almacenar las tuplas
                var createTempTableCommand = new NpgsqlCommand(
                    "CREATE TEMP TABLE TempEventUia (event_code VARCHAR, uia VARCHAR)", connection, transaction);
                await createTempTableCommand.ExecuteNonQueryAsync();

                // Insertar las tuplas en la tabla temporal
                using (var writer = connection.BeginBinaryImport("COPY TempEventUia (event_code, uia) FROM STDIN (FORMAT BINARY)"))
                {
                    for (int i = 0; i < eventCodes.Count; i++)
                    {
                        writer.StartRow();
                        writer.Write(eventCodes[i], NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(uias[i], NpgsqlTypes.NpgsqlDbType.Varchar);
                    }
                    await writer.CompleteAsync();
                }

                // Eliminar las tuplas correspondientes en la tabla principal
                var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM public.files_lac " +
                    "USING TempEventUia " +
                    "WHERE public.files_lac.event_code = TempEventUia.event_code " +
                    "AND public.files_lac.uia = TempEventUia.uia", connection, transaction);
                await deleteCommand.ExecuteNonQueryAsync();

                // Eliminar la tabla temporal
                var dropTempTableCommand = new NpgsqlCommand("DROP TABLE TempEventUia", connection, transaction);
                await dropTempTableCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar lote: {ex.Message}");
            }
        }

        private async Task UpdateLACbySSPD(string filePath)
        {
            const int batchSize = 5000; // Tamaño del lote
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
                                         : (!string.IsNullOrEmpty(values[1]) ? ParseEndDate($"{values[1].Split(' ')[0]} 23:59:59") : (DateTime?)null),
                                Uia = values[3],
                                ElementType = int.Parse(values[4]),
                                EventCause = int.Parse(values[5]),
                                EventContinues = values[6],
                                EventExcluidZin = int.Parse(values[7]),
                                AffectsConnection = int.Parse(values[8]),
                                LightingUsers = int.Parse(values[9]),
                                State = int.Parse(values[10]),
                                FileCode = values[11]
                            };

                            updates.Add(update);

                            if (updates.Count >= batchSize)
                            {
                                await UpdateBatchLACbySSPD(updates, connection);
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
                        await UpdateBatchLACbySSPD(updates, connection);
                    }
                }
            }
        }

        private async Task UpdateBatchLACbySSPD(List<FilesLacDTO> updates, NpgsqlConnection connection)
        {
            try
            {
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var updateQuery = new StringBuilder();
                    updateQuery.Append("WITH update_values (event_code, start_date, end_date, uia, element_type, event_cause, event_continues, event_excluid_zin, affects_connection, lighting_users, state, file_code) AS (VALUES ");

                    for (int i = 0; i < updates.Count; i++)
                    {
                        if (i > 0) updateQuery.Append(",");
                        updateQuery.Append($"(@eventCode{i}, @startDate{i}, @endDate{i}, @uia{i}, @elementType{i}, @eventCause{i}, @eventContinues{i}, @eventExcluidZin{i}, @affectsConnection{i}, @lightingUsers{i}, @state{i}, @fileCode{i})");
                    }

                    updateQuery.Append(") ");
                    updateQuery.Append("UPDATE public.files_lac SET start_date = uv.start_date, end_date = uv.end_date, element_type = uv.element_type, event_cause = uv.event_cause, event_continues = uv.event_continues, event_excluid_zin = uv.event_excluid_zin, affects_connection = uv.affects_connection, lighting_users = uv.lighting_users, state = uv.state, file_code = uv.file_code ");
                    updateQuery.Append("FROM update_values uv ");
                    updateQuery.Append("WHERE public.files_lac.event_code = uv.event_code AND public.files_lac.uia = uv.uia;");

                    var updateCommand = new NpgsqlCommand(updateQuery.ToString(), connection);

                    for (int i = 0; i < updates.Count; i++)
                    {
                        updateCommand.Parameters.AddWithValue($"eventCode{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].EventCode);
                        updateCommand.Parameters.AddWithValue($"startDate{i}", NpgsqlTypes.NpgsqlDbType.Timestamp, updates[i].StartDate);
                        updateCommand.Parameters.AddWithValue($"endDate{i}", NpgsqlTypes.NpgsqlDbType.Timestamp, updates[i].EndDate);
                        updateCommand.Parameters.AddWithValue($"uia{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].Uia);
                        updateCommand.Parameters.AddWithValue($"elementType{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].ElementType);
                        updateCommand.Parameters.AddWithValue($"eventCause{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].EventCause);
                        updateCommand.Parameters.AddWithValue($"eventContinues{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].EventContinues);
                        updateCommand.Parameters.AddWithValue($"eventExcluidZin{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].EventExcluidZin);
                        updateCommand.Parameters.AddWithValue($"affectsConnection{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].AffectsConnection);
                        updateCommand.Parameters.AddWithValue($"lightingUsers{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].LightingUsers);
                        updateCommand.Parameters.AddWithValue($"state{i}", NpgsqlTypes.NpgsqlDbType.Integer, updates[i].State);
                        updateCommand.Parameters.AddWithValue($"fileCode{i}", NpgsqlTypes.NpgsqlDbType.Varchar, updates[i].FileCode);
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

        private async Task CreateSspdFiles(string filePath)
        {
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
            }

            // Crear los nombres de los archivos adicionales basados en el nombre del archivo original
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileUnchanged = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_unchanged.csv");
            string fileContinuesInsert = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_continuesInsert.csv");
            string fileContinuesUpdate = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_continuesUpdate.csv");
            string fileContinuesInvalid = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_continuesInvalid.csv");
            string fileClosed = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_closed.csv");
            string fileClosedInvalid = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_closedInvalid.csv");
            string fileToDelete = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_delete.csv");
            string fileToUpdate = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_update.csv");

            // Filtrar las líneas que contienen "N" en la columna 6 y columna 10 = 1 para agregar
            var linesUnchanged = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool conditionAdd = columns.Length > 10 && columns[10] == "1";
                bool notEmptyFields = !(string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1])) &&
                                       !(string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]));
                return conditionN && conditionAdd && notEmptyFields;
            }).ToList();


            // Filtrar las líneas que contienen "S" en la columna 6 y columna 10 = 2 para agregar
            var linesContinuesInsert = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool conditionAdd = columns.Length > 10 && columns[10] == "1";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionS && conditionAdd && emptyFields;
            }).ToList();


            // Filtrar las líneas que contienen "S" en la columna 6 y columna 10 = 2 para agregar
            var linesContinuesUpdate = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionS && conditionAdd && emptyFields;
            }).ToList();


            var linesContinuesInvalid = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionN && conditionAdd && emptyFields;
            }).ToList();


            var linesClosed = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionN && conditionAdd && emptyFields;
            }).ToList();

            var linesClosedInvalid = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionS && conditionAdd && emptyFields;
            }).ToList();

            // Filtrar las líneas donde columna 10 = 3 para eliminar
            var linesToDelete = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionDelete = columns.Length > 10 && columns[10] == "3";
                return conditionDelete;
            }).ToList();

            // Filtrar las líneas donde columna 10 = 2 para actualizar
            var linesToUpdate = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionUpdate = columns.Length > 10 && columns[10] == "2";
                bool notEmptyFields = !(string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1])) &&
                                       !(string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]));
                return conditionUpdate && notEmptyFields;
            }).ToList();

            // Escribir las líneas con "N" y columna 10 = 1 en el archivo correspondiente
            if (linesUnchanged.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileUnchanged, linesUnchanged);
            }

            // Escribir las líneas con "S" y columna 10 = 1 en el archivo correspondiente
            if (linesContinuesInsert.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesInsert, linesContinuesInsert);
            }


            // Escribir las líneas con "S" y columna 10 = 2 en el archivo correspondiente
            if (linesContinuesUpdate.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesUpdate, linesContinuesUpdate);
            }

            // Escribir las líneas con "N" y columna 10 = 2 en el archivo correspondiente
            if (linesContinuesInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesInvalid, linesContinuesInvalid);
            }

            // Escribir las líneas con "N" y columna 10 = 2 en el archivo correspondiente
            if (linesClosed.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosed, linesClosed);
            }

            // Escribir las líneas con "S" y columna 10 = 2 en el archivo correspondiente
            if (linesClosedInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosedInvalid, linesClosedInvalid);
            }

            // Escribir las líneas con columna 10 = 3 en el archivo correspondiente
            if (linesToDelete.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileToDelete, linesToDelete);
            }

            // Escribir las líneas con columna 10 = 2 en el archivo correspondiente
            if (linesToUpdate.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileToUpdate, linesToUpdate);
            }

        }

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate; // Establecer la hora a las 00:00:00
                }
            }
            throw new FormatException($"El formato de fecha {dateString} no es válido.");
        }

        private DateTime ParseEndDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    // Ajustar la fecha al último día del mes con la hora 23:59:59
                    DateTime lastDayOfMonth = new DateTime(parsedDate.Year, parsedDate.Month, DateTime.DaysInMonth(parsedDate.Year, parsedDate.Month), 23, 59, 59);
                    return lastDayOfMonth;
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

        private async Task UploadSSpd(string filePath)
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


                                var startDateDef = !string.IsNullOrEmpty(values[1]) ? DateTime.Parse(values[1]) : DateTime.Parse($"{values[2].Split(' ')[0]} 00:00:00");
                                var endDateDef = !string.IsNullOrEmpty(values[2]) ? DateTime.Parse(values[2]) : DateTime.Parse($"{values[1].Split(' ')[0]} 23:59:59");                                

                                writer.Write(startDateDef, NpgsqlTypes.NpgsqlDbType.Timestamp); // start_date
                                writer.Write(endDateDef, NpgsqlTypes.NpgsqlDbType.Timestamp); // end_date                                
                                writer.Write(values[3], NpgsqlTypes.NpgsqlDbType.Varchar); // uia
                                writer.Write(int.Parse(values[4]), NpgsqlTypes.NpgsqlDbType.Integer); // element_type
                                writer.Write(int.Parse(values[5]), NpgsqlTypes.NpgsqlDbType.Integer); // event_cause
                                writer.Write(values[6], NpgsqlTypes.NpgsqlDbType.Varchar); // event_continues
                                writer.Write(int.Parse(values[7]), NpgsqlTypes.NpgsqlDbType.Integer); // event_excluid_zin
                                writer.Write(int.Parse(values[8]), NpgsqlTypes.NpgsqlDbType.Integer); // affects_connection
                                writer.Write(int.Parse(values[9]), NpgsqlTypes.NpgsqlDbType.Integer); // lighting_users
                                writer.Write(startDateDef.Year, NpgsqlTypes.NpgsqlDbType.Integer); // year
                                writer.Write(startDateDef.Month, NpgsqlTypes.NpgsqlDbType.Integer); // month
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
