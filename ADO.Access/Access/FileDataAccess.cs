using ADO.Access.DataEep;
using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class FileDataAccess : IFileDataAccess
    {
        protected DannteEssaTestingContext context;        

        public FileDataAccess(DannteEssaTestingContext _context)
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
