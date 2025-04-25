using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml.Drawing.Style.Fill;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class TC1FileProcessingServices : ITC1FileProcessingServices
    {
        private string _connectionString;        
        private readonly string _assetsDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ITC1ValidationServices _ITC1ValidationServices;        
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IMapper mapper;

        public TC1FileProcessingServices(IConfiguration configuration, 
            ITC1ValidationServices Itc1ValidationServices,            
            IStatusFileDataAccess _statusFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");            
            _assetsDirectoryPath = configuration["Tc1DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _ITC1ValidationServices = Itc1ValidationServices;            
            statusFileDataAccess = _statusFileDataAccess;
            mapper = _mapper;
            
        }

        public async Task<ResponseQuery<List<string>>> ReadAssets(TC1ValidationDTO request, ResponseQuery<List<string>> response)
        {
            try
            {
                var lacQueueList = new List<LacQueueDTO>();

                var listStatusTc1 = new List<StatusFileDTO>();

                var listEnds = new List<string>()
                {
                    "_Correct",
                    "_Error"
                };

                foreach (var filePath in Directory.GetFiles(_assetsDirectoryPath, "*.csv")
                                        .Where(file => !file.EndsWith("_Correct.csv")
                                                        && !file.EndsWith("_Error.csv"))
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

                    var UnitStatusTc1 = new StatusFileDTO()
                    {
                        FileName = fileName
                    };
                    var exist = listStatusTc1.FirstOrDefault(x => x.FileName == UnitStatusTc1.FileName);
                    if (exist == null)
                    {
                        listStatusTc1.Add(UnitStatusTc1);
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
                        listDates.Append($"'1-{endDate.Month}-{endDate.Year}',");
                        endDate = endDate.AddMonths(1);
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDatesDef = listDates.ToString().Remove(listDates.Length - 1, 1);
                        var SelectQuery = $@"SELECT file_name, year, month, day, status FROM queues.queue_status_tc1 where date_register in ({listDatesDef})";
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

                var files = Directory.GetFiles(_assetsDirectoryPath, "*_Correct.csv").OrderBy(f => f)
                     .ToArray();  // OJO TOCA ESTANDARIZAR!!!

                foreach (var filePath in files)
                {
                    await InsertAssets(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }

                var subgroupMap = mapper.Map<List<QueueStatusTc1>>(listStatusTc1);
                foreach (var item in subgroupMap)
                {
                    item.Status = 4;
                }
                var resultSave = await statusFileDataAccess.UpdateDataTC1List(subgroupMap);


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

        private async Task InsertAssets(string filePath)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Extraer el nombre del archivo sin la extensión
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                // Asumiendo que el formato del archivo es YYYYMM_TC1.csv
                if (fileName.Length >= 6)
                {
                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    // Usar COPY para cargar datos directamente en la tabla definitiva
                    using (var writer = connection.BeginBinaryImport(
                         $"COPY public.files_tc1(niu, uia, year, month, files, files_date) " +
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
                                    // Iniciar una nueva fila
                                    writer.StartRow();
                                    writer.Write(values[0], NpgsqlTypes.NpgsqlDbType.Varchar); // Niu (de la primera columna del archivo)
                                    writer.Write(values[1], NpgsqlTypes.NpgsqlDbType.Varchar); // Uia (de la segunda columna del archivo)
                                    writer.Write(year, NpgsqlTypes.NpgsqlDbType.Integer); // Year (extraído del nombre del archivo)
                                    writer.Write(month, NpgsqlTypes.NpgsqlDbType.Integer); // Month (extraído del nombre del archivo)
                                    writer.Write(Path.GetFileName(filePath), NpgsqlTypes.NpgsqlDbType.Varchar); // Files (nombre del archivo)
                                    writer.Write(DateOnly.FromDateTime(DateTime.Today), NpgsqlTypes.NpgsqlDbType.Date); // FilesDate (fecha actual)
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"Error procesando el archivo {filePath} en la línea {lineNumber}: {ex.Message}");
                                }
                            }
                        }

                        // Finalizar la escritura
                        writer.Complete();
                    }
                }
                else
                {
                    throw new Exception("Formato de nombre de archivo no válido. Debe ser YYYYMM_TC1.csv");
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
