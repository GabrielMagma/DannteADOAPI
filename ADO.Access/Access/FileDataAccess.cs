using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class FileDataAccess : IFileDataAccess
    {
        protected DannteTestingContext context;        

        public FileDataAccess(DannteTestingContext _context)
        {
            context = _context;            
        }

        public async Task<Boolean> CreateFile(List<IaIdeam> request)
        {
            
            context.IaIdeams.AddRangeAsync(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
