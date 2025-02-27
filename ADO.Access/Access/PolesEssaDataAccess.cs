using ADO.Access.DataEep;
using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class PolesEssaDataAccess : IPolesEssaDataAccess
    {
        protected DannteEssaTestingContext context;        

        public PolesEssaDataAccess(DannteEssaTestingContext _context)
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
