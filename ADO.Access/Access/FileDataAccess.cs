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

        public Boolean CreateFile(List<IaIdeam> request)
        {
            
            context.IaIdeams.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
