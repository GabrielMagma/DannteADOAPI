using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileTT2ValidationServices : IFileTT2ValidationServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _TT2FixDirectoryPath;
        private readonly string _connectionString;
        public FileTT2ValidationServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _TT2FixDirectoryPath = configuration["TT2DirectoryPath"];
        }

        public async Task<ResponseQuery<bool>> ValidationTT2(ResponseQuery<bool> response)
        {
            try
            {

                string inputFolder = _TT2FixDirectoryPath;
                var flagValidation = false;
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv").OrderBy(f => f)
                     .ToArray())
                {
                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();

                    // columnas tabla datos correctos
                    dataTable.Columns.Add("COD_CREG");
                    dataTable.Columns.Add("CODIGO_UBICACION");
                    dataTable.Columns.Add("GRUPO_CALIDAD");
                    dataTable.Columns.Add("IDMERCADO");
                    dataTable.Columns.Add("CAPACIDAD");
                    dataTable.Columns.Add("PROPIEDAD");
                    dataTable.Columns.Add("TIPO_SUBESTACION");
                    dataTable.Columns.Add("LONGITUD");
                    dataTable.Columns.Add("LATITUD");
                    dataTable.Columns.Add("ALTITUD");
                    dataTable.Columns.Add("ESTADO");
                    dataTable.Columns.Add("FECHA_ESTADO");
                    dataTable.Columns.Add("RESOLUCION_METODOLOGIA");

                    dataTableError.Columns.Add("Error");
                    dataTableError.Columns.Add("Data");

                    string[] fileLines = File.ReadAllLines(filePath);
                    var listDataString = new StringBuilder();
                    var listUIA = new StringBuilder();
                    var allAssetList = new List<AssetDTO>();
                    foreach (var item in fileLines)
                    {
                        var valueLinesTemp = item.Split(',',';');
                        if (valueLinesTemp[0] != "CODIGO TRANSFORMADOR")
                        {
                            listDataString.Append($"'{valueLinesTemp[0]}',");
                        }
                    }                    

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);
                        var SelectQuery = $@"SELECT uia, code_sig FROM public.all_asset where uia in ({listDef})";
                        using (var reader = new NpgsqlCommand(SelectQuery, connection))
                        {
                            try
                            {

                                using (var result = await reader.ExecuteReaderAsync())
                                {
                                    while (await result.ReadAsync())
                                    {
                                        listUIA.Append($"'{result[0]}',");
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

                    var ends = new List<string>()
                    {
                        "CODIGO TRANSFORMADOR",
                        "COD_CREG",
                        "Codigo_Transformador",
                    };

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');

                        if (!ends.Contains(valueLines[0].Replace("ó", "o")))
                        {
                            
                            var codesigTemp = allAssetList.FirstOrDefault(x => x.Uia == valueLines[0]);

                            if (codesigTemp != null)
                            {
                                if (valueLines.Length == 12) {
                                    var newRow = dataTable.NewRow();
                                    newRow[0] = valueLines[0];
                                    newRow[1] = codesigTemp.Code_sig;
                                    newRow[2] = valueLines[1];
                                    newRow[3] = valueLines[2];
                                    newRow[4] = valueLines[3];
                                    newRow[5] = valueLines[4];
                                    newRow[6] = valueLines[5];
                                    newRow[7] = valueLines[6];
                                    newRow[8] = valueLines[7];
                                    newRow[9] = valueLines[8];
                                    newRow[10] = valueLines[9];
                                    newRow[11] = valueLines[10];
                                    newRow[12] = valueLines[11];

                                    dataTable.Rows.Add(newRow);
                                }
                                else
                                {
                                    var newRow = dataTable.NewRow();
                                    newRow[0] = valueLines[0];
                                    newRow[1] = valueLines[1];
                                    newRow[2] = valueLines[2];
                                    newRow[3] = valueLines[3];
                                    newRow[4] = valueLines[4];
                                    newRow[5] = valueLines[5];
                                    newRow[6] = valueLines[6];
                                    newRow[7] = valueLines[7];
                                    newRow[8] = valueLines[8];
                                    newRow[9] = valueLines[9];
                                    newRow[10] = valueLines[10];
                                    newRow[11] = valueLines[11];
                                    newRow[12] = valueLines[12];

                                    dataTable.Rows.Add(newRow);
                                }
                                
                            }
                            else
                            {
                                var newRow = dataTableError.NewRow();
                                newRow[0] = "no Code_sig";
                                newRow[1] = item;
                                dataTableError.Rows.Add(newRow);
                            }
                        }
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        createCSV(dataTable, filePath);
                    }
                    
                    if (dataTableError.Rows.Count > 0)
                    {
                        flagValidation = true;
                        createCSVError(dataTableError, filePath);
                    }

                }

                if (flagValidation)
                {
                    response.Message = "files with errors";
                    response.SuccessData = false;
                    //response.Success = false;
                    response.Success = true; // cambiar en prod
                    return response;

                }
                else
                {
                    response.Message = "All files created";
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

        private void createCSV(DataTable table, string filePath)
        {
            string inputFolder = _TT2FixDirectoryPath;
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Fixed.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataColumn column in table.Columns)
                {
                    csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();

                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < 13; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private void createCSVError(DataTable table, string filePath)
        {
            string inputFolder = _TT2FixDirectoryPath;
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


    }
}
