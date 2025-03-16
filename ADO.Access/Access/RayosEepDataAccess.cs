using ADO.Access.DataDev;
using ADO.Access.DataEep;
using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class RayosEepCSVDataAccess : IRayosEepCSVDataAccess
    {
        protected DannteEepTestingContext context;
        private readonly IMapper mapper;

        public RayosEepCSVDataAccess(DannteEepTestingContext _context, IMapper _mapper)
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
