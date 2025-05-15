using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileIdeamValidationServices : IFileIdeamValidationServices
    {
        
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _connectionString;
        private readonly string _IdeamDirectoryPath;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        public FileIdeamValidationServices(IConfiguration configuration,
            IStatusFileDataAccess _statuFileDataAccess,
            IHubContext<NotificationHub> hubContext,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _IdeamDirectoryPath = configuration["FilesIdeamPath"];
            statusFileDataAccess = _statuFileDataAccess;
            _hubContext = hubContext;
            mapper = _mapper;
            
            
        }

        public async Task<ResponseQuery<bool>> ReadFilesIdeam(RayosValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {

                string inputFolder = _IdeamDirectoryPath;

                //Procesar cada archivo.csv en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv")
                                        .Where(file => !file.EndsWith("_Correct.csv")
                                        && !file.EndsWith("_Error.csv"))
                                        .OrderBy(f => f).ToArray())
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

                    var statusFilesingle = new StatusFileDTO();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "ASSETS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;
                    statusFilesingle.Status = 1;
                    statusFilesingle.DateRegister = ParseDate($"1/{month}/{year}");

                    List<IaIdeam> ideamListData = new List<IaIdeam>();
                    List<Register> valueFinal = new List<Register>();
                    Register register = new Register();
                    var ideamCompList = new List<IdeamCompDTO>();
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} se está validando");

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 11; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }

                    dataTableError.Columns.Add($"C1");
                    dataTableError.Columns.Add($"C2");

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var SelectQuery = $@"SELECT code, latitude, longitude, altitude, department, municipality FROM maps.mp_ideam_comp";
                        using (var reader = new NpgsqlCommand(SelectQuery, connection))
                        {
                            try
                            {

                                using (var result = await reader.ExecuteReaderAsync())
                                {
                                    while (await result.ReadAsync())
                                    {
                                        var ideamElement = new IdeamCompDTO();
                                        ideamElement.code = result[0].ToString();
                                        ideamElement.latitude = float.Parse(result[1].ToString());
                                        ideamElement.longitude = float.Parse(result[2].ToString());
                                        ideamElement.altitude = float.Parse(result[3].ToString());
                                        ideamElement.department = result[4].ToString();
                                        ideamElement.municipality = result[5].ToString();
                                        ideamCompList.Add(ideamElement);
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

                    string[] fileLines = File.ReadAllLines(filePath);

                    

                    var count = 1;
                    foreach (var item in fileLines)
                    {
                        IdeamDTO ideam = new IdeamDTO();
                        var valueLines = item.Split(',', ';');

                        if (valueLines.Count() != 8)
                        {
                            response.SuccessData = false;
                            response.Message = "Formato Incorrecto, favor corregirlo";
                            response.Success = false;
                            return response;
                        }

                        if (valueLines[0] != "CodigoEstacion")
                        {

                            var ideamTemp = ideamCompList.FirstOrDefault(x => x.code == valueLines[0]);

                            if (ideamTemp == null)
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"No existen datos para este código registro en la data de la fila {count}";
                                var rowText = new StringBuilder();
                                foreach (var line in valueLines)
                                {
                                    rowText.Append($"{line}, ");
                                }
                                newRowError[1] = rowText;
                                dataTableError.Rows.Add(newRowError);
                                count++;
                                continue;
                            }

                            var date = ParseDate(valueLines[4]).ToString();

                            if (date == "31/12/2099") 
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fecha en la data de la fila {count}";
                                var rowText = new StringBuilder();
                                foreach (var line in valueLines)
                                {                                    
                                    rowText.Append($"{line}, ");
                                }
                                newRowError[1] = rowText;
                                dataTableError.Rows.Add(newRowError);
                                count++;
                                continue;

                            }

                            var newRow = dataTable.NewRow();

                            newRow[0] = valueLines[0];
                            newRow[1] = valueLines[1];
                            newRow[2] = ideamTemp.latitude.ToString();
                            newRow[3] = ideamTemp.longitude.ToString();
                            newRow[4] = ideamTemp.altitude.ToString();
                            newRow[5] = ideamTemp.department.ToString();
                            newRow[6] = ideamTemp.municipality.ToString();
                            newRow[7] = "PRECIPITACION";
                            newRow[8] = "DIARIA";
                            newRow[9] = date;
                            newRow[10] = valueLines[6];

                            dataTable.Rows.Add(newRow);
                            count++;
                        }
                    }

                    response.SuccessData = true;
                    response.Message = "Archivo validado con éxito";
                    response.Success = true;

                    if (dataTable.Rows.Count > 0)
                    {
                        RegisterCorrect(dataTable, inputFolder, fileName);
                    }

                    if (dataTableError.Rows.Count > 0)
                    {
                        statusFilesingle.Status = 2;
                        RegisterError(dataTableError, inputFolder, fileName);
                        response.SuccessData = false;
                        response.Message = "El archivo cargado tiene errores";
                        response.Success = false;
                        await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores");
                    }

                    var entityMap = mapper.Map<QueueStatusPrecipitation>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataPrecipitacion(entityMap);

                }
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

        private DateOnly ParseDate(string dateString)
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

        private static void RegisterError(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Error.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    csv.WriteField(row[0]);
                    csv.WriteField(row[1]);
                    csv.NextRecord();
                }
            }
        }

        private static void RegisterCorrect(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

    }
}
