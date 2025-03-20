using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class PolesEepDataAccess : IPolesEepDataAccess
    {
        protected DannteTestingContext context;        

        public PolesEepDataAccess(DannteTestingContext _context)
        {
            context = _context;            
        }

        public async Task<Boolean> CreateFile(List<MpUtilityPole> request)
        {

            context.MpUtilityPoles.AddRangeAsync(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
