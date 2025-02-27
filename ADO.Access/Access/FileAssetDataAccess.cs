using ADO.Access.DataDev;
using ADO.Access.DataEep;
using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class FileAssetDataAccess : IFileAssetDataAccess
    {
        protected DannteEssaTestingContext context;        

        public FileAssetDataAccess(DannteEssaTestingContext _context)
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
