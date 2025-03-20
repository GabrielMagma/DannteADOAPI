using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class RayosCSVDataAccess : IRayosCSVDataAccess
    {
        protected DannteTestingContext context;
        private readonly IMapper mapper;

        public RayosCSVDataAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SaveData(List<MpLightning> request)
        {
            
            context.MpLightnings.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
