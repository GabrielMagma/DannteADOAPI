using ADO.Access.DataEep;
using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class AllAssetOracleDataAccess : IAllAssetOracleDataAccess
    {
        protected DannteEepTestingContext context;
        private readonly IMapper mapper;

        public AllAssetOracleDataAccess(DannteEepTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SearchData(List<AllAsset> request)
        {

            context.AllAssets.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public Boolean UpdateData(List<AllAssetDTO> request)
        {
            //  id list in request
            var idListToFind = request.Select(x => x.Id).ToList();

            // bring database data from related Ids
            List<AllAsset> entities = context.AllAssets.Where(x => idListToFind.Contains(x.Id)).ToList();

            foreach (var item in entities)
            {
                var EntityExist = request.FirstOrDefault(x => x.Id == item.Id);

                item.CodeSig = EntityExist.CodeSig != null ? EntityExist.CodeSig : item.CodeSig;
                item.Uia = EntityExist.Uia != null ? EntityExist.Uia : item.Uia;
                item.Codetaxo = EntityExist.Codetaxo != null ? EntityExist.Codetaxo : item.Codetaxo;
                item.Fparent = EntityExist.Fparent != null ? EntityExist.Fparent : item.Fparent;
                item.Uccap14 = EntityExist.Uccap14 != null ? EntityExist.Uccap14 : item.Uccap14;
                item.Group015 = EntityExist.Group015 != null ? EntityExist.Group015 : item.Group015;                                
                item.Latitude = EntityExist.Latitude != null ? EntityExist.Latitude : item.Latitude;
                item.Longitude = EntityExist.Longitude != null ? EntityExist.Longitude : item.Longitude;
                item.DateInst = EntityExist.DateInst != null ? EntityExist.DateInst : item.DateInst;
                item.Poblation = EntityExist.Poblation != null ? EntityExist.Poblation : item.Poblation;
                item.Address = EntityExist.Address != null ? EntityExist.Address : item.Address;                

            }

            context.SaveChanges();


            var result = true;

            return result;
        }

        public List<AllAsset> GetListAllAsset()
        {

            List<AllAsset> entidad = context.AllAssets.ToList();
            return entidad;

        }

        public List<AllAsset> GetListAllAssetNews()
        {

            List<AllAsset> entidad = context.AllAssets.ToList();
            return entidad;

        }
    }
}
