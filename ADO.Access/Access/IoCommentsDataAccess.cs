using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;

namespace ADO.Access.Access
{
    public class IoCommentsDataAccess : IIoCommentsDataAccess
    {
        protected DannteTestingContext context;        

        public IoCommentsDataAccess(DannteTestingContext _context)
        {
            context = _context;            
        }

        public long CreateRegister(IoComment request)
        {
            
            context.IoComments.Add(request);
            context.SaveChanges();
            var result = true;

            return request.Id;
        }

    }
}
