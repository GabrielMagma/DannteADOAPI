using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class SSPDFileValidationServices : ISSPDFileValidationServices
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;        
        private readonly string _sspdDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ISSPDValidationEepServices SSPDValidationServices;        
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IMapper mapper;

        public SSPDFileValidationServices(IConfiguration configuration, 
            ISSPDValidationEepServices _SSPDValidationServices,            
            IStatusFileDataAccess _statuFileEssaDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");            
            _sspdDirectoryPath = configuration["SspdDirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            SSPDValidationServices = _SSPDValidationServices;
            statusFileDataAccess = _statuFileEssaDataAccess;
            mapper = _mapper;

        }

        public async Task<ResponseQuery<bool>> ReadFilesSspd(LacValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {                
                var responseError = new ResponseEntity<List<StatusFileDTO>>();
                var viewErrors = await SSPDValidationServices.ValidationSSPD(request, responseError);
                if (viewErrors.Success == false)
                {
                    response.Message = viewErrors.Message;
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    var completed1 = await BeginProcess();
                    Console.WriteLine(completed1);

                    //update status queue
                    var subgroupMap = mapper.Map<List<QueueStatusSspd>>(viewErrors.Data);
                    if (completed1 != "Completed")
                    {
                        foreach (var item in subgroupMap)
                        {
                            item.Status = 3;
                        }
                        response.Message = "loaded file have some errors, please fix it and upload again";
                        response.SuccessData = false;
                        response.Success = false;
                    }
                    else
                    {
                        response.Message = "All files created";
                        response.SuccessData = true;
                        response.Success = true;
                    }


                    var resultSave = await statusFileDataAccess.UpdateDataSSPDList(subgroupMap);

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

        private async Task<string> BeginProcess()
        {
            try
            {
                Console.WriteLine("BeginProcess");
                // Obtener todos los archivos CSV en la carpeta que terminan en _withN.csv
                var files = Directory.GetFiles(_sspdDirectoryPath, "*_Correct.csv")
                   .Where(file => !file.EndsWith("_unchanged.csv")
                                  && !file.EndsWith("_continuesInsert.csv")
                                  && !file.EndsWith("_continuesUpdate.csv")
                                  && !file.EndsWith("_continuesInvalid.csv")
                                  && !file.EndsWith("_closed.csv")
                                  && !file.EndsWith("_closedInvalid.csv")
                                  && !file.EndsWith("_delete.csv")
                                  && !file.EndsWith("_update.csv"))
                   .ToList().OrderBy(f => f)
                     .ToArray();

                foreach (var filePath in files)
                {
                   await CreateSspdFiles(filePath);
                    Console.WriteLine($"Archivo {filePath} subido exitosamente.");
                }
                Console.WriteLine("EndBeginProcess");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        private async Task CreateSspdFiles(string filePath)
        {
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
            }

            // Crear los nombres de los archivos adicionales basados en el nombre del archivo original
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileUnchanged = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_unchanged.csv");
            string fileContinuesInsert = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_continuesInsert.csv");
            string fileContinuesUpdate = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_continuesUpdate.csv");
            string fileContinuesInvalid = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_continuesInvalid.csv");
            string fileClosed = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_closed.csv");
            string fileClosedInvalid = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_closedInvalid.csv");
            string fileToDelete = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_delete.csv");
            string fileToUpdate = Path.Combine(_sspdDirectoryPath, $"{fileNameWithoutExtension}_update.csv");

            // Filtrar las líneas que contienen "N" en la columna 6 y columna 10 = 1 para agregar
            var linesUnchanged = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool conditionAdd = columns.Length > 10 && columns[10] == "1";
                bool notEmptyFields = !(string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1])) &&
                                       !(string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]));
                return conditionN && conditionAdd && notEmptyFields;
            }).ToList();


            // Filtrar las líneas que contienen "S" en la columna 6 y columna 10 = 2 para agregar
            var linesContinuesInsert = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool conditionAdd = columns.Length > 10 && columns[10] == "1";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionS && conditionAdd && emptyFields;
            }).ToList();


            // Filtrar las líneas que contienen "S" en la columna 6 y columna 10 = 2 para agregar
            var linesContinuesUpdate = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionS && conditionAdd && emptyFields;
            }).ToList();


            var linesContinuesInvalid = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionN && conditionAdd && emptyFields;
            }).ToList();


            var linesClosed = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionN && conditionAdd && emptyFields;
            }).ToList();

            var linesClosedInvalid = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool conditionAdd = columns.Length > 10 && columns[10] == "2";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionS && conditionAdd && emptyFields;
            }).ToList();

            // Filtrar las líneas donde columna 10 = 3 para eliminar
            var linesToDelete = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionDelete = columns.Length > 10 && columns[10] == "3";
                return conditionDelete;
            }).ToList();

            // Filtrar las líneas donde columna 10 = 2 para actualizar
            var linesToUpdate = lines.Where(line =>
            {
                var columns = line.Split(',');
                bool conditionUpdate = columns.Length > 10 && columns[10] == "2";
                bool notEmptyFields = !(string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1])) &&
                                       !(string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]));
                return conditionUpdate && notEmptyFields;
            }).ToList();

            // Escribir las líneas con "N" y columna 10 = 1 en el archivo correspondiente
            if (linesUnchanged.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileUnchanged, linesUnchanged);
            }

            // Escribir las líneas con "S" y columna 10 = 1 en el archivo correspondiente
            if (linesContinuesInsert.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesInsert, linesContinuesInsert);
            }


            // Escribir las líneas con "S" y columna 10 = 2 en el archivo correspondiente
            if (linesContinuesUpdate.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesUpdate, linesContinuesUpdate);
            }

            // Escribir las líneas con "N" y columna 10 = 2 en el archivo correspondiente
            if (linesContinuesInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesInvalid, linesContinuesInvalid);
            }

            // Escribir las líneas con "N" y columna 10 = 2 en el archivo correspondiente
            if (linesClosed.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosed, linesClosed);
            }

            // Escribir las líneas con "S" y columna 10 = 2 en el archivo correspondiente
            if (linesClosedInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosedInvalid, linesClosedInvalid);
            }

            // Escribir las líneas con columna 10 = 3 en el archivo correspondiente
            if (linesToDelete.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileToDelete, linesToDelete);
            }

            // Escribir las líneas con columna 10 = 2 en el archivo correspondiente
            if (linesToUpdate.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileToUpdate, linesToUpdate);
            }

        }

        private void WriteAllLinesWithoutTrailingNewline(string path, List<string> lines)
        {
            using (var writer = new StreamWriter(path))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i == lines.Count - 1)
                    {
                        writer.Write(lines[i]);
                    }
                    else
                    {
                        writer.WriteLine(lines[i]);
                    }
                }
            }
        }

    }
}
