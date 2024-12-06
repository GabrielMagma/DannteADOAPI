using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class RamalesDataAccess : IRamalesDataAccess
    {
        protected DannteEssaTestingContext context;
        private readonly IMapper mapper;

        public RamalesDataAccess(DannteEssaTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SaveData(List<FileIoTemp> request)
        {
            
            context.FileIoTemps.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public Boolean SaveDataList(List<FileIoTempDetail> request)
        {

            context.FileIoTempDetails.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

    }
}
