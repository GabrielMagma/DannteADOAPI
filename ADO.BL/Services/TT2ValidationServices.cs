using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class TT2ValidationServices : ITT2ValidationServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _TT2DirectoryPath;
        private readonly IMapper mapper;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        public TT2ValidationServices(IConfiguration configuration,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _TT2DirectoryPath = configuration["TT2DirectoryPath"];
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseEntity<List<StatusFileDTO>>> ValidationTT2(TT2ValidationDTO request, ResponseEntity<List<StatusFileDTO>> response)
        {
            try
            {
                var columns = int.Parse(_configuration["Validations:TT2Columns"]);
                string inputFolder = _TT2DirectoryPath;
                var errorFlag = false;
                int uia = 0;
                int grupoCalidad = 2;
                int latitud = 8;
                int longitud = 7;
                int fecha = 11;
                if (request.Encabezado == false)
                {
                    uia = request.columns.UIA - 1;
                    grupoCalidad = request.columns.GrupoCalidad - 1;
                    latitud = request.columns.Latitud - 1;
                    longitud = request.columns.Longitud - 1;
                    fecha = request.columns.Fecha - 1;
                }
                var statusFileList = new List<StatusFileDTO>();
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv")
                    .Where(file => !file.EndsWith("_Correct.csv") 
                    && !file.EndsWith("_Error.csv")
                    && !file.EndsWith("_insert.csv") 
                    && !file.EndsWith("_check.csv") 
                    && !file.EndsWith("_update.csv"))
                    .ToList().OrderBy(f => f)
                     .ToArray()
                    )
                {

                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    int count = 1;                                                            
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "TT2";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;
                    statusFilesingle.DateRegister = DateOnly.Parse($"1-{month}-{year}");

                    // columnas tabla datos correctos
                    for (int i = 1; i <= columns; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }

                    string[] fileLines = File.ReadAllLines(filePath);

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
                        if (valueLines[0] != "COD_CREG")
                        {
                            string message = string.Empty;
                            var beacon = 0;
                            for (int i = 0; i < columns; i++)
                            {
                                if (valueLines[i] != "")
                                {
                                    beacon++;
                                }
                            }

                            if (beacon > 0)
                            {

                                if (valueLines.Length != columns)
                                {
                                    message = "Error de cantidad de columnas llenas";
                                    RegisterError(dataTableError, valueLines, count, message);
                                }

                                else if (valueLines[uia] == "")
                                {
                                    message = "Error de la data de UIA, no está llena correctamente, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                }

                                else if (valueLines[grupoCalidad].Length != 2)
                                {
                                    message = "Error de la data de grupo calidad, debe ser sólamente de dos dígitos, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                }

                                else if (valueLines[latitud] == "" || valueLines[longitud] == "")
                                {
                                    message = "Error de la data de Latitud y/o Longitud, no está llena correctamente, por favor corregirla";
                                    RegisterError(dataTableError, valueLines, count, message);
                                }

                                else if (valueLines[fecha] != "")
                                {
                                    var datefile = ParseDate(valueLines[fecha]);
                                    var dateToday = DateTime.Now;
                                    if (datefile.Contains("Error"))
                                    {
                                        message = "Error de la fecha en la data, no tiene el formato correcto";
                                        RegisterError(dataTableError, valueLines, count, message);
                                    }
                                    else if (DateTime.Parse(datefile) > dateToday)
                                    {
                                        message = "Error de la fecha en la data, no puede ser mayor a la fecha actual";
                                        RegisterError(dataTableError, valueLines, count, message);
                                    }
                                    else
                                    {
                                        InsertData(dataTable, valueLines, columns);
                                    }
                                }

                                //else
                                //{
                                //    InsertData(dataTable, valueLines, columns);
                                //}
                            }
                            beacon = 0;
                        }
                        count++;
                            
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        createCSV(dataTable, filePath, columns);
                    }

                    statusFilesingle.Status = 1;

                    if (dataTableError.Rows.Count > 0)
                    {
                        statusFilesingle.Status = 2;
                        errorFlag = true;
                        createCSVError(dataTableError, filePath);
                    }

                    var entityMap = mapper.Map<QueueStatusTt2>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataTT2(entityMap);

                    statusFileList.Add(statusFilesingle);

                }
                if (errorFlag)
                {
                    response.Message = "file with errors";
                    response.SuccessData = false;
                    response.Success = false;
                    response.Data = statusFileList;
                    return response;
                }
                else
                {
                    response.Message = "All files are created";
                    response.SuccessData = true;
                    response.Success = true;
                    response.Data = statusFileList;
                    return response;
                }

            }
            catch (FormatException ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                response.Data = new List<StatusFileDTO>();
            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                response.Data = new List<StatusFileDTO>();
            }

            return response;

        }

        private static void InsertData(DataTable dataTable, string[] valueLines, int columns)
        {
            var newRow = dataTable.NewRow();

            for (int i = 0; i <= columns - 1; i++)
            {
                newRow[i] = valueLines[i].ToUpper().Trim();
            }

            dataTable.Rows.Add(newRow);

        }

        private static void RegisterError(DataTable table, string[] item, int count, string message)
        {
            var messageError = $"{message} en la línea {count} del archivo cargado";

            var newRow = table.NewRow();

            newRow[0] = item;
            newRow[1] = messageError;

            table.Rows.Add(newRow);

        }

        private void createCSV(DataTable table, string filePath, int columns)
        {
            string inputFolder = _TT2DirectoryPath;
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < columns; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private void createCSVError(DataTable table, string filePath)
        {
            string inputFolder = _TT2DirectoryPath;
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

        private string ParseDate(string dateString)
        {            
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString();
                }
            }
            return $"Error en el formato de fecha {dateString} no es válido.";            
        }

        
    }
}
