using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class FileAssetServices : IFileAssetServices
    {
        private readonly IFileAssetDataAccess fileDataAccess;
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        public FileAssetServices(IConfiguration configuration, IFileAssetDataAccess _fileDataAccess, IMapper _mapper)
        {
            fileDataAccess = _fileDataAccess;
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
        }

        public async Task<ResponseQuery<string>> CreateFile(string name, ResponseQuery<string> response)
        {
            try
            {   

                List<AllAsset> AssetsListData = new List<AllAsset>();
                var keys = new List<string>();
                string[] fileLines = File.ReadAllLines($"C:\\Users\\ingen\\source\\repos\\DannteADOAPI\\files\\ASSETS\\{name}.csv");
                var listDataString = new StringBuilder();
                var listUIA = new StringBuilder();
                var allAssetList = new List<AssetDTO>();
                foreach (var item in fileLines)
                {
                    var valueLinesTemp = item.Split(',',';');
                    
                    listDataString.Append($"'{valueLinesTemp[2]}',");
                    
                }

                var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteDevelopment";

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

                foreach (var item in fileLines)
                {
                    AllAssetDTO asset = new AllAssetDTO();
                    var valueLines = item.Split(',',';');
                    var key = $"{valueLines[1]}-{valueLines[2]}";
                    var AssetUnit = new AssetDTO
                    {
                        Uia = valueLines[2],
                        Code_sig = valueLines[1]

                    };
                    if (valueLines[10] != null)
                    {
                        if (!(keys.Contains(key)))
                        {
                            var existAsset = allAssetList.FirstOrDefault(x => x.Uia == AssetUnit.Uia && x.Code_sig == AssetUnit.Code_sig);
                            if (existAsset == null) 
                            {
                                asset.Id = 0;
                                asset.TypeAsset = valueLines[0];
                                asset.CodeSig = valueLines[1];
                                asset.Uia = valueLines[2];
                                asset.Codetaxo = valueLines[3];
                                asset.Fparent = valueLines[4];
                                asset.Latitude = float.Parse(valueLines[5]);
                                asset.Longitude = float.Parse(valueLines[6]);
                                asset.Poblation = valueLines[7];
                                asset.Group015 = valueLines[8];
                                asset.Uccap14 = valueLines[9];
                                //asset.DateInst = string.IsNullOrEmpty(valueLines[10]) ? (DateTime?)null : ParseDate(valueLines[10]);
                                //asset.DateUnin = string.IsNullOrEmpty(valueLines[11]) ? new DateTime(2099, 12, 31) : ParseDate(valueLines[11]);
                                asset.DateInst = null;
                                asset.DateUnin = null;
                                asset.State = string.IsNullOrEmpty(valueLines[12]) ? 2 : int.Parse(valueLines[12]);
                                asset.IdZone = string.IsNullOrEmpty(valueLines[13]) ? (long?)null : long.Parse(valueLines[13]);
                                asset.NameZone = valueLines[14];
                                asset.IdRegion = string.IsNullOrEmpty(valueLines[15]) ? (long?)null : long.Parse(valueLines[15]);
                                asset.NameRegion = valueLines[16];
                                asset.IdLocality = string.IsNullOrEmpty(valueLines[17]) ? (long?)null : long.Parse(valueLines[17]);
                                asset.NameLocality = valueLines[18];
                                asset.IdSector = string.IsNullOrEmpty(valueLines[19]) ? (long?)null : long.Parse(valueLines[19]);
                                asset.NameSector = valueLines[20];
                                asset.GeographicalCode = string.IsNullOrEmpty(valueLines[21]) ? (long?)null : long.Parse(valueLines[21]);
                                asset.Address = valueLines[22];


                                keys.Add(key);

                                var AssetsMapped = mapper.Map<AllAsset>(asset);
                                AssetsListData.Add(AssetsMapped);
                            }
                        }
                    }
                                                            
                }
                        
                response.SuccessData = await fileDataAccess.CreateFile(AssetsListData);                 
                response.Message = "File created on the project root ./files";
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

        private DateTime ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateTime.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToUniversalTime();
                }
            }
            return DateTime.Parse("31/12/2099");
        }

    }
}
