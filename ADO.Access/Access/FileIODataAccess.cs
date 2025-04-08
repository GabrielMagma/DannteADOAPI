using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class FileIODataAccess : IFileIODataAccess
    {
        protected DannteTestingContext context;

        private readonly IMapper mapper;

        public FileIODataAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public async Task SaveData(List<FilesIo> request)
        {
            context.FilesIos.AddRange(request);
            context.SaveChanges();
        }

        public async Task SaveDataComplete(List<FilesIoComplete> request)
        {
            context.FilesIoCompletes.AddRange(request);
            context.SaveChanges();
        }

        public async Task DeleteData(string fileName)
        {
            var ioExist = context.FilesIos.Where(x => x.FileIo == fileName).ToList();

            if (ioExist != null)
            {
                context.FilesIos.RemoveRange(ioExist);
                context.SaveChanges();
            }

        }

    }
}
