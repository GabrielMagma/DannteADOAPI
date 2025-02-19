using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class RayosCSVServices : IRayosCSVServices
    {
        private readonly IRayosCSVDataAccess rayosCSVDataAccess;        
        private readonly string _RayosDirectoryPath;
        private readonly IStatusFileEepDataAccess statusFileDataAccess;
        private readonly IMapper mapper;

        public RayosCSVServices(IConfiguration configuration, 
            IRayosCSVDataAccess _rayosCSVDataAccess,
            IStatusFileEepDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            rayosCSVDataAccess = _rayosCSVDataAccess;
            mapper = _mapper;
            _RayosDirectoryPath = configuration["RayosPath"];
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseEntity<List<string>>> SearchDataCSV(RayosValidationDTO request, ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = _RayosDirectoryPath;
                var errorFlag = false;

                int fecha = 0;
                int region = 1;
                int zona = 2;
                int circuito = 3;
                int latitud = 4;
                int longitud = 5;
                int tipo = 6;
                int corriente = 7;
                int error = 8;
                int municipio = 9;

                var statusFileList = new List<StatusFileDTO>();
                if (request.Encabezado == false)
                {
                    fecha = request.columns.Fecha - 1;
                    region = request.columns.Region - 1;
                    zona = request.columns.Zona - 1;
                    circuito = request.columns.Circuito - 1;
                    latitud = request.columns.Latitud - 1;
                    longitud = request.columns.Longitud - 1;
                    tipo = request.columns.Tipo - 1;
                    corriente = request.columns.Corriente - 1;
                    error = request.columns.Error - 1;
                    municipio = request.columns.Municipio - 1;
                }

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    string[] fileLines = File.ReadAllLines(filePath);
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    //int count = 1;
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");
                    var count = 1;

                    var statusFilesingle = new StatusFileDTO();
                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "RAYOS";
                    statusFilesingle.Year = request.Year;
                    statusFilesingle.Month = request.Month;                    

                    // columnas tabla datos correctos
                    for (int i = 1; i <= 12; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }
                    var listDTOMpLightning = new List<MpLightningDTO>();

                    // columnas tabla datos correctos
                    foreach (var item in fileLines)
                    {

                        var valueLines = item.Split(";");
                        string message = string.Empty;
                        var dateTemp = valueLines[fecha] != "" ? valueLines[fecha] : string.Empty;
                        var date = dateTemp.Split(',', '.')[0];
                        if (valueLines.Length != 12)
                        {
                            message = $"Error de cantidad de columnas llenas en la línea {count}, formato incorrecto";
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"{item}";
                            newRowError[1] = message;
                            dataTableError.Rows.Add(newRowError);
                        }
                        else if (valueLines[fecha] == "" || valueLines[region] == "" || valueLines[zona] == "" ||
                                valueLines[circuito] == "" || valueLines[latitud] == "" || valueLines[longitud] == "" ||
                                valueLines[municipio] == "")
                        {
                            message = $"Error de la data en la línea {count}, las columnas Fecha, Región, Zona, Circuito, Latitud, Longitud y Municipio son Requeridas";
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"{item}";
                            newRowError[1] = message;
                            dataTableError.Rows.Add(newRowError);
                        }
                        else if (date != "")
                        {
                            date = ParseDate(date);
                            if (date.Contains("Error"))
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = date + $" En la data {item}";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else
                            {
                                var newRow = dataTable.NewRow();
                                newRow[0] = date;

                                newRow[1] = valueLines[region];
                                newRow[2] = valueLines[zona];
                                newRow[3] = valueLines[circuito].Replace(" ", "");
                                newRow[4] = valueLines[latitud];
                                newRow[5] = valueLines[longitud];
                                newRow[6] = valueLines[tipo];
                                newRow[7] = valueLines[corriente];
                                newRow[8] = valueLines[error];
                                newRow[9] = valueLines[municipio];

                                dataTable.Rows.Add(newRow);

                                var newEntity = new MpLightningDTO();
                                var aniomes = date.Split('/', ' ');
                                newEntity.NameRegion = valueLines[region].Trim().ToUpper();
                                newEntity.NameZone = valueLines[zona].Trim().ToUpper();
                                newEntity.NameLocality = valueLines[municipio].Trim().ToUpper();
                                newEntity.Fparent = valueLines[circuito].Trim().Replace(" ", "");
                                newEntity.DateEvent = DateTime.Parse(date);
                                newEntity.Latitude = float.Parse(valueLines[latitud].Replace(',', '.').Trim());
                                newEntity.Longitude = float.Parse(valueLines[longitud].Replace(',', '.').Trim());
                                newEntity.Amperage = float.Parse(valueLines[corriente].Replace(',', '.').Trim());
                                newEntity.Error = float.Parse(valueLines[error].Replace(',', '.').Trim());
                                newEntity.Type = int.Parse(valueLines[tipo].Trim());
                                newEntity.Year = int.Parse(aniomes[2]);
                                newEntity.Month = int.Parse(aniomes[1]);

                                listDTOMpLightning.Add(newEntity);

                            }

                        }
                        else
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = $"Error en la fila {item} y columna Fecha, la fecha no puede ser nula";
                            dataTableError.Rows.Add(newRowError);
                        }

                    }

                    // Guardar como CSV
                    string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
                    using (var writer = new StreamWriter(outputFilePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {

                        foreach (DataRow row in dataTable.Rows)
                        {
                            for (var i = 0; i < 12; i++)
                            {
                                csv.WriteField(row[i]);
                            }
                            csv.NextRecord();
                        }
                    }
                    statusFilesingle.Status = 1;

                    if (dataTableError.Rows.Count > 0)
                    {
                        statusFilesingle.Status = 0;
                        errorFlag = true;
                        RegisterError(dataTableError, inputFolder, filePath);
                    }

                    if (listDTOMpLightning.Count > 0 && errorFlag == false)
                    {
                        int i = 0;
                        while ((i * 1000) < listDTOMpLightning.Count())
                        {
                            var subgroup = listDTOMpLightning.Skip(i * 1000).Take(1000).ToList();
                            var EntityResult = mapper.Map<List<MpLightning>>(subgroup);
                            SaveData(EntityResult);
                            i++;
                            Console.WriteLine(i * 1000);
                        }

                    }

                    statusFileList.Add(statusFilesingle);

                    Console.WriteLine("Proceso completado.");
                }                

                if (errorFlag)
                {                    
                    response.Message = "file with errors";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    var subgroupMap = mapper.Map<List<StatusFile>>(statusFileList);
                    var resultSave = await statusFileDataAccess.SaveDataList(subgroupMap);
                    response.Message = "All files are created";
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

        public async Task<ResponseEntity<List<string>>> SearchDataExcel(RayosValidationDTO request, ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = _RayosDirectoryPath;
                var errorFlag = false;

                int fecha = 1;
                int region = 2;
                int zona = 3;
                int circuito = 4;
                int latitud = 5;
                int longitud = 6;
                int tipo = 7;
                int corriente = 8;
                int error = 9;
                int municipio = 10;

                if (request.Encabezado == false)
                {
                    fecha = request.columns.Fecha;
                    region = request.columns.Region;
                    zona = request.columns.Zona;
                    circuito = request.columns.Circuito;
                    latitud = request.columns.Latitud;
                    longitud = request.columns.Longitud;
                    tipo = request.columns.Tipo;
                    corriente = request.columns.Corriente;
                    error = request.columns.Error;
                    municipio = request.columns.Municipio;
                }

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))                
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet = package.Workbook.Worksheets[0];
                        var dataTable = new DataTable();
                        var dataTableError = new DataTable();
                        var listDTOMpLightning = new List<MpLightningDTO>();                        

                        // agregar nombres a columnas
                        for (int i = 1; i <= 12; i++)
                        {
                            dataTable.Columns.Add($"C{i}");
                        }

                        // columnas tabla error
                        dataTableError.Columns.Add("C1");                        

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {

                            var beacon = 0;
                            for (int i = 1; i <= 12; i++)
                            {
                                if (worksheet.Cells[row, i].Text == "")
                                {
                                    beacon++;
                                }
                            }
                            if (beacon == 12)
                            {
                                break;
                            }

                            var date = worksheet.Cells[row, fecha].Text != "" ? worksheet.Cells[row, fecha].Text : string.Empty;
                            if (worksheet.Cells[row, fecha].Text == "" || worksheet.Cells[row, region].Text == "" || worksheet.Cells[row, zona].Text == "" ||
                                worksheet.Cells[row, circuito].Text == "" || worksheet.Cells[row, latitud].Text == "" || worksheet.Cells[row, longitud].Text == "" ||
                                worksheet.Cells[row, municipio].Text == "")
                            {                                
                                var newRowError = dataTableError.NewRow();                                
                                newRowError[0] = $"Error en la data en la línea {row}, las columnas Fecha, Región, Zona, Circuito, Latitud, Longitud y Municipio son Requeridas";
                                dataTableError.Rows.Add(newRowError);
                            }
                            else if (date != "")
                            {
                                date = ParseDate(date);
                                if (date.Contains("Error"))
                                {                                                                        
                                    var newRowError = dataTableError.NewRow();                                    
                                    newRowError[0] = date + $" En la línea {row} y columna Fecha";
                                    dataTableError.Rows.Add(newRowError);
                                }
                                else
                                {
                                    var newRow = dataTable.NewRow();
                                    newRow[0] = date;
                                    for (int i = 1; i <= 11; i++)
                                    {
                                        newRow[i] = worksheet.Cells[row, i + 1].Text != "" ? worksheet.Cells[row, i + 1].Text.Trim().ToUpper() : string.Empty;
                                    }
                                    dataTable.Rows.Add(newRow);

                                    var newEntity = new MpLightningDTO();
                                    var aniomes = worksheet.Cells[row, fecha].Text.Split('/', ' ');
                                    newEntity.NameRegion = worksheet.Cells[row, region].Text.Trim().ToUpper();
                                    newEntity.NameZone = worksheet.Cells[row, zona].Text.Trim().ToUpper();
                                    newEntity.NameLocality = worksheet.Cells[row, municipio].Text.Trim().ToUpper();
                                    newEntity.Fparent = worksheet.Cells[row, circuito].Text.Trim().Replace(" ", "");
                                    newEntity.DateEvent = DateTime.Parse(worksheet.Cells[row, fecha].Text);
                                    newEntity.Latitude = float.Parse(worksheet.Cells[row, latitud].Text.Trim());
                                    newEntity.Longitude = float.Parse(worksheet.Cells[row, longitud].Text.Trim());
                                    newEntity.Amperage = float.Parse(worksheet.Cells[row, corriente].Text.Trim());
                                    newEntity.Error = float.Parse(worksheet.Cells[row, error].Text.Trim());
                                    newEntity.Type = int.Parse(worksheet.Cells[row, tipo].Text.Trim());
                                    newEntity.Year = int.Parse(aniomes[2]);
                                    newEntity.Month = int.Parse(aniomes[1]);

                                    listDTOMpLightning.Add(newEntity);

                                }

                            }
                            else
                            {
                                var newRowError = dataTableError.NewRow();
                                newRowError[0] = $"Error en la fila {row} y columna Fecha, la fecha no puede ser nula";
                                dataTableError.Rows.Add(newRowError);
                            }

                        }

                        // Guardar como CSV
                        string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
                        using (var writer = new StreamWriter(outputFilePath))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {

                            foreach (DataRow row in dataTable.Rows)
                            {
                                for (var i = 0; i < 12; i++)
                                {
                                    csv.WriteField(row[i]);
                                }
                                csv.NextRecord();
                            }
                        }

                        if (dataTableError.Rows.Count > 0)
                        {
                            errorFlag = true;
                            RegisterError(dataTableError, inputFolder, filePath);
                        }

                        if (listDTOMpLightning.Count > 0 && errorFlag == false)
                        {
                            int i = 0;
                            while ((i * 1000) < listDTOMpLightning.Count())
                            {
                                var subgroup = listDTOMpLightning.Skip(i * 1000).Take(1000).ToList();
                                var EntityResult = mapper.Map<List<MpLightning>>(subgroup);
                                SaveData(EntityResult);
                                i++;
                                Console.WriteLine(i * 1000);
                            }

                        }
                    }

                }
                if (errorFlag)
                {
                    response.Message = "file with errors";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    response.Message = "All files are created";
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

        private static string ParseDate(string dateString)
        {
            var _timeFormats = new List<string> {
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss tt",
                    "yyyy-MM-dd HH:mm:ss,fff",
                    "yyyy-MM-dd HH:mm:ss,ffff",
                    "yyyy-MM-dd HH:mm:ss,fffff",
                    "yyyy-MM-dd HH:mm:ss,ffffff",
                    "yyyy-MM-dd HH:mm:ss.fff",
                    "yyyy-MM-dd HH:mm:ss.ffff",
                    "yyyy-MM-dd HH:mm:ss.fffff",
                    "yyyy-MM-dd HH:mm:ss.ffffff",
                    "yyyy-MM-dd HH:mm",
                    "dd-MM-yyyy HH:mm",
                    "yyyy/MM/dd HH:mm",
                    "dd/MM/yyyy HH:mm",
                    "d/MM/yyyy HH:mm",
                    "d/MM/yyyy H:mm",
                    "dd/MM/yyyy HH:mm:ss",
                    "d/MM/yyyy HH:mm:ss",
                    "d/MM/yyyy H:mm:ss",
                    "dd/MM/yyyy",
                    "d/MM/yyyy",
                    "dd-MM-yyyy",
            };
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString();
                }
            }
            return $"Error en el formato de fecha {dateString} no es válido.";
            //throw new FormatException($"Error en el formato de fecha {dateString} no es válido.");
        }

        // acciones en bd y mappeo

        public Boolean SaveData(List<MpLightning> request)
        {
            
                var result = rayosCSVDataAccess.SaveData(request);

                return result;            

        }        

    }
}
