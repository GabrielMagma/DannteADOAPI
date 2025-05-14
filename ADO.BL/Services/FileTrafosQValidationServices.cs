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
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace ADO.BL.Services
{
    public class FileTrafosQValidationServices : IFileTrafosQValidationServices
    {
        //private readonly IPodasEssaDataAccess podasEssaDataAccess;        
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;        
        private readonly string _PodasDirectoryPath;
        private readonly string _connectionString;
        private readonly string[] _timeFormats;
        private readonly IMapper mapper;

        public FileTrafosQValidationServices(IConfiguration configuration,
            //IPodasEssaDataAccess _podasEssaDataAccess,            
            IStatusFileDataAccess _statuFileDataAccess,
            IHubContext<NotificationHub> hubContext,
            IMapper _mapper)
        {
            //podasEssaDataAccess = _podasEssaDataAccess;
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _PodasDirectoryPath = configuration["TrafosPath"];
            statusFileDataAccess = _statuFileDataAccess;
            _hubContext = hubContext;
            mapper = _mapper;
        }        

        public async Task<ResponseQuery<bool>> ReadFileTrafos(TrafosValidationDTO request, ResponseQuery<bool> response)
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
                    statusFilesingle.FileType = "TRANSFORMADORES QUEMADOS";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;

                    

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 10; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }


                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        
                        var sheetsCount = package.Workbook.Worksheets.Count;

                        var worksheet = package.Workbook.Worksheets[0];
                        var listDataString = new StringBuilder();
                        var assetList = new List<AllAssetDTO>();

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            if (worksheet.Cells[row, 2].Text != "")
                            {
                                var codeSigDoc = worksheet.Cells[row, 2].Text.Trim();
                                if (codeSigDoc == "0")
                                {
                                    continue;
                                }
                                listDataString.Append($"'{codeSigDoc.Trim().Replace(" ", "")}',");
                                if (codeSigDoc[0] == '0')
                                {
                                    listDataString.Append($"'{codeSigDoc.Trim().Replace(" ", "").Remove(0, 1)}',");
                                }
                                else
                                {
                                    listDataString.Append($"'0{codeSigDoc.Trim().Replace(" ", "")}',");
                                }

                            }

                        }

                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();

                            #region tempAnterior
                            var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                            var SelectQuery = $@"SELECT code_sig, latitude, longitude
                                                 FROM public.all_asset where code_sig in ({listDef})";
                            using (var reader = new NpgsqlCommand(SelectQuery, connection))
                            {
                                try
                                {

                                    using (var result = await reader.ExecuteReaderAsync())
                                    {
                                        while (await result.ReadAsync())
                                        {
                                            var temp = new AllAssetDTO();

                                            temp.CodeSig = result[0].ToString();
                                            temp.Latitude = float.Parse(result[1].ToString());
                                            temp.Longitude = float.Parse(result[2].ToString());

                                            assetList.Add(temp);
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
                            #endregion

                        }

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 86; i++)
                            {
                                if (worksheet.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 86)
                            {
                                break;
                            }

                            if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ||
                                string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ||
                                string.IsNullOrEmpty(worksheet.Cells[row, 5].Text))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data de la línea {row}, está incompleta";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var date = string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? "31/12/2099"  : worksheet.Cells[row, 3].Text.ToString();

                            var date2 = ParseDate(date);
                            if (date2 == ParseDate("31/12/2099"))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] =  $"Error en el dato de la fecha ejecución en la línea {row}";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var dateRet = string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? "31/12/2099" : worksheet.Cells[row, 4].Text.ToString();

                            var date2Ret = ParseDate(dateRet);
                            if (date2Ret == ParseDate("31/12/2099"))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en el dato de la fecha ejecución en la línea {row}";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var date3 = string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? "31/12/2099" : worksheet.Cells[row, 5].Text.ToString();

                            var date3Cam = ParseDate(date3);
                            if (date3Cam == ParseDate("31/12/2099"))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en el dato de la fecha ejecución en la línea {row}";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var codeSigTemp = worksheet.Cells[row, 2].Text.Trim().ToUpper();
                            var codeSigTemp2 = codeSigTemp;
                            if (codeSigTemp[0] == '0')
                            {
                                codeSigTemp2 = $"{codeSigTemp.Trim().Replace(" ", "").Remove(0, 1)}";
                            }
                            else
                            {
                                codeSigTemp2 = $"0{codeSigTemp.Trim().Replace(" ", "")}";
                            }
                            var assetTemp = assetList.FirstOrDefault(x => x.CodeSig == codeSigTemp);
                            if (assetTemp == null)
                            {
                                assetTemp = assetList.FirstOrDefault(x => x.CodeSig == codeSigTemp2);
                            }
                            if (assetTemp == null)
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en el dato del código de ubicación en la línea {row}, no existe este registro en la base de datos";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var newRow = dataTable.NewRow();
                            newRow[0] = worksheet.Cells[row, 2].Text.Trim().ToUpper();
                            newRow[1] = date2.Year;//
                            newRow[2] = date2.Month;//
                            newRow[3] = "1";//
                            newRow[4] = worksheet.Cells[row, 1].Text.Trim().ToUpper().Replace(" ", "");
                            newRow[5] = date2.ToString();
                            newRow[6] = date2Ret.ToString();
                            newRow[7] = date3Cam.ToString();
                            newRow[8] = assetTemp.Latitude;
                            newRow[9] = assetTemp.Longitude;

                            dataTable.Rows.Add(newRow);

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

                        var entityMap = mapper.Map<QueueStatusTransformerBurned>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataTrafosQuemados(entityMap);
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
                    for (var i = 0; i < 10; i++)
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
