using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class FileRayosValidationServices : IFileRayosValidationServices
    {   
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IRayosCSVDataAccess rayosCSVDataAccess;
        private readonly string _RayosDirectoryPath;
        private readonly string _connectionString;
        private readonly string[] _timeFormats;
        private readonly IMapper mapper;
        public FileRayosValidationServices(IConfiguration configuration, 
            IRayosCSVDataAccess _rayosCSVDataAccess,            
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _RayosDirectoryPath = configuration["RayosPath"];
            statusFileDataAccess = _statuFileDataAccess;
            rayosCSVDataAccess = _rayosCSVDataAccess;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesRayos(RayosValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _RayosDirectoryPath;
                var errorFlag = false;

                int fecha = 2;
                int region = 11;                
                int circuito = 10;
                int latitud = 3;
                int longitud = 4;                
                int corriente = 5;
                int error = 6;
                int municipio = 7;

                if (request.Encabezado == false)
                {
                    fecha = request.columns.Fecha - 1;
                    region = request.columns.Region - 1;                    
                    circuito = request.columns.Circuito - 1;
                    latitud = request.columns.Latitud - 1;
                    longitud = request.columns.Longitud - 1;                    
                    corriente = request.columns.Corriente - 1;
                    error = request.columns.Error - 1;
                    municipio = request.columns.Municipio - 1;
                }

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv")
                                        .Where(file => !file.EndsWith("_Correct.csv")
                                         && !file.EndsWith("_Error.csv"))
                                        .ToList().OrderBy(f => f)
                                        .ToArray())
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

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} está validando la estructura del formato");

                    var statusFileList = new List<StatusFileDTO>();

                    string[] fileLines = File.ReadAllLines(filePath);
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    var count = 2;

                    var statusFilesingle = new StatusFileDTO();                    

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "RAYOS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    statusFileList.Add(statusFilesingle);

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 7; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }
                    var listDTOMpLightning = new List<MpLightningDTO>();

                    // columnas tabla datos correctos
                    foreach (var item in fileLines.Skip(1))
                    {
                        var valueLines = item.Split(',',';');
                        string message = string.Empty;
                        var dateTemp = valueLines[fecha] != "" ? valueLines[fecha] : string.Empty;
                        var date = dateTemp.Split(',', '.')[0];
                        if (valueLines.Length != 13)
                        {                            
                            response.Message = "Error de cantidad de columnas llenas, formato incorrecto";
                            response.SuccessData = false;
                            response.Success = false;
                            return response;
                        }

                        if (string.IsNullOrEmpty(valueLines[fecha]) || string.IsNullOrEmpty(valueLines[region]) ||
                                string.IsNullOrEmpty(valueLines[circuito]) || string.IsNullOrEmpty(valueLines[latitud]) || 
                                string.IsNullOrEmpty(valueLines[longitud]) || string.IsNullOrEmpty(valueLines[municipio]) || 
                                string.IsNullOrEmpty(valueLines[error]) || string.IsNullOrEmpty(valueLines[corriente]))                            
                        {
                            message = $"Error de la data en la línea {count}, las columnas Fecha, Región, Circuito, Latitud, Longitud, Corriente, Error y Municipio son Requeridas";
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"{item}";
                            newRowError[1] = message;
                            dataTableError.Rows.Add(newRowError);
                            count++;
                            continue;
                        }

                        if (date == "")
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"{item}";
                            newRowError[1] = $"Error en la fila {count} y columna Fecha, la fecha no puede ser nula";
                            dataTableError.Rows.Add(newRowError);
                            count++;                            
                            continue;
                        }

                        var dateDef = ParseDate(date);
                        if (dateDef == ParseDate("31/12/2099"))
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"{item}";
                            newRowError[1] = date + $" En la data de la fila {count}";
                            dataTableError.Rows.Add(newRowError);
                            count++;
                            continue;
                        }
                            
                        var newRow = dataTable.NewRow();
                        newRow[0] = dateDef.ToString();

                        newRow[1] = valueLines[region];                        
                        newRow[2] = valueLines[circuito].Replace(" ", "");
                        newRow[3] = valueLines[latitud];
                        newRow[4] = valueLines[longitud];
                        newRow[5] = valueLines[corriente];
                        newRow[6] = valueLines[error];

                        dataTable.Rows.Add(newRow);

                        count++;

                    }                    
                    
                    statusFilesingle.Status = 1;

                    if (dataTableError.Rows.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores.");
                        statusFilesingle.Status = 2;
                        errorFlag = true;
                        RegisterError(dataTableError, inputFolder, filePath);
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        RegisterData(dataTable, inputFolder, filePath);
                    }

                    var subgroupMap = mapper.Map<QueueStatusLightning>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataRayos(subgroupMap);

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Proceso de validación completado para todos los archivos");
                }

                if (errorFlag)
                {                    
                    response.Message = "Archivos con errores";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {                    
                    response.Message = "Todos los archivos validados";
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

        private static void RegisterError(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Error.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private static void RegisterData(DataTable dataTable, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataRow row in dataTable.Rows)
                {
                    for (var i = 0; i < 7; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format, _spanishCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateTime.ParseExact("31/12/2099 00:00:00", "dd/MM/yyyy HH:mm:ss", _spanishCulture);
        }

    }
}
