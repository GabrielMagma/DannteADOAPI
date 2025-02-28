using ADO.Access.DataDev;
using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class PodasEssaDataAccess : IPodasEssaDataAccess
    {
        protected DannteEssaTestingContext context;
        private readonly IMapper mapper;

        public PodasEssaDataAccess(DannteEssaTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SaveData(List<Poda> request)
        {

            context.Podas.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
