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
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FilePolesValidationServices : IFilePolesValidationServices
    {
        private readonly IMapper mapper;
        private readonly string _connectionString;
        private readonly string _PolesDirectoryPath;
        private readonly IConfiguration _configuration;
        private readonly IPolesDataAccess polesEepDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        public FilePolesValidationServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext,
            IPolesDataAccess _polesEepDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _PolesDirectoryPath = configuration["PolesPath"];
            statusFileDataAccess = _statuFileDataAccess;
            polesEepDataAccess = _polesEepDataAccess;
            _configuration = configuration;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesPoles(PolesValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _PolesDirectoryPath;
                var errorFlag = false;

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} está validando la estructura del formato");
                    var listUtilityPoleDTO = new List<MpUtilityPoleDTO>();
                    
                    var statusFileList = new List<StatusFileDTO>();
                    var statusFilesingle = new StatusFileDTO();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "POLES";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    string[] fileLines = File.ReadAllLines(filePath);
                    var listDataString = new StringBuilder();
                    var listFparent = new List<string>();
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();

                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    var count = 2;

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 6; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }

                    foreach (var item in fileLines)
                    {
                        var valueLinesTemp = item.Split(',',';');
                        if (valueLinesTemp[0] != "NODO_FISICO")
                        {
                            if (!listFparent.Contains(valueLinesTemp[1]))
                            {
                                listDataString.Append($"'{valueLinesTemp[1]}',");
                                listFparent.Add(valueLinesTemp[1]);
                            }
                        }
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);                                                

                        var SelectQueryUtility = $@"SELECT painting_code, fparent from maps.mp_utility_pole where fparent in ({listDef})";
                        using (var reader = new NpgsqlCommand(SelectQueryUtility, connection))
                        {
                            try
                            {
                                using (var result = reader.ExecuteReader())
                                {
                                    while (result.Read())
                                    {
                                        var temp = new MpUtilityPoleDTO();                                        
                                        temp.PaintingCode = result[0].ToString();
                                        temp.Fparent = result[1].ToString();                                        
                                        listUtilityPoleDTO.Add(temp);
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

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
                        if (string.IsNullOrEmpty(valueLines[0]) || string.IsNullOrEmpty(valueLines[1]) || 
                            string.IsNullOrEmpty(valueLines[2]) || string.IsNullOrEmpty(valueLines[3]) ||
                            string.IsNullOrEmpty(valueLines[4]))
                        {
                            var message = $"Error de la data en la línea {count}, todas las columnas son Requeridas";
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"{item}";
                            newRowError[1] = message;
                            dataTableError.Rows.Add(newRowError);
                            count++;
                            continue;
                        }

                        if (valueLines[0] == "NODO_FISICO")
                        {
                            continue;
                        }

                        var poleTemp = listUtilityPoleDTO.FirstOrDefault(x => x.PaintingCode == valueLines[0]);

                        if (poleTemp == null)
                        {                            
                            
                            var newRow = dataTable.NewRow();
                            newRow[0] = valueLines[0].Trim();
                            newRow[1] = valueLines[1].Trim().Replace(" ","");
                            newRow[2] = valueLines[2].ToString();
                            newRow[3] = valueLines[3].ToString();
                            newRow[4] = valueLines[4].Trim();
                            newRow[5] = valueLines[5].ToString();

                            dataTable.Rows.Add(newRow);

                            count++;
                        }

                        else if(poleTemp.Fparent != valueLines[1].Trim())
                        {
                            var newRow = dataTable.NewRow();
                            newRow[0] = valueLines[0].Trim();
                            newRow[1] = valueLines[1].Trim().Replace(" ", "");
                            newRow[2] = valueLines[2].ToString();
                            newRow[3] = valueLines[3].ToString();
                            newRow[4] = valueLines[4].Trim();
                            newRow[5] = valueLines[5].ToString();

                            dataTable.Rows.Add(newRow);

                            count++;
                        }
                        else
                        {
                            count++;
                        }
                        
                    }

                    statusFilesingle.Status = 1;

                    statusFileList.Add(statusFilesingle);

                    if (dataTable.Rows.Count > 0)
                    {
                        RegisterData(dataTable, inputFolder, filePath);
                        //var polesMapped = mapper.Map<List<MpUtilityPole>>(listEntityPoleDTO);
                        //var respCreate = CreateData(polesMapped);
                    }

                    if (dataTableError.Rows.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores.");
                        statusFilesingle.Status = 2;
                        errorFlag = true;
                        RegisterError(dataTableError, inputFolder, filePath);
                    }

                    var entityMap = mapper.Map<QueueStatusPole>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataPole(entityMap);
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
                return response;
            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                return response;
            }

            
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
                    for (var i = 0; i < 6; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        // acciones en bd y mappeo

        public async Task<Boolean> CreateData(List<MpUtilityPole> request)
        {            
            await polesEepDataAccess.CreateFile(request);
            return true;

        }

    }
}
