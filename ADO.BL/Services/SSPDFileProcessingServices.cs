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
    public class SSPDFileProcessingServices : ISSPDFileProcessingServices
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;        
        private readonly string _sspdDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ISSPDValidationEepServices SSPDValidationServices;        
        private readonly IStatusFileDataAccess statusFileEssaDataAccess;
        private readonly IMapper mapper;

        public SSPDFileProcessingServices(IConfiguration configuration, 
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
                var lacQueueList = new List<LacQueueDTO>();

                var listStatusSspd = new List<StatusFileDTO>();

                var listEnds = new List<string>()
                {
                    "_Correct",
                    "_Error",                    
                    "_Correct_unchange",
                    "_Correct_continuesInsert",
                    "_Correct_continuesUpdate",
                    "_Correct_continuesInvalid",
                    "_Correct_closed",
                    "_Correct_closedInvalid",
                    "_Correct_delete",
                    "_Correct_update"
                };

                foreach (var filePath in Directory.GetFiles(_sspdDirectoryPath, "*.csv")
                                    .Where(file => !file.EndsWith("_Correct.csv")
                                                  && !file.EndsWith("_Error.csv")
                                                  && !file.EndsWith("_unchanged.csv")
                                                  && !file.EndsWith("_continuesInsert.csv")
                                                  && !file.EndsWith("_continuesUpdate.csv")
                                                  && !file.EndsWith("_continuesInvalid.csv")
                                                  && !file.EndsWith("_closed.csv")
                                                  && !file.EndsWith("_closedInvalid.csv")
                                                  && !file.EndsWith("_delete.csv")
                                                  && !file.EndsWith("_update.csv"))
                                    .ToList().OrderBy(f => f)
                     .ToArray()
                    )
                {

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    var nameTemp = fileName;

                    foreach (var item1 in listEnds)
                    {
                        nameTemp = nameTemp.Replace(item1, "");
                    }

                    var UnitStatusSspd = new StatusFileDTO()
                    {
                        FileName = fileName
                    };
                    var exist = listStatusSspd.FirstOrDefault(x => x.FileName == UnitStatusSspd.FileName);
                    if (exist == null)
                    {
                        listStatusSspd.Add(UnitStatusSspd);
                    }

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    var beginDate = DateOnly.Parse($"1/{month}/{year}");
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
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_sspd where date_register in ({listDatesDef})";
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

                var subgroupMap = mapper.Map<List<QueueStatusSspd>>(listStatusSspd);
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

        public async Task<string> ReadSspdUnchanged()
        {
            try
            {
                Console.WriteLine("ReadSspdUnchanged");
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_unchanged.csv").OrderBy(f => f)
                     .ToArray();

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
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_continuesInsert.csv").OrderBy(f => f)
                     .ToArray();

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
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_continuesUpdate.csv").OrderBy(f => f)
                     .ToArray();

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
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_update.csv").OrderBy(f => f)
                     .ToArray();

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
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_delete.csv").OrderBy(f => f)
                     .ToArray();

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
