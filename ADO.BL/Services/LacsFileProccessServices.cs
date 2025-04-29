using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ADO.BL.Services
{
    public class LacsFileProcessServices : ILacsFileProcessServices
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _lacDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ILACValidationEssaServices lACValidationServices;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IMapper mapper;

        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public LacsFileProcessServices(IConfiguration configuration, 
            ILACValidationEssaServices _lACValidationServices,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _lacDirectoryPath = configuration["FilesLACPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            lACValidationServices = _lACValidationServices;
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesLacs(LacValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var lacQueueList = new List<LacQueueDTO>();
                var listStatusLac = new List<StatusFileDTO>();

                var listEnds = new List<string>()
                {
                    "_Correct",
                    "_Error",                    
                    "_Correct_unchange",
                    "_Correct_continues",
                    "_Correct_continuesInvalid",
                    "_Correct_closed",
                    "_Correct_closedInvalid"
                };

                foreach (var filePath in Directory.GetFiles(_lacDirectoryPath, "*.csv")
                                        .Where(file => !file.EndsWith("_Correct.csv")
                                                        && !file.EndsWith("_Error.csv")
                                                        && !file.EndsWith("_unchanged.csv")
                                                        && !file.EndsWith("_continues.csv")
                                                        && !file.EndsWith("_continuesInvalid.csv")
                                                        && !file.EndsWith("_closed.csv")
                                                        && !file.EndsWith("_closedInvalid.csv"))
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

                    var UnitStatusLac = new StatusFileDTO()
                    {
                        FileName = fileName
                    };
                    var exist = listStatusLac.FirstOrDefault(x => x.FileName == UnitStatusLac.FileName);
                    if (exist == null)
                    {
                        listStatusLac.Add(UnitStatusLac);
                    }

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    // Obtener los siguientes 2 dígitos como el mes
                    int day = int.Parse(fileName.Substring(6, 2));

                    var beginDate = ParseDateTemp($"{year}/{month}/{day}");
                    var endDate = beginDate.AddDays(-30);
                    var listDates = new StringBuilder();
                    var listFilesError = new StringBuilder();


                    while (endDate <= beginDate)
                    {
                        listDates.Append($"'{endDate.Day}-{endDate.Month}-{endDate.Year}',");
                        endDate = endDate.AddDays(1);
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDatesDef = listDates.ToString().Remove(listDates.Length - 1, 1);
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_lac where date_register in ({listDatesDef})";
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

                // validate previous status 60 days 
                var completed2 = await ReadSspdUnchanged();
                Console.WriteLine(completed2);
                var completed3 = await ReadSSpdContinues();
                Console.WriteLine(completed3);
                var completed4 = await ReadSspdUpdate();
                Console.WriteLine(completed4);

                var subgroupMap = mapper.Map<List<QueueStatusLac>>(listStatusLac);
                foreach (var item in subgroupMap)
                {
                    item.Status = 4;
                }
                var resultSave = await statusFileDataAccess.UpdateDataLACList(subgroupMap);

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
                // var files = Directory.GetFiles(_lacDirectoryPath, "*_unchanged.csv");

                var files = Directory.GetFiles(_lacDirectoryPath, "*_unchanged.csv")
                     .OrderBy(f => f)
                     .ToArray();

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
                var files = Directory.GetFiles(_lacDirectoryPath, "*_continues.csv").OrderBy(f => f)
                     .ToArray();

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
                var files = Directory.GetFiles(_lacDirectoryPath, "*_closed.csv")
                    .OrderBy(f => f)
                     .ToArray(); 

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
                        //updateCommand.Parameters.AddWithValue($"@endDate{i}", NpgsqlTypes.NpgsqlDbType.Timestamp, updates[i].EndDate ?? (object)DBNull.Value);
                        var endDate = updates[i].EndDate?.ToLocalTime() ?? (object)DBNull.Value;
                        updateCommand.Parameters.AddWithValue($"@endDate{i}", NpgsqlTypes.NpgsqlDbType.Timestamp, endDate);

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

        //private DateTime ParseDate(string dateString)
        //{
        //    foreach (var format in _timeFormats)
        //    {
        //        if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        //        {
        //            return parsedDate.ToUniversalTime();
        //        }
        //    }
        //    return DateTime.ParseExact("31/12/2099 00:00:00", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToUniversalTime();
        //}

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate; // o .ToUniversalTime() si tu columna es timestamptz
                }
            }
            return DateTime.ParseExact("31/12/2099 00:00:00", "dd/MM/yyyy HH:mm:ss", _spanishCulture);
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

                                //var startDateDef = !string.IsNullOrEmpty(values[1]) ? DateTime.Parse(values[1]) : DateTime.Parse($"{values[2].Split(' ')[0]} 00:00:00");
                                //var endDateDef = !string.IsNullOrEmpty(values[2]) ? DateTime.Parse(values[2]) : DateTime.Parse($"{values[1].Split(' ')[0]} 23:59:59");

                                var startDateDef = !string.IsNullOrEmpty(values[1])
                                ? ParseDate(values[1])
                                : ParseDate($"{values[2].Split(' ')[0]} 00:00:00");

                                var endDateDef = !string.IsNullOrEmpty(values[2])
                                    ? ParseDate(values[2])
                                    : ParseDate($"{values[1].Split(' ')[0]} 23:59:59");

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

        private DateOnly ParseDateTemp(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
    }
}
