using ADO.Access.DataEep;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class FileDataAccess : IFileDataAccess
    {
        protected DannteEepTestingContext context;        

        public FileDataAccess(DannteEepTestingContext _context)
        {
            context = _context;            
        }

        public Boolean CreateFile(List<Ideam> request)
        {
            
            context.Ideams.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
