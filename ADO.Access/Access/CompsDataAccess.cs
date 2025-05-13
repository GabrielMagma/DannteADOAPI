using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class CompsDataAccess : ICompsDataAccess
    {
        protected DannteTestingContext context;        

        public CompsDataAccess(DannteTestingContext _context)
        {
            context = _context;            
        }

        public async Task<Boolean> CreateFile(List<MpCompensation> request)
        {

            context.MpCompensations.AddRangeAsync(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
