using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ADO.BL.Services
{
    public class AllAssetOracleServices : IAllAssetOracleServices
    {
        private readonly IAllAssetOracleDataAccess allAssetOracleDataAccess;
        private readonly IMapper mapper;
        public AllAssetOracleServices(IAllAssetOracleDataAccess _AllAssetOracleDataAccess, IMapper _mapper)
        {
            allAssetOracleDataAccess = _AllAssetOracleDataAccess;
            mapper = _mapper;
        }

        public ResponseEntity<List<AllAssetDTO>> SearchDataTransfor(ResponseEntity<List<AllAssetDTO>> response)
        {
            try
            {
                //oracle conection
                var dataOracle = OracleConectionTransfor();
                
                var listAllAssetNew = allAssetOracleDataAccess.GetListAllAssetNews();
                var listAssetNewMap = mapper.Map<List<AllAssetDTO>>(listAllAssetNew);
                List<AllAssetDTO> newListAsset = new List<AllAssetDTO>();
                List<AllAsset> newListAssetCreate = new List<AllAsset>();
                List<AllAsset> newListAssetUpdate = new List<AllAsset>();
                List<AllAssetDTO> UpdateListAsset = new List<AllAssetDTO>();
                List<AllAssetDTO> ErrorDate = new List<AllAssetDTO>();
                var assetExistUnit = new AllAssetDTO();
                var responseCreate = false;
                var responseUpdate = false;
                var dateToday = DateTime.Now;                

                int i = 0;
                while ((i * 1000) < dataOracle.Count())
                {
                    var subgroup = dataOracle.Skip(i * 1000).Take(1000);
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
                                assetExistUnit.TypeAsset = "TRANSFORMADOR";
                                assetExistUnit.DateUnin = ListAssetExist[0].DateUnin;
                                assetExistUnit.State = ListAssetExist[0].State;
                                assetExistUnit.IdRegion = ListAssetExist[0].IdRegion;
                                assetExistUnit.NameRegion = ListAssetExist[0].NameRegion;
                                assetExistUnit.IdZone = ListAssetExist[0].IdZone;
                                assetExistUnit.NameZone = ListAssetExist[0].NameZone;
                                assetExistUnit.IdLocality = ListAssetExist[0].IdLocality;
                                assetExistUnit.NameLocality = ListAssetExist[0].NameLocality;
                                assetExistUnit.IdSector = ListAssetExist[0].IdSector;
                                assetExistUnit.NameSector = ListAssetExist[0].NameSector;
                                //assetExistUnit.TypeAsset = "SECCIONADOR";
                                //assetExistUnit.TypeAsset = "RECONECTADOR";

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

                    newListAssetCreate = mapper.Map<List<AllAsset>>(newListAsset);

                    if (newListAssetCreate.Count > 0)
                    {
                        responseCreate = allAssetOracleDataAccess.SearchData(newListAssetCreate);
                    }

                    if (UpdateListAsset.Count > 0)
                    {
                        responseUpdate = allAssetOracleDataAccess.UpdateData(UpdateListAsset);
                    }

                    if (ErrorDate != null)
                    {
                        response.Data = ErrorDate;
                    }

                    newListAsset = new List<AllAssetDTO>();
                    newListAssetCreate = new List<AllAsset>();
                    newListAssetUpdate = new List<AllAsset>();
                    newListAsset = new List<AllAssetDTO>();

                    i++;
                }

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

        public ResponseEntity<List<AllAssetDTO>> SearchDataSwitch(ResponseEntity<List<AllAssetDTO>> response)
        {
            try
            {
                //oracle conection
                var dataOracle = OracleConection("SPARD_SWITCH");

                var listAllAssetNew = allAssetOracleDataAccess.GetListAllAssetNews();
                var listAssetNewMap = mapper.Map<List<AllAssetDTO>>(listAllAssetNew);
                List<AllAssetDTO> newListAsset = new List<AllAssetDTO>();
                List<AllAsset> newListAssetCreate = new List<AllAsset>();
                List<AllAsset> newListAssetUpdate = new List<AllAsset>();
                List<AllAssetDTO> UpdateListAsset = new List<AllAssetDTO>();
                List<AllAssetDTO> ErrorDate = new List<AllAssetDTO>();
                var assetExistUnit = new AllAssetDTO();
                var responseCreate = false;
                var responseUpdate = false;
                var dateToday = DateTime.Now;

                int i = 0;
                while ((i * 1000) < dataOracle.Count())
                {
                    var subgroup = dataOracle.Skip(i * 1000).Take(1000);
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

                                assetExistUnit.CodeSig = ListAssetExist[0].CodeSig;
                                assetExistUnit.Uia = ListAssetExist[0].Uia;
                                assetExistUnit.Codetaxo = ListAssetExist[0].Codetaxo;
                                assetExistUnit.Fparent = ListAssetExist[0].Fparent;
                                assetExistUnit.Uccap14 = ListAssetExist[0].Uccap14;                                
                                assetExistUnit.Latitude = ListAssetExist[0].Latitude;
                                assetExistUnit.Longitude = ListAssetExist[0].Longitude;
                                assetExistUnit.DateInst = ListAssetExist[0].DateInst;
                                assetExistUnit.Poblation = ListAssetExist[0].Poblation;
                                assetExistUnit.Address = ListAssetExist[0].Address;
                                //assetExistUnit.TypeAsset = "TRANSFORMADOR";
                                assetExistUnit.TypeAsset = "SECCIONADOR";
                                //assetExistUnit.TypeAsset = "RECONECTADOR";
                                assetExistUnit.DateUnin = ListAssetExist[0].DateUnin;
                                assetExistUnit.State = ListAssetExist[0].State;
                                assetExistUnit.IdRegion = ListAssetExist[0].IdRegion;
                                assetExistUnit.NameRegion = ListAssetExist[0].NameRegion;
                                assetExistUnit.IdZone = ListAssetExist[0].IdZone;
                                assetExistUnit.NameZone = ListAssetExist[0].NameZone;
                                assetExistUnit.IdLocality = ListAssetExist[0].IdLocality;
                                assetExistUnit.NameLocality = ListAssetExist[0].NameLocality;
                                assetExistUnit.IdSector = ListAssetExist[0].IdSector;
                                assetExistUnit.NameSector = ListAssetExist[0].NameSector;

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

                    newListAssetCreate = mapper.Map<List<AllAsset>>(newListAsset);

                    if (newListAssetCreate.Count > 0)
                    {
                        responseCreate = allAssetOracleDataAccess.SearchData(newListAssetCreate);
                    }

                    if (UpdateListAsset.Count > 0)
                    {
                        responseUpdate = allAssetOracleDataAccess.UpdateData(UpdateListAsset);
                    }

                    if (ErrorDate != null)
                    {
                        response.Data = ErrorDate;
                    }

                    newListAsset = new List<AllAssetDTO>();
                    newListAssetCreate = new List<AllAsset>();
                    newListAssetUpdate = new List<AllAsset>();
                    newListAsset = new List<AllAssetDTO>();

                    i++;
                }

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

        public ResponseEntity<List<AllAssetDTO>> SearchDataRecloser(ResponseEntity<List<AllAssetDTO>> response)
        {
            try
            {
                //oracle conection
                var dataOracle = OracleConection("SPARD_RECLOSER");

                var listAllAssetNew = allAssetOracleDataAccess.GetListAllAssetNews();
                var listAssetNewMap = mapper.Map<List<AllAssetDTO>>(listAllAssetNew);
                List<AllAssetDTO> newListAsset = new List<AllAssetDTO>();
                List<AllAsset> newListAssetCreate = new List<AllAsset>();
                List<AllAsset> newListAssetUpdate = new List<AllAsset>();
                List<AllAssetDTO> UpdateListAsset = new List<AllAssetDTO>();
                List<AllAssetDTO> ErrorDate = new List<AllAssetDTO>();
                var assetExistUnit = new AllAssetDTO();
                var responseCreate = false;
                var responseUpdate = false;
                var dateToday = DateTime.Now;

                int i = 0;
                while ((i * 1000) < dataOracle.Count())
                {
                    var subgroup = dataOracle.Skip(i * 1000).Take(1000);
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

                                assetExistUnit.CodeSig = ListAssetExist[0].CodeSig;
                                assetExistUnit.Uia = ListAssetExist[0].Uia;
                                assetExistUnit.Codetaxo = ListAssetExist[0].Codetaxo;
                                assetExistUnit.Fparent = ListAssetExist[0].Fparent;
                                assetExistUnit.Uccap14 = ListAssetExist[0].Uccap14;                                
                                assetExistUnit.Latitude = ListAssetExist[0].Latitude;
                                assetExistUnit.Longitude = ListAssetExist[0].Longitude;
                                assetExistUnit.DateInst = ListAssetExist[0].DateInst;
                                assetExistUnit.Poblation = ListAssetExist[0].Poblation;
                                assetExistUnit.Address = ListAssetExist[0].Address;
                                //assetExistUnit.TypeAsset = "TRANSFORMADOR";
                                //assetExistUnit.TypeAsset = "SECCIONADOR";
                                assetExistUnit.TypeAsset = "RECONECTADOR";
                                assetExistUnit.DateUnin = ListAssetExist[0].DateUnin;
                                assetExistUnit.State = ListAssetExist[0].State;
                                assetExistUnit.IdRegion = ListAssetExist[0].IdRegion;
                                assetExistUnit.NameRegion = ListAssetExist[0].NameRegion;
                                assetExistUnit.IdZone = ListAssetExist[0].IdZone;
                                assetExistUnit.NameZone = ListAssetExist[0].NameZone;
                                assetExistUnit.IdLocality = ListAssetExist[0].IdLocality;
                                assetExistUnit.NameLocality = ListAssetExist[0].NameLocality;
                                assetExistUnit.IdSector = ListAssetExist[0].IdSector;
                                assetExistUnit.NameSector = ListAssetExist[0].NameSector;

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

                    newListAssetCreate = mapper.Map<List<AllAsset>>(newListAsset);

                    if (newListAssetCreate.Count > 0)
                    {
                        responseCreate = allAssetOracleDataAccess.SearchData(newListAssetCreate);
                    }

                    if (UpdateListAsset.Count > 0)
                    {
                        responseUpdate = allAssetOracleDataAccess.UpdateData(UpdateListAsset);
                    }

                    if (ErrorDate != null)
                    {
                        response.Data = ErrorDate;
                    }

                    newListAsset = new List<AllAssetDTO>();
                    newListAssetCreate = new List<AllAsset>();
                    newListAssetUpdate = new List<AllAsset>();
                    newListAsset = new List<AllAssetDTO>();

                    i++;
                }

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

        private List<AllAssetDTO> OracleConectionTransfor()
        {

            string localConnectionString = "Data Source=ndzh4dbucqnindx4_tpurgent ;User ID=ADMIN;Password=MagmaDannte2024!;Connection Timeout=120;Tns_Admin=C:\\Users\\ingen\\source\\repos\\wallet;Wallet_Location=C:\\Users\\ingen\\source\\repos\\wallet;";

            using (OracleConnection localConnection = new OracleConnection(localConnectionString))
            {
                List<AllAssetDTO> allAssets = new List<AllAssetDTO>();
                try
                {
                    localConnection.Open();
                    Console.WriteLine("Conexión abierta con éxito.");


                    string query = $"SELECT * FROM SPARD_TRANSFOR";
                    using (OracleCommand command = new OracleCommand(query, localConnection))
                    {
                        // Ejecutar el comando y leer los datos
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // catch error 
                                if (!reader.IsDBNull(3) && !reader.IsDBNull(4) && !reader.IsDBNull(6))
                                {
                                    // Procesar cada fila (cambia el índice según el tipo de dato)
                                    AllAssetDTO allAsset = new AllAssetDTO();
                                    allAsset.CodeSig = reader.GetString(3);
                                    allAsset.Uia = reader.GetString(4);
                                    allAsset.Codetaxo = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;
                                    allAsset.Fparent = reader.GetString(6);
                                    allAsset.Uccap14 = !reader.IsDBNull(8) ? reader.GetString(8) : string.Empty;
                                    allAsset.Group015 = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty;
                                    allAsset.Latitude = !reader.IsDBNull(13) ? float.Parse(reader.GetString(13)) : null;
                                    allAsset.Longitude = !reader.IsDBNull(14) ? float.Parse(reader.GetString(14)) : null;
                                    allAsset.DateInst = !reader.IsDBNull(15) ? DateTime.Parse(reader.GetString(15)) : null;
                                    allAsset.Poblation = !reader.IsDBNull(16) ? reader.GetString(16) : string.Empty;
                                    allAsset.Address = !reader.IsDBNull(19) ? reader.GetString(19) : string.Empty;
                                    allAsset.TypeAsset = "TRANSFORMADOR";
                                    allAsset.DateUnin = DateTime.Parse("2099-12-31");
                                    allAsset.State = 2;
                                    allAsset.IdRegion = 1;
                                    allAsset.NameRegion = "GENERAL";
                                    allAsset.IdZone = 1;
                                    allAsset.NameZone = "ZONA GENERAL";
                                    allAsset.IdLocality = 1;
                                    allAsset.NameLocality = "LOCALIDAD GENERAL";
                                    allAsset.IdSector = 1;
                                    allAsset.NameSector = "SECTOR GENERAL";                                    
                                    allAssets.Add(allAsset);
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

        private List<AllAssetDTO> OracleConection(string table) {

            string localConnectionString = "Data Source=ndzh4dbucqnindx4_tpurgent ;User ID=ADMIN;Password=MagmaDannte2024!;Connection Timeout=120;Tns_Admin=C:\\Users\\ingen\\source\\repos\\wallet;Wallet_Location=C:\\Users\\ingen\\source\\repos\\wallet;";

            using (OracleConnection localConnection = new OracleConnection(localConnectionString))            
            {
                List<AllAssetDTO> allAssets = new List<AllAssetDTO>();
                try
                {
                    localConnection.Open();
                    Console.WriteLine("Conexión abierta con éxito.");                    
                    
                    string query = $"SELECT * FROM {table}";
                    using (OracleCommand command = new OracleCommand(query, localConnection))
                    {
                        // Ejecutar el comando y leer los datos
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // catch error 
                                if (!reader.IsDBNull(3) && !reader.IsDBNull(4) && !reader.IsDBNull(6))
                                {
                                    // Procesar cada fila (cambia el índice según el tipo de dato)
                                    AllAssetDTO allAsset = new AllAssetDTO();
                                    allAsset.CodeSig = reader.GetString(3);
                                    allAsset.Uia = reader.GetString(4);                                    
                                    allAsset.Codetaxo = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;
                                    allAsset.Fparent = reader.GetString(6);                                    
                                    allAsset.Uccap14 = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty;
                                    allAsset.Latitude = !reader.IsDBNull(12) ? float.Parse(reader.GetString(12)) : null;
                                    allAsset.Longitude = !reader.IsDBNull(13) ? float.Parse(reader.GetString(13)) : null;
                                    allAsset.DateInst = !reader.IsDBNull(14) ? DateTime.Parse(reader.GetString(14)) : null;
                                    allAsset.Poblation = !reader.IsDBNull(15) ? reader.GetString(15) : string.Empty;
                                    allAsset.Address = !reader.IsDBNull(20) ? reader.GetString(20) : string.Empty;
                                    allAsset.TypeAsset = table == "SPARD_SWITCH" ? "SECCIONADOR" : "RECONECTADOR";
                                    allAsset.DateUnin = DateTime.Parse("2099-12-31");
                                    allAsset.State = 2;
                                    allAsset.IdRegion = 1;
                                    allAsset.NameRegion = "GENERAL";
                                    allAsset.IdZone = 1;
                                    allAsset.NameZone = "ZONA GENERAL";
                                    allAsset.IdLocality = 1;
                                    allAsset.NameLocality = "LOCALIDAD GENERAL";
                                    allAsset.IdSector = 1;
                                    allAsset.NameSector = "SECTOR GENERAL";
                                    allAssets.Add(allAsset);                                    
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
    }
}
