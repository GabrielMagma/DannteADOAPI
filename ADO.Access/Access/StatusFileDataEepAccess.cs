using ADO.Access.DataEep;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class StatusFileEepDataAccess : IStatusFileEepDataAccess
    {
        protected DannteEepTestingContext context;
        private readonly IMapper mapper;

        public StatusFileEepDataAccess(DannteEepTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public async Task<Boolean> SaveDataList(List<StatusFile> request)
        {

            context.StatusFiles.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
