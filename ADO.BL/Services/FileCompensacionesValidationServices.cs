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
using System.Text.RegularExpressions;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ADO.BL.Services
{
    public class FileCompensacionesValidationServices : IFileCompensacionesValidationServices
    {
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _connectionString;
        private readonly string _CompsDirectoryPath;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStatusFileDataAccess statusFileDataAccess;        
        private static readonly CultureInfo _spanishCultureOnly = new CultureInfo("es-CO"); // o "es-ES"

        public FileCompensacionesValidationServices(IConfiguration configuration,
            IMapper _mapper,
            IStatusFileDataAccess _statuFileDataAccess,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _CompsDirectoryPath = configuration["CompensationsPath"];
            statusFileDataAccess = _statuFileDataAccess;
            _hubContext = hubContext;
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ReadFilesComp(CompsValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                string inputFolder = _CompsDirectoryPath;
                var errorFlag = false;
                var statusFileList = new List<StatusFileDTO>();

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx").OrderBy(f => f).ToArray())
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

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var count = 0;

                        var worksheet1 = package.Workbook.Worksheets[0];                        

                        var dataTableError = new DataTable();
                        var dataTable = new DataTable();
                        var dataTableComplete = new DataTable();

                        var statusFilesingle = new StatusFileDTO();

                        // Obtener los primeros 4 dígitos como el año
                        int year = int.Parse(fileName.Substring(0, 4));

                        // Obtener los siguientes 2 dígitos como el mes
                        int month = int.Parse(fileName.Substring(4, 2));

                        statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                        statusFilesingle.UserId = request.UserId;
                        statusFilesingle.FileName = fileName;
                        statusFilesingle.FileType = "COMPENSACIONES";
                        statusFilesingle.Year = year;
                        statusFilesingle.Month = month;
                        statusFilesingle.Day = 1;
                        statusFilesingle.DateRegister = ParseDate($"1/{month}/{year}");                   

                        // columnas tablas para csv
                        dataTableError.Columns.Add("C1");

                        for (int i = 1; i <= 12; i++)
                        {
                            dataTable.Columns.Add($"C{i}");
                        }

                        var listDataString = new StringBuilder();
                        var listCodeSig = new List<string>();
                        var assetList = new List<AllAssetDTO>();

                        for (int row = 2; row <= worksheet1.Dimension.End.Row; row++)
                        {
                            if (worksheet1.Cells[row, 14].Text != "")
                            {
                                if (listCodeSig.Contains(worksheet1.Cells[row, 14].Text.Trim().Replace(" ", ""))) 
                                { 
                                    continue;
                                }
                                listDataString.Append($"'{worksheet1.Cells[row, 14].Text.Trim().Replace(" ", "")}',");
                                listCodeSig.Add(worksheet1.Cells[row, 14].Text.Trim().Replace(" ", ""));
                            }

                        }

                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();

                            #region tempAnterior
                            var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                            var SelectQuery = $@"SELECT code_sig, latitude, longitude FROM public.all_asset where code_sig in ({listDef})";
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

                        for (int row = 2; row <= worksheet1.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 17; i++)
                            {
                                if (worksheet1.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 24)
                            {
                                break;
                            }

                            
                            if (string.IsNullOrEmpty(worksheet1.Cells[row, 2].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 5].Text) ||
                                string.IsNullOrEmpty(worksheet1.Cells[row, 9].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 10].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 11].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 12].Text) ||
                                string.IsNullOrEmpty(worksheet1.Cells[row, 14].Text) || string.IsNullOrEmpty(worksheet1.Cells[row, 15].Text) || 
                                string.IsNullOrEmpty(worksheet1.Cells[row, 16].Text))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data en la línea {row}, hay uno o más campos vacíos y estos son Requeridos";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var date = worksheet1.Cells[row, 5].Text != "" ? worksheet1.Cells[row, 5].Text : string.Empty;

                            if (date == "")
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} y columna Fecha, la fecha no puede ser nula";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var date2 = ParseDate(date);
                            if (date2 == ParseDate("31/12/2099"))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fecha en la línea {row}";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var assetExist = assetList.FirstOrDefault(x => x.CodeSig == worksheet1.Cells[row, 14].Text);

                            if (assetExist == null)
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la data de la línea {row}, no existe este circuito en assets";
                                dataTableError.Rows.Add(newRowError);
                                continue;
                            }

                            var newRow = dataTable.NewRow();
                                
                            newRow[0] = date2.Year;
                            newRow[1] = date2.Month;
                            newRow[2] = worksheet1.Cells[row, 15].Text.ToUpper();
                            newRow[3] = worksheet1.Cells[row, 14].Text.ToUpper();
                            newRow[4] = worksheet1.Cells[row, 12].Text.ToUpper();
                            newRow[5] = worksheet1.Cells[row, 16].Text.ToUpper();
                            newRow[6] = worksheet1.Cells[row, 2].Text.ToUpper();
                            newRow[7] = worksheet1.Cells[row, 11].Text.ToUpper();
                            newRow[8] = worksheet1.Cells[row, 10].Text.ToUpper();
                            newRow[9] = worksheet1.Cells[row, 9].Text.ToUpper();
                            newRow[10] = assetExist.Longitude.ToString();
                            newRow[11] = assetExist.Latitude.ToString();
                            dataTable.Rows.Add(newRow);

                        }

                        statusFilesingle.Status = 1;
                        if (dataTableError.Rows.Count > 0)
                        {
                            await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores");
                            errorFlag = true;
                            statusFilesingle.Status = 2;
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        var subgroupMap = mapper.Map<QueueStatusCompensation>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataCompensations(subgroupMap);

                        if (dataTable.Rows.Count > 0)
                        {                            
                            RegisterTable(dataTable, inputFolder, filePath);
                        }

                    }

                }

                if (errorFlag)
                {
                    response.Message = "Archivo con errores";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    response.Message = "Archivos validados sin problemas";
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

        private static void RegisterTable(DataTable table, string inputFolder, string filePath)
        {
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < 12; i++)
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
                if (DateOnly.TryParseExact(dateString, format, _spanishCultureOnly, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate; // o .ToUniversalTime() si tu columna es timestamptz
                }
            }
            return DateOnly.ParseExact("31/12/2099", "dd/MM/yyyy", _spanishCultureOnly);
        }        
    }
}
