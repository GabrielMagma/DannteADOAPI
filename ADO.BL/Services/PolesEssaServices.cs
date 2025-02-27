using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

namespace ADO.BL.Services
{
    public class PolesEssaServices : IPolesEssaServices
    {
        private readonly IConfiguration _configuration;        
        private readonly string _PolesDirectoryPath;
        private readonly IPolesEssaDataAccess polesEssaDataAccess;
        private readonly IMapper mapper;
        public PolesEssaServices(IConfiguration configuration,
            IPolesEssaDataAccess _polesEssaDataAccess,
            IMapper _mapper)
        {
            polesEssaDataAccess = _polesEssaDataAccess;
            _configuration = configuration;            
            _PolesDirectoryPath = configuration["PolesPath"];
            mapper = _mapper;
        }

        public async Task<ResponseQuery<bool>> ValidationFile(ResponseQuery<bool> response)
        {
            try
            {
                var inputFolder = _PolesDirectoryPath;

                //Procesar cada archivo.csv en la carpeta
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    var listAssetsDTO = new List<AssetsDTO>();
                    var listUtilityPoleDTO = new List<MpUtilityPoleDTO>();
                    var listEntityPoleDTO = new List<MpUtilityPoleDTO>();
                    var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteEssaTesting";

                    string[] fileLines = File.ReadAllLines(filePath);
                    var listDataString = new StringBuilder();
                    var listFparent = new List<string>();                    
                    foreach (var item in fileLines)
                    {
                        var valueLinesTemp = item.Split(',',';');
                        if (valueLinesTemp[0] != "CODIGOINTERRUPTORDIS")
                        {
                            var fparentTemp = valueLinesTemp[5].Trim().Replace(" ", "");
                            if (!listFparent.Contains(fparentTemp))
                            {
                                if (fparentTemp.Length > 1) {
                                    listDataString.Append($"'{fparentTemp}',");
                                    listFparent.Add(fparentTemp);
                                }
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
                        if (string.IsNullOrEmpty(valueLines[5]) || string.IsNullOrEmpty(valueLines[14]) ||
                            string.IsNullOrEmpty(valueLines[18]) || string.IsNullOrEmpty(valueLines[19]))
                        {
                            continue;
                        }
                        else
                        { 
                            if (valueLines[0] != "CODIGOINTERRUPTORDIS")
                            {
                                var assetTemp = listAssetsDTO.FirstOrDefault(x => x.Fparent == valueLines[5].Trim().Replace(" ", ""));
                                var poleTemp = listUtilityPoleDTO.FirstOrDefault(x => x.PaintingCode == valueLines[14]);

                                if (poleTemp == null && assetTemp != null)
                                {

                                    var entityPole = new MpUtilityPoleDTO();

                                    entityPole.InventaryCode = valueLines[14].Trim();
                                    entityPole.PaintingCode = valueLines[14].Trim();
                                    entityPole.Latitude = float.Parse(valueLines[18].ToString());
                                    entityPole.Longitude = float.Parse(valueLines[19].ToString());
                                    entityPole.Fparent = valueLines[5].Trim().Replace(" ", "");
                                    entityPole.IdRegion = assetTemp.IdRegion;
                                    entityPole.NameRegion = assetTemp.NameRegion.Trim().ToUpper();
                                    entityPole.TypePole = 1;

                                    listEntityPoleDTO.Add(entityPole);
                                }
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
            await polesEssaDataAccess.CreateFile(request);
            return true;

        }

    }
}
