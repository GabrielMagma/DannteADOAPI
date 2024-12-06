using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class RayosCSVDataAccess : IRayosCSVDataAccess
    {
        protected DannteEssaTestingContext context;
        private readonly IMapper mapper;

        public RayosCSVDataAccess(DannteEssaTestingContext _context, IMapper _mapper)
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
