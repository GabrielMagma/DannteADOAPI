using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class StatusFileDataEssaAccess : IStatusFileEssaDataAccess
    {
        protected DannteEssaTestingContext context;
        private readonly IMapper mapper;

        public StatusFileDataEssaAccess(DannteEssaTestingContext _context, IMapper _mapper)
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
