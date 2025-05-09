﻿using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text;

namespace ADO.BL.Services
{
    public class PolesEepServices : IPolesEepServices
    {
        private readonly IConfiguration _configuration;        
        private readonly string _PolesDirectoryPath;
        private readonly IPolesEepDataAccess polesEepDataAccess;
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IMapper mapper;
        private readonly string _connectionString;
        public PolesEepServices(IConfiguration configuration,
            IPolesEepDataAccess _polesEepDataAccess,
            IStatusFileDataAccess _statuFileDataAccess,
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            polesEepDataAccess = _polesEepDataAccess;
            _configuration = configuration;            
            _PolesDirectoryPath = configuration["PolesPath"];
            mapper = _mapper;
            statusFileDataAccess = _statuFileDataAccess;
        }

        public async Task<ResponseQuery<bool>> ValidationFile(PolesValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _PolesDirectoryPath;
                
                //Procesar cada archivo.xlsx en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    var listAssetsDTO = new List<AssetsDTO>();
                    var listUtilityPoleDTO = new List<MpUtilityPoleDTO>();
                    var listEntityPoleDTO = new List<MpUtilityPoleDTO>();                    
                    
                    var statusFileList = new List<StatusFileDTO>();
                    var statusFilesingle = new StatusFileDTO();

                    // Extraer el nombre del archivo sin la extensión
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    // Obtener los primeros 4 dígitos como el año
                    int year = int.Parse(fileName.Substring(0, 4));

                    // Obtener los siguientes 2 dígitos como el mes
                    int month = int.Parse(fileName.Substring(4, 2));

                    statusFilesingle.DateFile = DateOnly.FromDateTime(DateTime.Now);
                    statusFilesingle.UserId = request.UserId;
                    statusFilesingle.FileName = fileName;
                    statusFilesingle.FileType = "POLES";
                    statusFilesingle.Year = year;
                    statusFilesingle.Month = month;
                    statusFilesingle.Day = 1;
                    statusFilesingle.Status = 4;

                    statusFileList.Add(statusFilesingle);

                    string[] fileLines = File.ReadAllLines(filePath);
                    var listDataString = new StringBuilder();
                    var listFparent = new List<string>();                    
                    foreach (var item in fileLines)
                    {
                        var valueLinesTemp = item.Split(',',';');
                        if (valueLinesTemp[0] != "NODO_FISICO")
                        {
                            if (!listFparent.Contains(valueLinesTemp[1]))
                            {
                                listDataString.Append($"'{valueLinesTemp[1]}',");
                                listFparent.Add(valueLinesTemp[1]);
                            }
                        }
                    }

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        var listDef = listDataString.ToString().Remove(listDataString.Length - 1, 1);                                                

                        var SelectQueryUtility = $@"SELECT painting_code, fparent from maps.mp_utility_pole where fparent in ({listDef})";
                        using (var reader = new NpgsqlCommand(SelectQueryUtility, connection))
                        {
                            try
                            {
                                using (var result = reader.ExecuteReader())
                                {
                                    while (result.Read())
                                    {
                                        var temp = new MpUtilityPoleDTO();                                        
                                        temp.PaintingCode = result[0].ToString();
                                        temp.Fparent = result[1].ToString();                                        
                                        listUtilityPoleDTO.Add(temp);
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

                    foreach (var item in fileLines)
                    {
                        var valueLines = item.Split(';', ',');
                        if (string.IsNullOrEmpty(valueLines[0]) || string.IsNullOrEmpty(valueLines[1]) || 
                            string.IsNullOrEmpty(valueLines[2]) || string.IsNullOrEmpty(valueLines[3]) ||
                            string.IsNullOrEmpty(valueLines[4]))
                        {
                            continue;
                        }
                        if (valueLines[0] == "NODO_FISICO")
                        {
                            continue;
                        }

                        var assetTemp = listAssetsDTO.FirstOrDefault(x => x.Fparent == valueLines[1]);
                        var poleTemp = listUtilityPoleDTO.FirstOrDefault(x => x.PaintingCode == valueLines[0]);

                        if (assetTemp == null)
                        {
                            continue;
                        }

                        if (poleTemp == null)
                        {                            
                            var entityPole = new MpUtilityPoleDTO();

                            entityPole.InventaryCode = valueLines[1].Trim();
                            entityPole.PaintingCode = valueLines[2].Trim();
                            entityPole.Latitude = float.Parse(valueLines[3].ToString());
                            entityPole.Longitude = float.Parse(valueLines[4].ToString());
                            entityPole.Fparent = valueLines[8].Trim();                            
                            entityPole.TypePole = int.Parse(valueLines[11].ToString());

                            listEntityPoleDTO.Add(entityPole);
                        }
                        else if(poleTemp.Fparent != valueLines[1].Trim())
                        {
                            var entityPole = new MpUtilityPoleDTO();

                            entityPole.InventaryCode = valueLines[1].Trim();
                            entityPole.PaintingCode = valueLines[2].Trim();
                            entityPole.Latitude = float.Parse(valueLines[3].ToString());
                            entityPole.Longitude = float.Parse(valueLines[4].ToString());
                            entityPole.Fparent = valueLines[8].Trim();                            
                            entityPole.TypePole = int.Parse(valueLines[11].ToString());

                            listEntityPoleDTO.Add(entityPole);
                        }
                        
                    }

                    if(listEntityPoleDTO.Count > 0)
                    {
                        
                        var polesMapped = mapper.Map<List<MpUtilityPole>>(listEntityPoleDTO);
                        var respCreate = CreateData(polesMapped);

                        var entityMap = mapper.Map<QueueStatusPole>(statusFilesingle);
                        var resultSave = await statusFileDataAccess.UpdateDataPole(entityMap);
                    }
                }

                response.Message = "File uploaded successfully";
                response.Success = true;
                response.SuccessData = true;
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

        

        // acciones en bd y mappeo

        public async Task<Boolean> CreateData(List<MpUtilityPole> request)
        {            
            await polesEepDataAccess.CreateFile(request);
            return true;

        }

    }
}
