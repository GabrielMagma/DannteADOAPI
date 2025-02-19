using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class PolesEepServices : IPolesEepServices
    {
        private readonly IConfiguration _configuration;        
        private readonly string _PolesDirectoryPath;
        private readonly IPolesEepDataAccess polesEepDataAccess;
        private readonly IMapper mapper;
        public PolesEepServices(IConfiguration configuration,
            IPolesEepDataAccess _polesEepDataAccess,
            IMapper _mapper)
        {
            polesEepDataAccess = _polesEepDataAccess;
            _configuration = configuration;            
            _PolesDirectoryPath = configuration["PolesPath"];
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ValidationFile(ResponseQuery<bool> response)
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
                    var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteEepTesting";

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
                        var SelectQueryAssets = $@"SELECT distinct fparent, name_region, id_region from public.all_asset where fparent in ({listDef})";
                        using (var reader = new NpgsqlCommand(SelectQueryAssets, connection))
                        {
                            try
                            {

                                using (var result = reader.ExecuteReader())
                                {
                                    while (result.Read())
                                    {
                                        var temp = new AssetsDTO();
                                        temp.Fparent = result[0].ToString();
                                        temp.NameRegion = result[1].ToString();
                                        temp.IdRegion = long.Parse(result[2].ToString());
                                        listAssetsDTO.Add(temp);
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
                        if (valueLines[0] != "NODO_FISICO")
                        {
                            var assetTemp = listAssetsDTO.FirstOrDefault(x => x.Fparent == valueLines[1]);
                            var poleTemp = listUtilityPoleDTO.FirstOrDefault(x => x.PaintingCode == valueLines[0]);

                            if (poleTemp == null && assetTemp != null)
                            {
                            
                                    var entityPole = new MpUtilityPoleDTO();

                                    entityPole.InventaryCode = valueLines[0];
                                    entityPole.PaintingCode = valueLines[0];
                                    entityPole.Latitude = float.Parse(valueLines[3].ToString());
                                    entityPole.Longitude = float.Parse(valueLines[4].ToString());
                                    entityPole.Fparent = valueLines[1];
                                    entityPole.IdRegion = assetTemp.IdRegion;
                                    entityPole.NameRegion = assetTemp.NameRegion;
                                    entityPole.TypePole = int.Parse(valueLines[2].ToString());

                                    listEntityPoleDTO.Add(entityPole);
                            }
                        }                                                   
                    }

                    if(listEntityPoleDTO.Count > 0)
                    {
                        
                        var polesMapped = mapper.Map<List<MpUtilityPole>>(listEntityPoleDTO);
                        //var respCreate = CreateData(polesMapped);
                    }
                }

                response.Message = "File uploaded correctly";
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
