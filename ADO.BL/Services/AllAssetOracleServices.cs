using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ADO.BL.Services
{
    public class AllAssetOracleServices : IAllAssetOracleServices
    {
        private readonly IAllAssetOracleDataAccess allAssetOracleDataAccess;
        private readonly IMapper mapper;
        private readonly string _connectionString;
        public AllAssetOracleServices(IConfiguration configuration, 
            IAllAssetOracleDataAccess _AllAssetOracleDataAccess, 
            IMapper _mapper)
        {
            _connectionString = configuration.GetConnectionString("PgDbTestingConnection");
            allAssetOracleDataAccess = _AllAssetOracleDataAccess;
            mapper = _mapper;
        }

        public async Task<ResponseEntity<List<AllAssetDTO>>> SearchData(string table, ResponseEntity<List<AllAssetDTO>> response)
        {
            try
            {
                //oracle conection
                var dataOracle = await OracleConection(table);

                var assetList = new List<AllAssetDTO>();
                var listAssetNewMap = await GetListAllAssetNews(assetList);                                
                var responseCreate = false;
                var responseUpdate = false;
                var dateToday = DateOnly.FromDateTime(DateTime.Now);

                int i = 0;
                while ((i * 10000) < dataOracle.Count())
                {
                    var subgroup = dataOracle.Skip(i * 10000).Take(10000);

                    List<AllAssetDTO> newListAsset = new List<AllAssetDTO>();
                    List<AllAsset> newListAssetCreate = new List<AllAsset>();
                    List<AllAsset> newListAssetUpdate = new List<AllAsset>();
                    List<AllAssetDTO> UpdateListAsset = new List<AllAssetDTO>();
                    List<AllAssetDTO> ErrorDate = new List<AllAssetDTO>();
                    var assetExistUnit = new AllAssetDTO();

                    foreach (var item in subgroup)
                    {
                        var ListAssetExist = subgroup.Where(x => x.CodeSig == item.CodeSig && x.Uia == item.Uia).ToList();
                        if (ListAssetExist.Count == 1)
                        {
                            assetExistUnit = listAssetNewMap.FirstOrDefault(x => x.CodeSig == ListAssetExist[0].CodeSig && x.Uia == ListAssetExist[0].Uia);

                            if (assetExistUnit == null)
                            {
                                newListAsset.Add(ListAssetExist[0]);
                            }
                            else if (assetExistUnit.State != ListAssetExist[0].State)
                            {

                                assetExistUnit.TypeAsset = "TRANSFORMADOR";
                                if (table == "RECONECTADOR")
                                {
                                    assetExistUnit.TypeAsset = "RECONECTADOR";
                                }
                                else if (table == "INTERRUPTOR")
                                {
                                    assetExistUnit.TypeAsset = "SECCIONADOR";
                                }

                                assetExistUnit.CodeSig = ListAssetExist[0].CodeSig;
                                assetExistUnit.Uia = ListAssetExist[0].Uia;
                                assetExistUnit.Codetaxo = ListAssetExist[0].Codetaxo;
                                assetExistUnit.Fparent = ListAssetExist[0].Fparent;
                                assetExistUnit.Uccap14 = ListAssetExist[0].Uccap14;
                                assetExistUnit.Group015 = ListAssetExist[0].Group015;
                                assetExistUnit.Latitude = ListAssetExist[0].Latitude;
                                assetExistUnit.Longitude = ListAssetExist[0].Longitude;
                                assetExistUnit.DateInst = ListAssetExist[0].DateInst;
                                assetExistUnit.Poblation = ListAssetExist[0].Poblation;
                                assetExistUnit.Address = ListAssetExist[0].Address;
                                
                                assetExistUnit.DateUnin = ListAssetExist[0].DateUnin;
                                assetExistUnit.State = ListAssetExist[0].State;
                                assetExistUnit.IdRegion = ListAssetExist[0].IdRegion;
                                assetExistUnit.NameRegion = ListAssetExist[0].NameRegion;                                
                                

                                UpdateListAsset.Add(assetExistUnit);
                            }
                        }
                        else
                        {
                            var greaterDate = ListAssetExist.OrderByDescending(x => x.DateInst).FirstOrDefault();
                            if (greaterDate.DateInst > dateToday)
                            {
                                ErrorDate.Add(greaterDate);
                            }
                            else
                            {
                                assetExistUnit = listAssetNewMap.FirstOrDefault(x => x.CodeSig == greaterDate.CodeSig && x.Uia == greaterDate.Uia);
                                if (assetExistUnit == null)
                                {
                                    newListAsset.Add(assetExistUnit);
                                }
                                else
                                {
                                    if (assetExistUnit.State != greaterDate.State)
                                    {

                                        assetExistUnit.CodeSig = greaterDate.CodeSig;
                                        assetExistUnit.Uia = greaterDate.Uia;
                                        assetExistUnit.Codetaxo = greaterDate.Codetaxo;
                                        assetExistUnit.Fparent = greaterDate.Fparent;
                                        assetExistUnit.Uccap14 = greaterDate.Uccap14;
                                        assetExistUnit.Group015 = greaterDate.Group015;
                                        assetExistUnit.Latitude = greaterDate.Latitude;
                                        assetExistUnit.Longitude = greaterDate.Longitude;
                                        assetExistUnit.DateInst = greaterDate.DateInst;
                                        assetExistUnit.Poblation = greaterDate.Poblation;
                                        assetExistUnit.Address = greaterDate.Address;
                                        UpdateListAsset.Add(assetExistUnit);
                                    }
                                }
                            }
                        }

                    }

                    newListAssetCreate = MapperListReverse(newListAsset);

                    if (newListAssetCreate.Count > 0)
                    {
                        responseCreate = await SaveData(newListAssetCreate);
                    }

                    if (UpdateListAsset.Count > 0)
                    {
                        responseUpdate = await UpdateData(UpdateListAsset);
                    }

                    newListAsset = new List<AllAssetDTO>();
                    newListAssetCreate = new List<AllAsset>();
                    newListAssetUpdate = new List<AllAsset>();
                    newListAsset = new List<AllAssetDTO>();

                    i++;
                }


                Console.WriteLine("Proceso completado.");

                response.Message = "All Registers are created and/or updated";
                response.SuccessData = responseCreate && responseUpdate;
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
                var SelectQuery = $@"SELECT * FROM public.all_asset";
                using (var reader = new NpgsqlCommand(SelectQuery, connection))
                {
                    try
                    {

                        using (var result = await reader.ExecuteReaderAsync())
                        {
                            while (await result.ReadAsync())
                            {                                
                                var temp = new AllAssetDTO();
                                temp.Id = long.Parse(result[0].ToString());
                                temp.TypeAsset = result[1].ToString();
                                temp.CodeSig = result[2].ToString();
                                temp.Uia = result[3].ToString();
                                temp.Codetaxo = result[4].ToString();
                                temp.Fparent = result[5].ToString();
                                temp.Latitude = !string.IsNullOrEmpty(result[6].ToString()) ? float.Parse(result[6].ToString()) : -1;
                                temp.Longitude = !string.IsNullOrEmpty(result[7].ToString()) ? float.Parse(result[7].ToString()) : -1;
                                temp.Poblation = result[8].ToString();
                                temp.Group015 = result[9].ToString();
                                temp.Uccap14 = result[10].ToString();
                                if (!string.IsNullOrEmpty(result[11].ToString()))
                                {
                                    temp.DateInst = DateOnly.FromDateTime(DateTime.Parse(result[11].ToString()));
                                }
                                if (!string.IsNullOrEmpty(result[12].ToString()))
                                {
                                    temp.DateUnin = DateOnly.FromDateTime(DateTime.Parse(result[12].ToString()));
                                }
                                temp.State = !string.IsNullOrEmpty(result[13].ToString()) ? int.Parse(result[13].ToString()) : -1;                                
                                temp.IdRegion = !string.IsNullOrEmpty(result[14].ToString()) ? long.Parse(result[14].ToString()) : -1;
                                temp.NameRegion = result[15].ToString();                                
                                temp.Address = result[16].ToString();                                


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
    }
}
