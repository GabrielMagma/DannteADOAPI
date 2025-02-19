using ADO.Access.DataEep;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class PolesEepDataAccess : IPolesEepDataAccess
    {
        protected DannteEepTestingContext context;        

        public PolesEepDataAccess(DannteEepTestingContext _context)
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
