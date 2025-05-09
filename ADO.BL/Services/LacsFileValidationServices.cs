using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class LacsFileValidationServices : ILacsFileValidationServices
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _lacDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ILACValidationEssaServices lACValidationServices;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IMapper mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        public LacsFileValidationServices(IConfiguration configuration, 
            ILACValidationEssaServices _lACValidationServices,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _lacDirectoryPath = configuration["FilesLACPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            lACValidationServices = _lACValidationServices;
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
            _hubContext = hubContext;
        }

        public async Task<ResponseQuery<bool>> ReadFilesLacs(LacValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var responseError = new ResponseEntity<List<StatusFileDTO>>();
                var viewErrors =  await lACValidationServices.ValidationLAC(request, responseError);
                if (viewErrors.Success == false) {
                    response.Message = viewErrors.Message;
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {

                    var completed1 = await BeginProcess(request);
                    Console.WriteLine(completed1);

                    //update status queue
                    var subgroupMap = mapper.Map<List<QueueStatusLac>>(viewErrors.Data);
                    if (completed1 != "Completed")
                    {

                        foreach (var item in subgroupMap)
                        {
                            item.Status = 3;
                        }
                        response.Message = "File with errors";
                        response.SuccessData = false;
                        response.Success = false;
                    }
                    else
                    {
                        response.Message = "All files created";
                        response.SuccessData = true;
                        response.Success = true;
                    }
                    

                    var resultSave = await statusFileDataAccess.UpdateDataLACList(subgroupMap);

                    
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

        private async Task<string> BeginProcess(LacValidationDTO request)
        {
            try
            {
                Console.WriteLine("BeginProcess");
                // Obtener todos los archivos CSV en la carpeta que terminan en _withN.csv
                var files = Directory.GetFiles(_lacDirectoryPath, "*_Correct.csv")
                    .Where(file => !file.EndsWith("_unchanged.csv")
                                   && !file.EndsWith("_continues.csv")
                                   && !file.EndsWith("_continuesInvalid.csv")
                                   && !file.EndsWith("_closed.csv")
                                   && !file.EndsWith("_closedInvalid.csv"))
                    .ToList().OrderBy(f => f)
                     .ToArray();

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileNameTemp = $"{fileName.Substring(0, 12)}.csv";
                    if (request.NombreArchivo != null)
                    {
                        if (!fileName.Contains(request.NombreArchivo))
                        {
                            continue;
                        }
                    }

                    await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileNameTemp} se está validando");
                    await CreateLacFiles(filePath);
                    await _hubContext.Clients.All.SendAsync("Receive", true, $"Archivo {fileNameTemp} subido exitosamente.");
                }
                Console.WriteLine("EndBeginProcess");
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"Error, {ex.Message}";
            }
        }

        private async Task CreateLacFiles(string filePath)
        {

            List<string> lines = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileUnchanged = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_unchanged.csv");
            string fileContinues = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_continues.csv");
            string fileContinuesInvalid = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_continuesInvalid.csv");
            string fileClosed = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_closed.csv");
            string fileClosedInvalid = Path.Combine(_lacDirectoryPath, $"{fileNameWithoutExtension}_closedInvalid.csv");

            var linesUnchanged = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool notEmptyFields = !(string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1])) &&
                                       !(string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]));
                return conditionN && notEmptyFields;
            }).ToList();

            var linesContinues = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionS && emptyFields;
            }).ToList();

            var linesContinuesInvalid = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool emptyFields = string.IsNullOrEmpty(columns[2]) || string.IsNullOrWhiteSpace(columns[2]);
                return conditionN && emptyFields;
            }).ToList();

            var linesClosed = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionN = columns.Length > 6 && columns[6] == "N";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionN && emptyFields;
            }).ToList();

            var linesClosedInvalid = lines.Where(line =>
            {
                var columns = line.Split(new[] { ',', ';' }, StringSplitOptions.None);
                bool conditionS = columns.Length > 6 && columns[6] == "S";
                bool emptyFields = string.IsNullOrEmpty(columns[1]) || string.IsNullOrWhiteSpace(columns[1]);
                return conditionS && emptyFields;
            }).ToList();

            if (linesUnchanged.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileUnchanged, linesUnchanged);
            }

            if (linesContinues.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinues, linesContinues);
            }

            if (linesContinuesInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileContinuesInvalid, linesContinuesInvalid);
            }

            if (linesClosed.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosed, linesClosed);
            }

            if (linesClosedInvalid.Any())
            {
                WriteAllLinesWithoutTrailingNewline(fileClosedInvalid, linesClosedInvalid);
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
