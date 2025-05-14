using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class TrafosDataAccess : ITrafosDataAccess
    {
        protected DannteTestingContext context;

        public TrafosDataAccess(DannteTestingContext _context)
        {
            context = _context;
        }

        public async Task<Boolean> SaveData(List<MpTransformerBurned> request)
        {

            context.MpTransformerBurneds.AddRangeAsync(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
