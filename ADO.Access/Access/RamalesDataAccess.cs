using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class RamalesDataAccess : IRamalesDataAccess
    {
        protected DannteTestingContext context;
        private readonly IMapper mapper;

        public RamalesDataAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public Boolean SaveData(List<FilesIo> request)
        {
            
            context.FilesIos.AddRange(request);
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
