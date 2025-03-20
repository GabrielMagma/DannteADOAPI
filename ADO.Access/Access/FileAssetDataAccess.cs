using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class FileAssetDataAccess : IFileAssetDataAccess
    {
        protected DannteTestingContext context;        

        public FileAssetDataAccess(DannteTestingContext _context)
        {
            context = _context;            
        }

        public async Task<Boolean> CreateFile(List<AllAsset> request)
        {
            
            context.AllAssets.AddRangeAsync(request);
            context.SaveChanges();
            var result = true;

            return result;
        }        

    }
}
