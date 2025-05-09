﻿using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Globalization;

namespace ADO.BL.Services
{
    public class AllAssetOracleServices : IAllAssetOracleServices
    {
        private readonly IAllAssetOracleDataAccess allAssetOracleDataAccess;
        private readonly IMapper mapper;
        private readonly string _connectionString;
        private readonly string _AssetsDirectoryPath;
        private readonly string[] _timeFormats;

        private static readonly CultureInfo _spanishCulture = new CultureInfo("es-CO"); // o "es-ES"

        public AllAssetOracleServices(IConfiguration configuration, 
            IAllAssetOracleDataAccess _AllAssetOracleDataAccess, 
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            allAssetOracleDataAccess = _AllAssetOracleDataAccess;
            _AssetsDirectoryPath = configuration["FilesAssetsPath"];
            mapper = _mapper;
        }

        public async Task<ResponseEntity<List<AllAssetDTO>>> SearchData(ResponseEntity<List<AllAssetDTO>> response)
        {
            try
            {
                var assetList = new List<AllAssetDTO>();
                var listAssetNewMap = await GetListAllAssetNews(assetList);
                var responseCreate = false;
                var responseUpdate = false;
                var dateToday = DateOnly.FromDateTime(DateTime.Now);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage("info");                
                ExcelWorksheet workSheet1 = excel.Workbook.Worksheets.Add("Page 1");
                var beginRow = 3;

                var listTypeAssets = new List<string>()
                {
                    "TRANSFORMADOR",
                    "RECONECTADOR",
                    "INTERRUPTOR"

                };

                foreach (var itemList in listTypeAssets)
                {

                    //oracle conection
                    var dataOracle = await OracleConection(itemList);

                    int i = 0;
                    while ((i * 10000) < dataOracle.Count())
                    {
                        var subgroup = dataOracle.Skip(i * 10000).Take(10000);                                                
                        
                        var assetExistUnit = new AllAssetDTO();

                        foreach (var item in subgroup)
                        {
                            var ListAssetExist = subgroup.FirstOrDefault(x => x.CodeSig == item.CodeSig && x.Uia == item.Uia);

                            assetExistUnit = listAssetNewMap.FirstOrDefault(x => x.CodeSig == ListAssetExist.CodeSig && x.Uia == ListAssetExist.Uia);

                            if (assetExistUnit == null)
                            {                                

                                workSheet1.Cells[beginRow, 1].Value = "TRANSFORMADOR";
                                if (itemList == "RECONECTADOR")
                                {
                                    workSheet1.Cells[beginRow, 1].Value = "RECONECTADOR";
                                }
                                else if (itemList == "INTERRUPTOR")
                                {
                                    workSheet1.Cells[beginRow, 1].Value = "SECCIONADOR";
                                }

                                workSheet1.Cells[beginRow, 2].Value = ListAssetExist.CodeSig;
                                workSheet1.Cells[beginRow, 3].Value = ListAssetExist.Uia;
                                workSheet1.Cells[beginRow, 4].Value = ListAssetExist.Codetaxo;
                                workSheet1.Cells[beginRow, 5].Value = ListAssetExist.Fparent;
                                workSheet1.Cells[beginRow, 6].Value = ListAssetExist.Latitude;
                                workSheet1.Cells[beginRow, 7].Value = ListAssetExist.Longitude;
                                workSheet1.Cells[beginRow, 8].Value = ListAssetExist.Poblation;
                                workSheet1.Cells[beginRow, 9].Value = ListAssetExist.Group015;
                                workSheet1.Cells[beginRow, 10].Value = ListAssetExist.Address;                                
                                workSheet1.Cells[beginRow, 11].Value = ParseDate(ListAssetExist.DateInst.ToString()).ToString();
                                workSheet1.Cells[beginRow, 12].Value = ListAssetExist.Uccap14;
                            }
                            else if (assetExistUnit.State != ListAssetExist.State)
                            {

                                workSheet1.Cells[beginRow, 1].Value = "TRANSFORMADOR";
                                if (itemList == "RECONECTADOR")
                                {
                                    workSheet1.Cells[beginRow, 1].Value = "RECONECTADOR";
                                }
                                else if (itemList == "INTERRUPTOR")
                                {
                                    workSheet1.Cells[beginRow, 1].Value = "SECCIONADOR";
                                }
                                
                                workSheet1.Cells[beginRow, 2].Value = ListAssetExist.CodeSig;
                                workSheet1.Cells[beginRow, 3].Value = ListAssetExist.Uia;
                                workSheet1.Cells[beginRow, 4].Value = ListAssetExist.Codetaxo;
                                workSheet1.Cells[beginRow, 5].Value = ListAssetExist.Fparent;
                                workSheet1.Cells[beginRow, 6].Value = ListAssetExist.Latitude;
                                workSheet1.Cells[beginRow, 7].Value = ListAssetExist.Longitude;
                                workSheet1.Cells[beginRow, 8].Value = ListAssetExist.Poblation;
                                workSheet1.Cells[beginRow, 9].Value = ListAssetExist.Group015;
                                workSheet1.Cells[beginRow, 10].Value = ListAssetExist.Address;                                
                                workSheet1.Cells[beginRow, 11].Value = ParseDate(ListAssetExist.DateInst.ToString()).ToString();
                                workSheet1.Cells[beginRow, 12].Value = ListAssetExist.Uccap14;
                                
                            }

                            beginRow++;
                        }
                        
                        i++;
                    }

                }

                //crea mapa de bytes
                //excel.GetAsByteArray();
                var monthDateToday = DateTime.Now.ToString("MM");
                var yearDateToday = DateTime.Now.Year;
                var nameFile = $"{yearDateToday}{monthDateToday}_ASSET.xlsx";
                var rutaCompleta = Path.Combine(_AssetsDirectoryPath, nameFile);

                if (File.Exists(rutaCompleta))
                {
                    File.Delete(rutaCompleta);
                }

                excel.SaveAs(rutaCompleta);

                //cierra librería
                excel.Dispose();

                Console.WriteLine("Proceso completado.");

                response.Message = "All Registers are created and/or updated";
                response.SuccessData = true;
                response.Success = true;
                return response;

            }
            catch (OracleException ex)
            {
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
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

        private async Task<List<AllAssetDTO>> OracleConection(string table)
        {

            //string localConnectionString = "Data Source=ndzh4dbucqnindx4_tpurgent ;User ID=ADMIN;Password=MagmaDannte2024!;Connection Timeout=120;Tns_Admin=C:\\Users\\ingen\\source\\repos\\wallet;Wallet_Location=C:\\Users\\ingen\\source\\repos\\wallet;";
            string localConnectionString = "Data Source=172.25.3.201:1521/SPARD;User Id=CMANIOBRAS_OMS;Password=eep2022";

            using (OracleConnection localConnection = new OracleConnection(localConnectionString))
            {
                List<AllAssetDTO> allAssets = new List<AllAssetDTO>();
                try
                {
                    localConnection.Open();
                    Console.WriteLine("Conexión abierta con éxito.");


                    string query = $"SELECT * FROM SPARD.TRANSFOR";
                    if (table == "RECONECTADOR")
                    {
                        query = $"SELECT * FROM SPARD.RECLOSER";
                    }
                    else if (table == "INTERRUPTOR") {
                        query = $"SELECT * FROM SPARD.SWITCHES";
                    }
                    using (OracleCommand command = new OracleCommand(query, localConnection))
                    {
                        // Ejecutar el comando y leer los datos
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // catch error 
                                
                                if (table == "RECONECTADOR")
                                {
                                    if (!reader.IsDBNull(0) && !reader.IsDBNull(71) && !reader.IsDBNull(12))
                                    {
                                        // Procesar cada fila (cambia el índice según el tipo de dato)
                                        AllAssetDTO allAsset = new AllAssetDTO();

                                        allAsset.CodeSig = reader.GetString(0);
                                        Console.WriteLine(reader.GetString(0));
                                        allAsset.Uia = reader.GetString(71);
                                        allAsset.Codetaxo = !reader.IsDBNull(68) ? reader.GetString(68) : "-1";
                                        allAsset.Fparent = !reader.IsDBNull(12) ? reader.GetString(12) : "-1";
                                        allAsset.Uccap14 = !reader.IsDBNull(77) ? reader.GetString(77) : "-1";
                                        allAsset.Group015 = "-1";
                                        allAsset.Latitude = !reader.IsDBNull(55) ? float.Parse(reader.GetString(55)) : 0;
                                        allAsset.Longitude = !reader.IsDBNull(56) ? float.Parse(reader.GetString(56)) : 0;
                                        allAsset.DateInst = !reader.IsDBNull(66) ? DateOnly.Parse(reader.GetString(66)) : null;
                                        allAsset.Poblation = !reader.IsDBNull(70) ? reader.GetString(70) : "-1";
                                        allAsset.Address = !reader.IsDBNull(2) ? reader.GetString(2) : "-1";
                                        allAsset.DateUnin = DateOnly.Parse("2099-12-31");
                                        allAsset.State = 2;
                                        allAsset.IdRegion = 1;
                                        allAsset.NameRegion = "GENERAL";                                        
                                        allAssets.Add(allAsset);
                                    }
                                }
                                else if (table == "INTERRUPTOR")
                                {
                                    if (!reader.IsDBNull(0) && !reader.IsDBNull(49) && !reader.IsDBNull(14))
                                    {
                                        // Procesar cada fila (cambia el índice según el tipo de dato)
                                        AllAssetDTO allAsset = new AllAssetDTO();

                                        allAsset.CodeSig = reader.GetString(0);
                                        Console.WriteLine(reader.GetString(0));
                                        allAsset.Uia = reader.GetString(49);
                                        allAsset.Codetaxo = !reader.IsDBNull(46) ? reader.GetString(46) : "-1";
                                        allAsset.Fparent = !reader.IsDBNull(14) ? reader.GetString(14) : "-1";
                                        allAsset.Uccap14 = !reader.IsDBNull(55) ? reader.GetString(55) : "-1";
                                        allAsset.Group015 = "-1";
                                        allAsset.Latitude = !reader.IsDBNull(29) ? float.Parse(reader.GetString(29)) : 0;
                                        allAsset.Longitude = !reader.IsDBNull(30) ? float.Parse(reader.GetString(30)) : 0;
                                        allAsset.DateInst = !reader.IsDBNull(44) ? DateOnly.Parse(reader.GetString(44)) : null;
                                        allAsset.Poblation = !reader.IsDBNull(48) ? reader.GetString(48) : "-1";
                                        allAsset.Address = !reader.IsDBNull(2) ? reader.GetString(2) : "-1";
                                        allAsset.TypeAsset = "SECCIONADOR";
                                        allAsset.DateUnin = DateOnly.Parse("2099-12-31");
                                        allAsset.State = 2;
                                        allAsset.IdRegion = 1;
                                        allAsset.NameRegion = "GENERAL";                                        
                                        allAssets.Add(allAsset);
                                    }
                                }
                                else
                                {
                                    if (!reader.IsDBNull(0) && !reader.IsDBNull(100) && !reader.IsDBNull(14))
                                    {
                                        // Procesar cada fila (cambia el índice según el tipo de dato)
                                        AllAssetDTO allAsset = new AllAssetDTO();

                                        allAsset.CodeSig = reader.GetString(0);
                                        Console.WriteLine(reader.GetString(0));
                                        allAsset.Uia = reader.GetString(100);
                                        allAsset.Codetaxo = !reader.IsDBNull(96) ? reader.GetString(96) : "-1";
                                        allAsset.Fparent = !reader.IsDBNull(14) ? reader.GetString(14) : "-1";
                                        allAsset.Uccap14 = !reader.IsDBNull(107) ? reader.GetString(107) : "-1";
                                        allAsset.Group015 = !reader.IsDBNull(97) ? reader.GetString(97) : "-1";
                                        allAsset.Latitude = !reader.IsDBNull(53) ? float.Parse(reader.GetString(53)) : 0;
                                        allAsset.Longitude = !reader.IsDBNull(55) ? float.Parse(reader.GetString(55)) : 0;
                                        allAsset.DateInst = !reader.IsDBNull(46) ? DateOnly.Parse(reader.GetString(46)) : null;
                                        allAsset.Poblation = !reader.IsDBNull(44) ? reader.GetString(44) : "-1";
                                        allAsset.Address = !reader.IsDBNull(2) ? reader.GetString(2) : "-1";
                                        allAsset.TypeAsset = "TRANSFORMADOR";
                                        allAsset.DateUnin = DateOnly.Parse("2099-12-31");
                                        allAsset.State = 2;
                                        allAsset.IdRegion = 1;
                                        allAsset.NameRegion = "GENERAL";                                        
                                        allAssets.Add(allAsset);
                                    }
                                }

                            }
                        }

                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Error en la conexión: {ex.Message}");
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    // Cerrar conexión
                    if (localConnection.State == System.Data.ConnectionState.Open)
                    {
                        localConnection.Close();
                        Console.WriteLine("Conexión cerrada.");
                    }
                }
                return allAssets.Count > 0 ? allAssets : new List<AllAssetDTO>();
            }
        }

        public async Task<Boolean> SaveData(List<AllAsset> request)
        {

            await allAssetOracleDataAccess.SaveData(request);            
            return true;
        }

        public async Task<Boolean> UpdateData(List<AllAssetDTO> request)
        {

            await allAssetOracleDataAccess.UpdateData(request);
            return true;
        }

        public async Task<List<AllAssetDTO>> GetListAllAssetNews(List<AllAssetDTO> assetList)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var SelectQuery = $@"SELECT code_sig, uia, state FROM public.all_asset";
                using (var reader = new NpgsqlCommand(SelectQuery, connection))
                {
                    try
                    {

                        using (var result = await reader.ExecuteReaderAsync())
                        {
                            while (await result.ReadAsync())
                            {                                
                                var temp = new AllAssetDTO();                                
                                temp.CodeSig = result[0].ToString();
                                temp.Uia = result[1].ToString();
                                temp.State = !string.IsNullOrEmpty(result[2].ToString()) ? int.Parse(result[2].ToString()) : -1;

                                assetList.Add(temp);
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

            return assetList;
        }

        public static List<AllAssetDTO> MapperList(List<AllAsset> request)
        {
            var newListAsset = new List<AllAssetDTO>();


            foreach (var item in request)
            {
                var newAsset = new AllAssetDTO();

                newAsset.Id = item.Id;
                newAsset.TypeAsset = item.TypeAsset;
                newAsset.CodeSig = item.CodeSig;
                newAsset.Uia = item.Uia;
                newAsset.Codetaxo = item.Codetaxo;
                newAsset.Fparent = item.Fparent;
                newAsset.Latitude = item.Latitude;
                newAsset.Longitude = item.Longitude;
                newAsset.Poblation = item.Poblation;
                newAsset.Group015 = item.Group015;
                newAsset.DateInst = item.DateInst;
                newAsset.DateUnin = item.DateUnin;
                newAsset.State = item.State;
                newAsset.Uccap14 = item.Uccap14;                
                newAsset.IdRegion = item.IdRegion;
                newAsset.NameRegion = item.NameRegion;                
                newAsset.Address = item.Address;                

                newListAsset.Add(newAsset);
            }

            return newListAsset;
        }

        public static List<AllAsset> MapperListReverse(List<AllAssetDTO> request)
        {
            var newListAsset = new List<AllAsset>();

            foreach (var item in request)
            {
                var newAsset = new AllAsset();

                newAsset.Id = item.Id;
                newAsset.TypeAsset = item.TypeAsset;
                newAsset.CodeSig = item.CodeSig;
                newAsset.Uia = item.Uia;
                newAsset.Codetaxo = item.Codetaxo;
                newAsset.Fparent = item.Fparent;
                newAsset.Latitude = item.Latitude;
                newAsset.Longitude = item.Longitude;
                newAsset.Poblation = item.Poblation;
                newAsset.Group015 = item.Group015;
                newAsset.DateInst = item.DateInst;
                newAsset.DateUnin = item.DateUnin;
                newAsset.State = item.State;
                newAsset.Uccap14 = item.Uccap14;                
                newAsset.IdRegion = item.IdRegion;
                newAsset.NameRegion = item.NameRegion;                
                newAsset.Address = item.Address;                

                newListAsset.Add(newAsset);
            }

            return newListAsset;
        }

        private DateOnly ParseDate(string dateString)
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
