using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class RamalesServices : IRamalesServices
    {
        private readonly IRamalesDataAccess ramalesDataAccess;
        private readonly IMapper mapper;
        private readonly string _RamalesDirectoryPath;
        public RamalesServices(IConfiguration configuration, IRamalesDataAccess _ramalesDataAccess, IMapper _mapper)
        {
            ramalesDataAccess = _ramalesDataAccess;
            mapper = _mapper;
            _RamalesDirectoryPath = configuration["RamalesPath"];

        }

        public ResponseEntity<List<string>> SearchData(ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = _RamalesDirectoryPath;

                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    ProcessXlsx(filePath, inputFolder);
                }

                response.Message = "All Registers are created and/or updated";
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

        async void ProcessXlsx(string filePath, string inputFolder)
        {

            string[] fileLines = File.ReadAllLines(filePath);
            var dataTable = new DataTable();
            var dataTableError = new DataTable();
            var filesIOList = new List<FileIoTempDTO>();
            var filesIODetailList = new List<FileIoTempDetailDTO>();
            //var listDTOMpLightning = new List<MpLightningDTO>();

            // agregar nombres a columnas
            for (int i = 1; i <= 8; i++)
            {
                dataTable.Columns.Add($"C{i}");
            }

            // columnas tabla error
            dataTableError.Columns.Add("C1");

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

                var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteDevelopment";

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
                    var date = valueLines[1] != "" ? valueLines[1].Trim().ToString() : string.Empty;
                    //var date = dateTemp.ToString();                    
                    if (valueLines[0] == "" || valueLines[1] == "" || valueLines[2] == "" || valueLines[3] == "" ||
                        valueLines[4] == "" || valueLines[5] == "" || valueLines[6] == "" || valueLines[7] == "" ||
                        valueLines[8] == "" || valueLines[9] == "" || valueLines[10] == "" || valueLines[11] == "" || valueLines[12] == "")
                    {
                        var newRowError = dataTableError.NewRow();
                        newRowError[0] = $"Error en la data en la fila {valueLines}, las columnas Codigo evento, Total Trafo y Total clientes son Requeridas";
                        dataTableError.Rows.Add(newRowError);
                    }
                    else if (date != "")
                    {
                        date = ParseDate(date);
                        if (date.Contains("Error"))
                        {
                            var newRowError = dataTableError.NewRow();
                            newRowError[0] = date + $" En la fila {valueLines} y columna Fecha";
                            dataTableError.Rows.Add(newRowError);
                        }
                        else
                        {
                            var aniomes = date.Split('/', ' ');
                            var LacUnit = fileLacList.Where(x => x.event_code == valueLines[0]).ToList();
                            var UiaLists = new List<string>();
                            foreach (var item1 in LacUnit)
                            {
                                UiaLists.Add(item1.Uia);
                            }

                            var countCodeSig = allAssetList.Where(x => UiaLists.Contains(x.Uia)).ToList();
                            //var countClients = fileTc1List.Where(x => UiaLists.Contains(x.Uia) && x.Month == int.Parse(aniomes[1]) && x.Year == int.Parse(aniomes[2])).Count();

                            var newRow = dataTable.NewRow();
                            newRow[0] = date;
                            newRow[1] = valueLines[0];
                            newRow[2] = valueLines[10];
                            newRow[3] = countCodeSig.Count();
                            newRow[4] = int.Parse(valueLines[10]) - countCodeSig.Count();

                            dataTable.Rows.Add(newRow);

                            var filesIOUnit = new FileIoTempDTO();
                            var dateOnlyIniPart = valueLines[1].Split(' ');
                            var dateOnlyFinPart = valueLines[2].Split(' ');
                            filesIOUnit.CodigoEvento = valueLines[0];
                            filesIOUnit.FechaInicio = DateOnly.Parse(dateOnlyIniPart[0]);
                            filesIOUnit.FechaFinal = DateOnly.Parse(dateOnlyFinPart[0]);
                            filesIOUnit.Duracion = float.Parse(valueLines[3]);
                            filesIOUnit.CodigoCircuito = valueLines[4];
                            filesIOUnit.CodInteruptor = valueLines[5];
                            filesIOUnit.NombreTipoInteruptor = valueLines[6];
                            filesIOUnit.ApoyoApertura = valueLines[7];
                            filesIOUnit.ApoyoFalla = valueLines[8];
                            filesIOUnit.CodigoCausaEvento = int.Parse(valueLines[9]);
                            filesIOUnit.TotalTafo = int.Parse(valueLines[10]);
                            filesIOUnit.TotalClientes = int.Parse(valueLines[11]);
                            filesIOUnit.TotalOperaciones = int.Parse(valueLines[12]);

                            filesIOList.Add(filesIOUnit);

                            foreach (var item3 in countCodeSig)
                            {
                                var filesIODetailUnit = new FileIoTempDetailDTO();

                                filesIODetailUnit.CodigoEvento = valueLines[0];
                                filesIODetailUnit.FechaInicio = DateOnly.Parse(dateOnlyIniPart[0]);
                                filesIODetailUnit.FechaFinal = DateOnly.Parse(dateOnlyFinPart[0]);
                                filesIODetailUnit.Duracion = float.Parse(valueLines[3]);
                                filesIODetailUnit.CodigoCircuito = valueLines[4].Trim().Replace(" ", "");
                                filesIODetailUnit.CodInteruptor = valueLines[5];
                                filesIODetailUnit.NombreTipoInteruptor = valueLines[6];
                                filesIODetailUnit.ApoyoApertura = valueLines[7];
                                filesIODetailUnit.ApoyoFalla = valueLines[8];
                                filesIODetailUnit.CodigoCausaEvento = int.Parse(valueLines[9]);
                                filesIODetailUnit.TotalTafo = 1;
                                filesIODetailUnit.UiaTrafo = item3.Uia;
                                filesIODetailUnit.TotalClientes = int.Parse(valueLines[11]);
                                filesIODetailUnit.TotalOperaciones = int.Parse(valueLines[12]);

                                filesIODetailList.Add(filesIODetailUnit);
                            }

                        }

                    }
                    else
                    {
                        var newRowError = dataTableError.NewRow();
                        newRowError[0] = $"Error en la fila {valueLines} y columna Fecha, la fecha no puede ser nula";
                        dataTableError.Rows.Add(newRowError);
                    }

                }

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

            if (dataTableError.Rows.Count > 0)
            {
                RegisterError(dataTableError, inputFolder, filePath);
            }


            if (filesIOList.Count > 0)
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

            if (filesIODetailList.Count > 0)
            {
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
