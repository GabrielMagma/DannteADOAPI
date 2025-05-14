using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Xml.Linq;

namespace ADO.BL.Services
{
    public class FilePodasValidationServices : IFilePodasValidationServices
    {
        //private readonly IPodasEssaDataAccess podasEssaDataAccess;        
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;        
        private readonly string _PodasDirectoryPath;
        private readonly string _connectionString;
        private readonly string[] _timeFormats;
        private readonly IMapper mapper;

        public FilePodasValidationServices(IConfiguration configuration,
            //IPodasEssaDataAccess _podasEssaDataAccess,            
            IStatusFileDataAccess _statuFileDataAccess,
            IHubContext<NotificationHub> hubContext,
            IMapper _mapper)
        {
            //podasEssaDataAccess = _podasEssaDataAccess;
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _PodasDirectoryPath = configuration["PodasPath"];
            statusFileDataAccess = _statuFileDataAccess;
            _hubContext = hubContext;
            mapper = _mapper;
        }        

        public async Task<ResponseQuery<bool>> ReadFilePodas(PodasValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _PodasDirectoryPath;
                var errorFlag = false;                

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx").OrderBy(f => f).ToArray())                
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

                    
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();

                    // columnas tabla error
                    dataTableError.Columns.Add("C1");

                    var statusFilesingle = new StatusFileDTO();

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "PODAS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 17; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }


                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        
                        var sheetsCount = package.Workbook.Worksheets.Count;

                        for (int j = 0; j < sheetsCount; j++)
                        {
                            var worksheet = package.Workbook.Worksheets[j];                            
                            
                            var listDTOPodas = new List<PodaDTO>();                            
                            
                            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                            {

                                var beacon = 0;
                                for (int i = 1; i <= 17; i++)
                                {
                                    if (worksheet.Cells[row, i].Text == "")
                                    {
                                        beacon++;
                                    }
                                }
                                if (beacon == 17)
                                {
                                    break;
                                }

                                if (string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 8].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 12].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 13].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 14].Text))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row} y hoja {worksheet}, está incompleta";
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var date = string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? "31/12/2099"  : worksheet.Cells[row, 5].Text.ToString();

                                var date2 = ParseDate(date);
                                if (date2 == ParseDate("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] =  $"Error en el dato de la fecha ejecución en la línea {row} y hoja {worksheet}";
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                //var newEntity = new PodaDTO();

                                //newEntity.NameRegion = worksheet.Cells[row, 1].Text.Trim().ToUpper();
                                //newEntity.NameZone = worksheet.Cells[row, 2].Text.Trim().ToUpper();
                                //newEntity.Circuit = worksheet.Cells[row, 3].Text.Trim().ToUpper();
                                //newEntity.NameLocation = worksheet.Cells[row, 4].Text.Trim().ToUpper();
                                //newEntity.DateExecuted = date2;
                                //newEntity.Scheduled = worksheet.Cells[row, 6].Text.Trim().ToUpper();
                                //newEntity.NoOt = worksheet.Cells[row, 7].Text.Trim().ToUpper();
                                //newEntity.StateOt = worksheet.Cells[row, 8].Text.Trim().ToUpper();
                                //newEntity.DateState = worksheet.Cells[row, 9].Text != null ? ParseDate(worksheet.Cells[row, 9].Text.ToString()) : (DateOnly?)null;
                                //newEntity.Pqr = worksheet.Cells[row, 10].Text.Trim().ToUpper();
                                //newEntity.NoReport = worksheet.Cells[row, 11].Text.Trim().ToUpper();
                                //newEntity.Consig = worksheet.Cells[row, 12].Text.Trim().ToUpper();
                                //newEntity.BeginSup = worksheet.Cells[row, 13].Text.Trim().ToUpper();
                                //newEntity.EndSup = worksheet.Cells[row, 14].Text.Trim().ToUpper();
                                //newEntity.Urban = worksheet.Cells[row, 15].Text.Trim().ToUpper();
                                //newEntity.Item = worksheet.Cells[row, 16].Text.Trim().ToUpper();
                                //newEntity.Description = worksheet.Cells[row, 17].Text.Trim().ToUpper();


                                //listDTOPodas.Add(newEntity);

                                var newRow = dataTable.NewRow();
                                newRow[0] = worksheet.Cells[row, 1].Text.Trim().ToUpper();
                                newRow[1] = worksheet.Cells[row, 2].Text.Trim().ToUpper();
                                newRow[2] = worksheet.Cells[row, 3].Text.Trim().ToUpper();
                                newRow[3] = worksheet.Cells[row, 4].Text.Trim().ToUpper();
                                newRow[4] = date2.ToString();
                                newRow[5] = worksheet.Cells[row, 6].Text.Trim().ToUpper();
                                newRow[6] = worksheet.Cells[row, 7].Text.Trim().ToUpper();
                                newRow[7] = worksheet.Cells[row, 8].Text.Trim().ToUpper();
                                newRow[8] = worksheet.Cells[row, 9].Text.Trim().ToUpper();
                                newRow[9] = worksheet.Cells[row, 10].Text.Trim().ToUpper();
                                newRow[10] = worksheet.Cells[row, 11].Text.Trim().ToUpper();
                                newRow[11] = worksheet.Cells[row, 12].Text.Trim().ToUpper();
                                newRow[12] = worksheet.Cells[row, 13].Text.Trim().ToUpper();
                                newRow[13] = worksheet.Cells[row, 14].Text.Trim().ToUpper();
                                newRow[14] = worksheet.Cells[row, 15].Text.Trim().ToUpper();
                                newRow[15] = worksheet.Cells[row, 16].Text.Trim().ToUpper();
                                newRow[16] = worksheet.Cells[row, 17].Text.Trim().ToUpper().Replace(",",";");

                                dataTable.Rows.Add(newRow);



                            }                                                                                   

                            //if (listDTOPodas.Count > 0)
                            //{
                            //    int i = 0;
                            //    while ((i * 10000) < listDTOPodas.Count())
                            //    {
                            //        var subgroup = listDTOPodas.Skip(i * 10000).Take(10000).ToList();
                            //        var EntityResult = mapper.Map<List<IaPoda>>(subgroup);
                            //        SaveData(EntityResult);
                            //        i++;
                            //        Console.WriteLine(i * 10000);
                            //    }

                            //}
                        }

                        statusFilesingle.Status = 1;

                        if (dataTableError.Rows.Count > 0)
                        {
                            await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores");
                            statusFilesingle.Status = 2;
                            errorFlag = true;
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        if (dataTable.Rows.Count > 0)
                        {
                            RegisterData(dataTable, inputFolder, filePath);
                        }

                        var entityMap = mapper.Map<QueueStatusPoda>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataPodas(entityMap);
                    }
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
                    response.Message = "Archivos validados correctamente";
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
                    csv.WriteField(row[0]);
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
                    for (var i = 0; i < 17; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
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

        // acciones en bd y mappeo

        //public Boolean SaveData(List<IaPoda> request)
        //{

        //        var result = podasEssaDataAccess.SaveData(request);

        //        return result;            

        //}        

    }
}
