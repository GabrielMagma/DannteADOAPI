using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class PodasEssaDataAccess : IPodasEssaDataAccess
    {
        protected DannteTestingContext context;
        private readonly IMapper mapper;

        public PodasEssaDataAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SaveData(List<IaPoda> request)
        {

            context.IaPodas.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
