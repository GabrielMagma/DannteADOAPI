﻿using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ADO.BL.Services
{
    public class FileAssetServices : IFileAssetServices
    {
        private readonly IFileAssetDataAccess fileDataAccess;
        private readonly IMapper mapper;
        private readonly string[] _timeFormats;
        private readonly string _AssetsDirectoryPath;
        public FileAssetServices(IConfiguration configuration, IFileAssetDataAccess _fileDataAccess, IMapper _mapper)
        {
            fileDataAccess = _fileDataAccess;
            mapper = _mapper;
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _AssetsDirectoryPath = configuration["AssetsDirectoryPath"];
        }

        public async Task<ResponseQuery<string>> CreateFile(ResponseQuery<string> response)
        {
            try
            {
                string inputFolder = _AssetsDirectoryPath;
                foreach (var filePath in Directory.GetFiles(inputFolder, "*.csv"))
                {
                    string[] fileLines = File.ReadAllLines(filePath);
                    List<AllAsset> AssetsListData = new List<AllAsset>();
                    var keys = new List<string>();                    
                    var listUpdateDataString = new StringBuilder();
                    var listUIA = new StringBuilder();
                    var allAssetList = new List<AssetDTO>();                    

                    var _connectionString = "Host=89.117.149.219;Port=5432;Username=postgres;Password=DannteEssa2024;Database=DannteEssaTesting";

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();                        
                        var SelectQuery = $@"SELECT uia, code_sig FROM public.all_asset";
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
                        var valueLines = item.Split(',', ';');
                        var key = $"{valueLines[1]}-{valueLines[2]}";
                        var AssetUnit = new AssetDTO
                        {
                            Uia = valueLines[2],
                            Code_sig = valueLines[1]

                        };
                        if (false)
                        {
                            continue;
                        }
                        if (valueLines[10] != null)
                        {
                            if (!(keys.Contains(key)))
                            {
                                var existAsset = allAssetList.FirstOrDefault(x => x.Uia == AssetUnit.Uia && x.Code_sig == AssetUnit.Code_sig);
                                if (existAsset == null)
                                {
                                    var exist2Asset = allAssetList.FirstOrDefault(x => x.Code_sig == AssetUnit.Code_sig);

                                    if (exist2Asset != null)
                                    {                                         
                                       listUpdateDataString.Append($"'{exist2Asset.Code_sig}',");                                        
                                    }

                                    asset.Id = 0;
                                    asset.TypeAsset = !string.IsNullOrEmpty(valueLines[0]) ? valueLines[0].Trim().ToUpper() : "-1";
                                    asset.CodeSig = !string.IsNullOrEmpty(valueLines[1]) ? valueLines[1].Trim() : "-1";
                                    asset.Uia = !string.IsNullOrEmpty(valueLines[2]) ? valueLines[2].Trim() : "-1";
                                    asset.Codetaxo = !string.IsNullOrEmpty(valueLines[3]) ? valueLines[3].Trim() : "-1";
                                    asset.Fparent = !string.IsNullOrEmpty(valueLines[4]) ? valueLines[4].Trim().Replace(" ", "") : "-1";
                                    asset.Latitude = !string.IsNullOrEmpty(valueLines[5]) ? float.Parse(valueLines[5]) : -2;
                                    asset.Longitude = !string.IsNullOrEmpty(valueLines[6]) ? float.Parse(valueLines[6]) : -2;
                                    asset.Poblation = !string.IsNullOrEmpty(valueLines[7]) ? valueLines[7].Trim() : "-1";
                                    asset.Group015 = !string.IsNullOrEmpty(valueLines[8]) ? valueLines[8].Trim() : "-1";
                                    asset.Uccap14 = !string.IsNullOrEmpty(valueLines[9]) ? valueLines[9].Trim() : "-1";
                                    asset.DateInst = string.IsNullOrEmpty(valueLines[10]) ? new DateOnly(2099, 12, 31) : ParseDate(valueLines[10]);
                                    asset.DateUnin = string.IsNullOrEmpty(valueLines[11]) ? new DateOnly(2099, 12, 31) : ParseDate(valueLines[11]);
                                    asset.State = string.IsNullOrEmpty(valueLines[12]) ? 2 : int.Parse(valueLines[12]);
                                    asset.IdZone = string.IsNullOrEmpty(valueLines[13]) ? -2 : long.Parse(valueLines[13]);
                                    asset.NameZone = !string.IsNullOrEmpty(valueLines[14]) ? valueLines[14].Trim().ToUpper() : "-1";
                                    asset.IdRegion = string.IsNullOrEmpty(valueLines[15]) ? -2 : long.Parse(valueLines[15]);
                                    asset.NameRegion = string.IsNullOrEmpty(valueLines[16]) ? "-1" : valueLines[16].Trim().ToUpper();
                                    asset.IdLocality = string.IsNullOrEmpty(valueLines[17]) ? -2 : long.Parse(valueLines[17]);
                                    asset.NameLocality = string.IsNullOrEmpty(valueLines[18]) ? "-1" : valueLines[18].Trim().ToUpper();
                                    asset.IdSector = string.IsNullOrEmpty(valueLines[19]) ? -2 : long.Parse(valueLines[19]);
                                    asset.NameSector = string.IsNullOrEmpty(valueLines[20]) ? "-1" : valueLines[20].Trim().ToUpper();
                                    asset.GeographicalCode = string.IsNullOrEmpty(valueLines[21]) ? -2 : long.Parse(valueLines[21]);
                                    asset.Address = string.IsNullOrEmpty(valueLines[22]) ? "-1" : valueLines[22].Trim().ToUpper();


                                    keys.Add(key);

                                    var AssetsMapped = mapper.Map<AllAsset>(asset);
                                    AssetsListData.Add(AssetsMapped);
                                    
                                }
                            }
                        }

                    }

                    if (listUpdateDataString.Length > 1)
                    {                        
                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();
                            var listDefUpdate = listUpdateDataString.ToString().Remove(listUpdateDataString.Length - 1, 1);
                            string upsertQuery = $@"
                                UPDATE public.all_asset SET state = 3 WHERE code_sig IN ({listDefUpdate});
                            ";

                            using (var upsertCmd = new NpgsqlCommand(upsertQuery, connection))
                            {
                                upsertCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    response.SuccessData = await fileDataAccess.CreateFile(AssetsListData);
                    response.Message = "File created on the project root ./files";
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

        private DateOnly ParseDate(string dateString)
        {
            foreach (var format in _timeFormats)
            {
                if (DateOnly.TryParseExact(dateString, format.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateOnly.Parse("31/12/2099");
        }

    }
}
