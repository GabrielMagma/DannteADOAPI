using ADO.Access.DataTemp;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class FileAssetCierreDataAccess : IFileAssetCierreDataAccess
    {
        protected DannteTestingContext context;

        private readonly IMapper mapper;

        public FileAssetCierreDataAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public async Task SaveData(List<AllAsset> request)
        {
            context.AllAssets.AddRange(request);
            context.SaveChanges();
        }        

    }
}
