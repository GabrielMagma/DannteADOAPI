﻿using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class SSPDValidationEepServices : ISSPDValidationEepServices
    {
        private readonly IConfiguration _configuration;
        private readonly string[] _timeFormats;
        private readonly string _FilesLACDirectoryPath;
        private readonly IMapper mapper;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IHubContext<NotificationHub> _hubContext;
        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public SSPDValidationEepServices(IConfiguration configuration,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _configuration = configuration;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _FilesLACDirectoryPath = configuration["SspdDirectoryPath"];
            statusFileDataAccess = _statuFileDataAccess;
            mapper = _mapper;
            _hubContext = hubContext;
        }

        public async Task<ResponseEntity<List<StatusFileDTO>>> ValidationSSPD(LacValidationDTO request, ResponseEntity<List<StatusFileDTO>> response)
        {            

            try
            {
                string inputFolder = _FilesLACDirectoryPath;
                var errorFlag = false;
                int eventCode = 0;
                int startDate =  1;
                int endDate =  2;
                int uia =  3;
                int eventContinues =  6;
                if (request.Encabezado == false)
                {
                    eventCode = request.columns.EventCode - 1;
                    startDate = request.columns.StartDate - 1;
                    endDate = request.columns.EndDate - 1;
                    uia = request.columns.Uia - 1;
                    eventContinues = request.columns.EventContinues - 1;
                }
                var statusFileList = new List<StatusFileDTO>();
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv")
                                    .Where(file => !file.EndsWith("_Correct.csv")
                                                  && !file.EndsWith("_Error.csv")
                                                  && !file.EndsWith("_unchanged.csv")
                                                  && !file.EndsWith("_continuesInsert.csv")
                                                  && !file.EndsWith("_continuesUpdate.csv")
                                                  && !file.EndsWith("_continuesInvalid.csv")
                                                  && !file.EndsWith("_closed.csv")
                                                  && !file.EndsWith("_closedInvalid.csv")
                                                  && !file.EndsWith("_delete.csv")
                                                  && !file.EndsWith("_update.csv"))
                                    .ToList().OrderBy(f => f)
                     .ToArray()
                    )
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

                    var dataTable = new DataTable();
                    var dataTableError = new DataTable();
                    int count = 1;
                    var columns = int.Parse(_configuration["Validations:SSPDColumns"]);                    
                    // columnas tabla error
                    dataTableError.Columns.Add("C1");
                    dataTableError.Columns.Add("C2");

                    var statusFilesingle = new StatusFileDTO();                    

                    string[] fileLines = File.ReadAllLines(filePath);
                    /// Asumiendo que el formato del archivo es AAAAMM_SSPD.csv

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "SSPD";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;
                    statusFilesingle.DateRegister = ParseDateTemp($"1/{month}/{year}");

                    // columnas tabla datos correctos
                    for (int i = 1; i <= columns; i++)
                    {
                        dataTable.Columns.Add($"C{i}");
                    }                                        
                    
                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
                        string message = string.Empty;
                        var beacon = 0;
                        for (int i = 0; i < columns; i++)
                        {
                            if (valueLines[i] != "")
                            {
                                beacon++;
                            }
                        }

                        if (beacon > 0)
                        {
                            if (valueLines.Length != columns)
                            {
                                message = "Error de cantidad de columnas llenas";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[uia] == "")
                            {
                                message = "Error en el código UIA, no puede ser nulo";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] == "")
                            {
                                message = "Error en el código de evento, no puede ser nulo";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && (valueLines[startDate] == "" && valueLines[endDate] == ""))
                            {
                                message = "Error de la data, no está llena correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] != "" && valueLines[endDate] != "" && valueLines[eventContinues] == "S")
                            {
                                message = "Error en la data, no puede estar en estado S cuando tiene fecha de terminación";
                                RegisterError(dataTableError, item, count, message);
                            }                            

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] == "" && valueLines[endDate] != "" && valueLines[eventContinues] == "S")
                            {
                                message = "Error de la fecha de terminación y/o estado en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            else if (valueLines[eventCode] != "NA" && valueLines[startDate] != "" && valueLines[endDate] == "" && valueLines[eventContinues] == "N")
                            {
                                message = "Error de la fecha de terminación y/o estado en la data, no están llenas correctamente";
                                RegisterError(dataTableError, item, count, message);
                            }

                            if (valueLines[eventCode] != "NA" && valueLines[eventCode] != "")
                            {
                                InsertData(dataTable, valueLines, columns);
                            }

                        }

                        count++;
                        beacon = 0;
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        createCSV(dataTable, filePath, columns);
                    }

                    statusFilesingle.Status = 1;

                    if (dataTableError.Rows.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("Receive", true, $"El archivo {fileName} tiene errores");
                        statusFilesingle.Status = 2;
                        errorFlag = true;
                        createCSVError(dataTableError, filePath);
                    }

                    var entityMap = mapper.Map<QueueStatusSspd>(statusFilesingle);
                    var resultSave = await statusFileDataAccess.UpdateDataSSPD(entityMap);

                    statusFileList.Add(statusFilesingle);

                }
                if (errorFlag)
                {
                    response.Message = "Archivo con errores";
                    response.SuccessData = false;
                    response.Success = false;
                    response.Data = statusFileList;
                    return response;
                }
                else
                {
                    response.Message = "All files are created";
                    response.SuccessData = true;
                    response.Success = true;
                    response.Data = statusFileList;
                    return response;
                }

            }

            catch (FormatException ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                response.Data = new List<StatusFileDTO>();
            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
                response.Data = new List<StatusFileDTO>();
            }

            return response;
        }

        private static void InsertData(DataTable dataTable, string[] valueLines, int columns)
        {
            var newRow = dataTable.NewRow();

            for (int i = 0; i < columns; i++)
            {
                newRow[i] = valueLines[i].ToUpper().Trim();
            }

            dataTable.Rows.Add(newRow);

        }

        private static void RegisterError(DataTable table, string item, int count, string message)
        {
            var messageError = $"{message} en la línea {count} del archivo cargado";

            var newRow = table.NewRow();

            newRow[0] = item;
            newRow[1] = messageError;

            table.Rows.Add(newRow);

        }

        private void createCSV(DataTable table, string filePath, int columns)
        {
            string inputFolder = _FilesLACDirectoryPath;
            string outputFilePath = Path.Combine(inputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_Correct.csv");
            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < columns; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
        }

        private void createCSVError(DataTable table, string filePath)
        {
            string inputFolder = _FilesLACDirectoryPath;
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

        private DateOnly ParseDateTemp(string dateString)
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
    }
}
