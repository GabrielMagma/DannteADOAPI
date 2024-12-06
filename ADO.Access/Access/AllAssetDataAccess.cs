using ADO.Access.DataEep;
using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class AllAssetDataAccess : IAllAssetsDataAccess
    {
        protected DannteEepTestingContext context;
        private readonly IMapper mapper;

        public AllAssetDataAccess(DannteEepTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SearchData(List<AllAssetNew> request)
        {
            
            context.AllAssetNews.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public Boolean UpdateData(List<AllAssetDTO> request)
        {
            //  id list in request
            var idListToFind = request.Select(x => x.Id).ToList();

            // bring database data from related Ids
            List<AllAssetNew> entities = context.AllAssetNews.Where(x => idListToFind.Contains(x.Id)).ToList();            

            foreach (var item in entities)
            {
                var EntityExist = request.FirstOrDefault(x => x.Id == item.Id);
                
                item.State = EntityExist.State != null ? EntityExist.State : item.State;
                
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

        public List<AllAssetNew> GetListAllAssetNews()
        {

            List<AllAssetNew> entidad = context.AllAssetNews.ToList();
            return entidad;

        }

    }
}
