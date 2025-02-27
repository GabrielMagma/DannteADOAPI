using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class RamalesServices : IRamalesServices
    {
        private readonly IRamalesDataAccess ramalesDataAccess;        
        private readonly string _RamalesDirectoryPath;
        private readonly IStatusFileEepDataAccess statusFileDataAccess;
        private readonly IMapper mapper;

        public RamalesServices(IConfiguration configuration, 
            IRamalesDataAccess _ramalesDataAccess,
            IStatusFileEepDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            ramalesDataAccess = _ramalesDataAccess;            
            _RamalesDirectoryPath = configuration["RamalesPath"];
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;

        }

        public async Task<ResponseEntity<List<string>>> SearchData(RamalesValidationDTO request, ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = _RamalesDirectoryPath;
                var responseTotal = false;
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                  var responseProcess = ProcessFile(request, filePath, inputFolder);
                  Console.WriteLine(responseProcess);
                }
                if (responseTotal)
                {
                    response.Message = "All Registers are created and/or updated";
                    response.SuccessData = true;
                    response.Success = true;
                    return response;
                }
                else
                {
                    response.Message = "File with errors";
                    response.SuccessData = false;
                    response.Success = false;
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

        async Task<bool> ProcessFile(RamalesValidationDTO request, string filePath, string inputFolder)
        {
            string[] fileLines = File.ReadAllLines(filePath);
            var dataTable = new DataTable();
            var dataTableError = new DataTable();
            var filesIOList = new List<FileIoTempDTO>();
            var filesIODetailList = new List<FileIoTempDetailDTO>();
            var count = 1;

            var errorFlag = false;

            int codEvento = 0;
            int fechaIni = 1;
            int fechaFin = 2;
            int duracion = 3;
            int fparent = 4;
            int codInter = 5;
            int nombreInter = 6;
            int apoyoApertura = 7;
            int apoyoFalla = 8;
            int codCausaEvent = 9;
            int totalTrafo = 10;
            int totalCliente = 11;
            int totalOpe = 12;

            var statusFileList = new List<StatusFileDTO>();

            var statusFilesingle = new StatusFileDTO();
            // Extraer el nombre del archivo sin la extensión
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
            statusFilesingle.UserId = request.UserId;
            statusFilesingle.FileName = fileName;
            statusFilesingle.FileType = "RAYOS";
            statusFilesingle.Year = request.Year;
            statusFilesingle.Month = request.Month;

            if (request.Encabezado == false)
            {
                codEvento = request.columns.CodEvento - 1;
                fechaIni = request.columns.FechaIni - 1;
                fechaFin = request.columns.FechaFin - 1;
                duracion = request.columns.Duracion - 1;
                fparent = request.columns.Fparent - 1;
                codInter = request.columns.CodInter - 1;
                nombreInter = request.columns.NombreInter - 1;
                apoyoApertura = request.columns.ApoyoApertura - 1;
                apoyoFalla = request.columns.ApoyoFalla - 1;
                codCausaEvent = request.columns.CodCausaEvent - 1;
                totalTrafo = request.columns.TotalTrafo - 1;
                totalCliente = request.columns.TotalCliente - 1;
                totalOpe = request.columns.TotalOpe - 1;
            }


            // agregar nombres a columnas
            for (int i = 1; i <= 8; i++)
            {
                dataTable.Columns.Add($"C{i}");
            }

            // columnas tabla error
            dataTableError.Columns.Add("C1");
            dataTableError.Columns.Add("C2");

            int j = 0;
            while ((j * 1000) < fileLines.Count())
            {
                var subgroup = fileLines.Skip(j * 1000).Take(1000).ToList();

                var listDataString = new StringBuilder();
                var listUIA = new StringBuilder();
                var fileLacList = new List<FilesLacDTO>();
                var allAssetList = new List<AssetDTO>();

                foreach (var item in subgroup)
                {
                    var valueLines = item.Split(",");
                    if (valueLines[0] != "CODIGOEVENTO")
                    {
                        listDataString.Append($"'{valueLines[0]}',");
                    }
                }

                var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteEssaTesting";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                    var SelectQuery = $@"SELECT uia, event_code FROM public.files_lac where event_code in ({listDef})";
                    using (var reader = new NpgsqlCommand(SelectQuery, connection))
                    {
                        try
                        {

                            using (var result = await reader.ExecuteReaderAsync())
                            {
                                while (await result.ReadAsync())
                                {
                                    listUIA.Append($"'{result[0]}',");
                                    var temp = new FilesLacDTO();
                                    temp.Uia = result[0].ToString();
                                    temp.event_code = result[1].ToString();
                                    fileLacList.Add(temp);
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
                    var listUiaDef = listUIA.Remove(listUIA.Length - 1, 1);
                    var CountQueryAssets = $@"SELECT uia, code_sig FROM public.all_asset where uia in ({listUiaDef})";
                    using (var reader = new NpgsqlCommand(CountQueryAssets, connection))
                    {
                        try
                        {
                            using (var result = await reader.ExecuteReaderAsync())
                            {
                                while (await result.ReadAsync())
                                {
                                    var temp = new AssetDTO();

                                    temp.Uia = result[0].ToString();
                                    temp.Code_sig = result[1].ToString();
                                    allAssetList.Add(temp);
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

                foreach (var item in subgroup)
                {
                    var valueLines = item.Split(';', ',');
                    var columnCount = 13;
                    var beacon = 0;
                    for (int i = 0; i < columnCount; i++)
                    {
                        if (valueLines[i] == "")
                        {
                            beacon++;
                        }
                    }
                    if (beacon == columnCount + 1)
                    {
                        break;                    
                    }

                    var date = valueLines[fechaIni] != "" ? valueLines[fechaIni].Trim().ToString() : string.Empty;
                    //var date = dateTemp.ToString();                    
                    if (string.IsNullOrEmpty(valueLines[codEvento]) || string.IsNullOrEmpty(valueLines[fechaIni]) || string.IsNullOrEmpty(valueLines[fechaFin]) || 
                        string.IsNullOrEmpty(valueLines[duracion]) || string.IsNullOrEmpty(valueLines[fparent]) || string.IsNullOrEmpty(valueLines[codInter]) || 
                        string.IsNullOrEmpty(valueLines[nombreInter]) || string.IsNullOrEmpty(valueLines[apoyoApertura]) || string.IsNullOrEmpty(valueLines[apoyoFalla]) || 
                        string.IsNullOrEmpty(valueLines[codCausaEvent]) || string.IsNullOrEmpty(valueLines[totalTrafo]) || string.IsNullOrEmpty(valueLines[totalCliente]) || 
                        string.IsNullOrEmpty(valueLines[totalOpe]))
                    {
                        var newRowError = dataTableError.NewRow();
                        newRowError[0] = $"Error en la data en la fila {count}, las columnas Codigo evento, Total Trafo y Total clientes son Requeridas";
                        newRowError[1] = $"{valueLines}";
                        dataTableError.Rows.Add(newRowError);
                    }
                    else if (date != "")
                    {
                        date = ParseDate(date);
                        if (date.Contains("Error"))
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = date + $" En la fila {count} y columna Fecha";
                            newRowError[1] = $"{valueLines}";
                            dataTableError.Rows.Add(newRowError);
                        }
                        else
                        {
                            var aniomes = date.Split('/', ' ');
                            var LacUnit = fileLacList.Where(x => x.event_code == valueLines[codEvento]).ToList();
                            var UiaLists = new List<string>();
                            foreach (var item1 in LacUnit)
                            {
                                UiaLists.Add(item1.Uia);
                            }

                            var countCodeSig = allAssetList.Where(x => UiaLists.Contains(x.Uia)).ToList();                            

                            var newRow = dataTable.NewRow();
                            newRow[0] = date;
                            newRow[1] = valueLines[codEvento].Trim();
                            newRow[2] = valueLines[totalTrafo].Trim();
                            newRow[3] = countCodeSig.Count();
                            newRow[4] = int.Parse(valueLines[totalTrafo]) - countCodeSig.Count();

                            dataTable.Rows.Add(newRow);

                            var filesIOUnit = new FileIoTempDTO();
                            var dateOnlyIniPart = valueLines[fechaIni].Split(' ');
                            var dateOnlyFinPart = valueLines[fechaFin].Split(' ');
                            filesIOUnit.CodigoEvento = valueLines[codEvento].Trim();
                            filesIOUnit.FechaInicio = DateOnly.Parse(dateOnlyIniPart[0]);
                            filesIOUnit.FechaFinal = DateOnly.Parse(dateOnlyFinPart[0]);
                            filesIOUnit.Duracion = float.Parse(valueLines[duracion]);
                            filesIOUnit.CodigoCircuito = valueLines[fparent].Trim();
                            filesIOUnit.CodInteruptor = valueLines[codInter].Trim();
                            filesIOUnit.NombreTipoInteruptor = valueLines[nombreInter].Trim().ToUpper();
                            filesIOUnit.ApoyoApertura = valueLines[apoyoApertura].Trim();
                            filesIOUnit.ApoyoFalla = valueLines[apoyoFalla].Trim();
                            filesIOUnit.CodigoCausaEvento = int.Parse(valueLines[codCausaEvent]);
                            filesIOUnit.TotalTafo = int.Parse(valueLines[totalTrafo]);
                            filesIOUnit.TotalClientes = int.Parse(valueLines[totalCliente]);
                            filesIOUnit.TotalOperaciones = int.Parse(valueLines[totalOpe]);

                            filesIOList.Add(filesIOUnit);

                            foreach (var item3 in countCodeSig)
                            {
                                var filesIODetailUnit = new FileIoTempDetailDTO();

                                filesIODetailUnit.CodigoEvento = valueLines[codEvento].Trim();
                                filesIODetailUnit.FechaInicio = DateOnly.Parse(dateOnlyIniPart[0]);
                                filesIODetailUnit.FechaFinal = DateOnly.Parse(dateOnlyFinPart[0]);
                                filesIODetailUnit.Duracion = float.Parse(valueLines[duracion]);
                                filesIODetailUnit.CodigoCircuito = valueLines[fparent].Trim().Replace(" ", "");
                                filesIODetailUnit.CodInteruptor = valueLines[codInter].Trim();
                                filesIODetailUnit.NombreTipoInteruptor = valueLines[nombreInter].Trim().ToUpper();
                                filesIODetailUnit.ApoyoApertura = valueLines[apoyoApertura].Trim();
                                filesIODetailUnit.ApoyoFalla = valueLines[apoyoFalla].Trim();
                                filesIODetailUnit.CodigoCausaEvento = int.Parse(valueLines[codCausaEvent]);
                                filesIODetailUnit.TotalTafo = 1;
                                filesIODetailUnit.UiaTrafo = item3.Uia;
                                filesIODetailUnit.TotalClientes = int.Parse(valueLines[totalCliente]);
                                filesIODetailUnit.TotalOperaciones = int.Parse(valueLines[totalOpe]);

                                filesIODetailList.Add(filesIODetailUnit);
                            }

                        }

                    }
                    else
                    {
                        var newRowError = dataTableError.NewRow();
                        newRowError[0] = $"Error en la fila {count} y columna Fecha, la fecha no puede ser nula";
                        newRowError[1] = $"{valueLines}";
                        dataTableError.Rows.Add(newRowError);
                    }

                }
                count++;
                j++;

                Console.WriteLine(j * 1000);
            }

            //Guardar como CSV
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataRow row in dataTable.Rows)
                {
                    for (var i = 0; i < 5; i++)
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

            statusFileList.Add(statusFilesingle);

            if (filesIOList.Count > 0 && errorFlag == false)
            {
                int i = 0;
                while ((i * 1000) < filesIOList.Count())
                {
                    var subgroup = filesIOList.Skip(i * 1000).Take(1000).ToList();
                    var subgroupMap = mapper.Map<List<FileIoTemp>>(subgroup);
                    SaveData(subgroupMap);
                    i++;
                    Console.WriteLine(i * 1000);
                }

            }

            if (filesIODetailList.Count > 0 && errorFlag == false)
            {
                var subgroupMaped = mapper.Map<List<StatusFile>>(statusFileList);
                var resultSave = await statusFileDataAccess.SaveDataList(subgroupMaped);

                int i = 0;
                while ((i * 1000) < filesIODetailList.Count())
                {
                    var subgroup = filesIODetailList.Skip(i * 1000).Take(1000).ToList();
                    var subgroupMap = mapper.Map<List<FileIoTempDetail>>(subgroup);
                    SaveDataDetail(subgroupMap);
                    i++;
                    Console.WriteLine(i * 1000);
                }

            }

            

            Console.WriteLine("Proceso terminado");

            return errorFlag;

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

        public Boolean SaveData(List<FileIoTemp> request)
        {
            
            var result = ramalesDataAccess.SaveData(request);

            return result;            

        }

        public Boolean SaveDataDetail(List<FileIoTempDetail> request)
        {
            var result = ramalesDataAccess.SaveDataList(request);
            return result;

        }

    }
}
