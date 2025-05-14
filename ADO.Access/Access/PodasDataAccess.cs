using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class PodasDataAccess : IPodasDataAccess
    {
        protected DannteTestingContext context;

        public PodasDataAccess(DannteTestingContext _context)
        {
            context = _context;
        }

        public async Task<Boolean> SaveData(List<IaPoda> request)
        {

            context.IaPodas.AddRangeAsync(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
