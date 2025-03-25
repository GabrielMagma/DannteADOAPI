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
    public class PodasEssaServices : IPodasEssaServices
    {
        private readonly IPodasEssaDataAccess podasEssaDataAccess;        
        private readonly string _PodasDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly IMapper mapper;

        public PodasEssaServices(IConfiguration configuration,
            IPodasEssaDataAccess _podasEssaDataAccess,            
            IMapper _mapper)
        {
            podasEssaDataAccess = _podasEssaDataAccess;
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _PodasDirectoryPath = configuration["PodasPath"];            
            mapper = _mapper;
        }        

        public async Task<ResponseEntity<List<string>>> SaveDataExcel(ResponseEntity<List<string>> response)
        {
            try
            {
                string inputFolder = _PodasDirectoryPath;
                var errorFlag = false;                

                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.xlsx"))                
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var dataTableError = new DataTable();
                        // columnas tabla error
                        dataTableError.Columns.Add("C1");
                        for (int j = 0; j < 9; j++)
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

                                if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 8].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 9].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 10].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 11].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 12].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 13].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 14].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 15].Text) || string.IsNullOrEmpty(worksheet.Cells[row, 16].Text) ||
                                    string.IsNullOrEmpty(worksheet.Cells[row, 17].Text) )
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] = $"Error en la data en la línea {row} y hoja {worksheet}, está incompleta";
                                    dataTableError.Rows.Add(newRowError);
                                    continue;
                                }

                                var date = string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? "31/12/2099"  : worksheet.Cells[row, 5].Text.ToString();

                                var date2 = ParseDate(date);
                                if (date2 == DateOnly.Parse("31/12/2099"))
                                {
                                    var newRowError = dataTableError.NewRow();
                                    newRowError[0] =  $"Error en el dato de la fecha ejecución en la línea {row} y hoja {worksheet}";
                                    dataTableError.Rows.Add(newRowError);
                                }
                                else
                                {                                    

                                    var newEntity = new PodaDTO();
                                    
                                    newEntity.NameRegion = worksheet.Cells[row, 1].Text.Trim().ToUpper();
                                    newEntity.NameZone = worksheet.Cells[row, 2].Text.Trim().ToUpper();
                                    newEntity.Circuit = worksheet.Cells[row, 3].Text.Trim().ToUpper();
                                    newEntity.NameLocation = worksheet.Cells[row, 4].Text.Trim().ToUpper();
                                    newEntity.DateExecuted = date2;
                                    newEntity.Scheduled = worksheet.Cells[row, 6].Text.Trim().ToUpper();
                                    newEntity.NoOt = worksheet.Cells[row, 7].Text.Trim().ToUpper();
                                    newEntity.StateOt = worksheet.Cells[row, 8].Text.Trim().ToUpper();
                                    newEntity.DateState = worksheet.Cells[row, 9].Text != null ? ParseDate(worksheet.Cells[row, 9].Text.ToString()) : (DateOnly?)null;
                                    newEntity.Pqr = worksheet.Cells[row, 10].Text.Trim().ToUpper();
                                    newEntity.NoReport = worksheet.Cells[row, 11].Text.Trim().ToUpper();
                                    newEntity.Consig = worksheet.Cells[row, 12].Text.Trim().ToUpper();
                                    newEntity.BeginSup = worksheet.Cells[row, 13].Text.Trim().ToUpper();
                                    newEntity.EndSup = worksheet.Cells[row, 14].Text.Trim().ToUpper();
                                    newEntity.Urban = worksheet.Cells[row, 15].Text.Trim().ToUpper();
                                    newEntity.Item = worksheet.Cells[row, 16].Text.Trim().ToUpper();
                                    newEntity.Description = worksheet.Cells[row, 17].Text.Trim().ToUpper();


                                    listDTOPodas.Add(newEntity);

                                }

                            }                                                                                   

                            if (listDTOPodas.Count > 0)
                            {
                                int i = 0;
                                while ((i * 10000) < listDTOPodas.Count())
                                {
                                    var subgroup = listDTOPodas.Skip(i * 10000).Take(10000).ToList();
                                    var EntityResult = mapper.Map<List<IaPoda>>(subgroup);
                                    SaveData(EntityResult);
                                    i++;
                                    Console.WriteLine(i * 10000);
                                }

                            }
                        }
                        if (dataTableError.Rows.Count > 0)
                        {
                            errorFlag = true;
                            RegisterError(dataTableError, inputFolder, filePath);
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
                    csv.WriteField(row[0]);
                    csv.NextRecord();
                }
            }
        }

        private DateOnly ParseDate(string dateString)
        {
            var value = DateOnly.Parse("31/12/2099");

            foreach (var format in _timeFormats)
            {
                
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    value =  parsedDate;
                }                
            }
            return value;            
        }

        // acciones en bd y mappeo

        public Boolean SaveData(List<IaPoda> request)
        {
            
                var result = podasEssaDataAccess.SaveData(request);

                return result;            

        }        

    }
}
