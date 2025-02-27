using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;

namespace ADO.BL.Services
{
    public class AllAssetServices : IAllAssetsServices
    {
        private readonly IAllAssetsDataAccess allAssetDataAccess;
        private readonly IMapper mapper;
        public AllAssetServices(IAllAssetsDataAccess _AllAssetDataAccess, IMapper _mapper)
        {
            allAssetDataAccess = _AllAssetDataAccess;
            mapper = _mapper;
        }

        public ResponseEntity<List<AllAssetDTO>> SearchData(ResponseEntity<List<AllAssetDTO>> response)
        {
            try
            {                                                                 

                var listAllAsset = allAssetDataAccess.GetListAllAsset();
                var listAssetMap = mapper.Map<List<AllAssetDTO>>(listAllAsset);
                var listAllAssetNew = allAssetDataAccess.GetListAllAssetNews();
                var listAssetNewMap = mapper.Map<List<AllAssetDTO>>(listAllAssetNew);
                List< AllAssetDTO > newListAsset = new List< AllAssetDTO >();
                List<AllAssetNew> newListAssetCreate = new List<AllAssetNew>();
                List<AllAssetNew> newListAssetUpdate = new List<AllAssetNew>();
                List<AllAssetDTO> UpdateListAsset = new List<AllAssetDTO>();
                List<AllAssetDTO> ErrorDate = new List<AllAssetDTO>();
                var assetExistUnit = new AllAssetDTO();
                var responseCreate = false;
                var responseUpdate = false;
                var dateToday = DateOnly.FromDateTime(DateTime.Now);

                int i = 0;
                while ((i * 1000) < listAssetMap.Count())
                {
                    var subgroup = listAssetMap.Skip(i*1000).Take(1000);
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
                                assetExistUnit.TypeAsset = ListAssetExist[0].TypeAsset;
                                assetExistUnit.CodeSig = ListAssetExist[0].CodeSig;
                                assetExistUnit.Uia = ListAssetExist[0].Uia;
                                assetExistUnit.Codetaxo = ListAssetExist[0].Codetaxo;
                                assetExistUnit.Fparent = ListAssetExist[0].Fparent;
                                assetExistUnit.Latitude = ListAssetExist[0].Latitude;
                                assetExistUnit.Longitude = ListAssetExist[0].Longitude;
                                assetExistUnit.Poblation = ListAssetExist[0].Poblation;
                                assetExistUnit.Group015 = ListAssetExist[0].Group015;
                                assetExistUnit.Uccap14 = ListAssetExist[0].Uccap14;
                                assetExistUnit.DateInst = ListAssetExist[0].DateInst;
                                assetExistUnit.DateUnin = ListAssetExist[0].DateUnin;
                                assetExistUnit.State = ListAssetExist[0].State;
                                assetExistUnit.IdZone = ListAssetExist[0].IdZone;
                                assetExistUnit.NameZone = ListAssetExist[0].NameZone;
                                assetExistUnit.IdRegion = ListAssetExist[0].IdRegion;
                                assetExistUnit.NameRegion = ListAssetExist[0].NameRegion;
                                assetExistUnit.IdLocality = ListAssetExist[0].IdLocality;
                                assetExistUnit.NameLocality = ListAssetExist[0].NameLocality;
                                assetExistUnit.IdSector = ListAssetExist[0].IdSector;
                                assetExistUnit.NameSector = ListAssetExist[0].NameSector;
                                assetExistUnit.Address = ListAssetExist[0].Address;
                                assetExistUnit.GeographicalCode = ListAssetExist[0].GeographicalCode;
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

                                        assetExistUnit.TypeAsset = greaterDate.TypeAsset;
                                        assetExistUnit.CodeSig = greaterDate.CodeSig;
                                        assetExistUnit.Uia = greaterDate.Uia;
                                        assetExistUnit.Codetaxo = greaterDate.Codetaxo;
                                        assetExistUnit.Fparent = greaterDate.Fparent;
                                        assetExistUnit.Latitude = greaterDate.Latitude;
                                        assetExistUnit.Longitude = greaterDate.Longitude;
                                        assetExistUnit.Poblation = greaterDate.Poblation;
                                        assetExistUnit.Group015 = greaterDate.Group015;
                                        assetExistUnit.Uccap14 = greaterDate.Uccap14;
                                        assetExistUnit.DateInst = greaterDate.DateInst;
                                        assetExistUnit.DateUnin = greaterDate.DateUnin;
                                        assetExistUnit.State = greaterDate.State;
                                        assetExistUnit.IdZone = greaterDate.IdZone;
                                        assetExistUnit.NameZone = greaterDate.NameZone;
                                        assetExistUnit.IdRegion = greaterDate.IdRegion;
                                        assetExistUnit.NameRegion = greaterDate.NameRegion;
                                        assetExistUnit.IdLocality = greaterDate.IdLocality;
                                        assetExistUnit.NameLocality = greaterDate.NameLocality;
                                        assetExistUnit.IdSector = greaterDate.IdSector;
                                        assetExistUnit.NameSector = greaterDate.NameSector;
                                        assetExistUnit.Address = greaterDate.Address;
                                        assetExistUnit.GeographicalCode = greaterDate.GeographicalCode;
                                        UpdateListAsset.Add(assetExistUnit);
                                    }
                                }
                            }
                        }

                    }

                    newListAssetCreate = mapper.Map<List<AllAssetNew>>(newListAsset);

                    if (newListAssetCreate.Count > 0)
                    {
                        responseCreate = allAssetDataAccess.SearchData(newListAssetCreate);
                    }

                    if (UpdateListAsset.Count > 0)
                    {
                        responseUpdate = allAssetDataAccess.UpdateData(UpdateListAsset);
                    }

                    if (ErrorDate != null)
                    {
                        response.Data = ErrorDate;
                    }

                    newListAsset = new List<AllAssetDTO>();
                    newListAssetCreate = new List<AllAssetNew>();
                    newListAssetUpdate = new List<AllAssetNew>();
                    newListAsset = new List<AllAssetDTO>();

                    i++;
                }

                response.Message = "All Registers are created and/or updated";
                response.SuccessData = responseCreate && responseUpdate;
                response.Success = true;
                return response;

            }
            //catch (SqliteException ex)
            //{
            //    response.Message = ex.Message;
            //    response.Success = false;
            //    response.SuccessData = false;
            //}
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

    }
}
