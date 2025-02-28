using ADO.Access.DataDev;
using ADO.Access.DataEep;
using ADO.Access.DataEssa;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class FileIODataAccess : IFileIODataAccess
    {
        protected DannteEepTestingContext context;
        private readonly IMapper mapper;

        public FileIODataAccess(DannteEepTestingContext _context, IMapper _mapper)
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
            var ioExist = context.FilesIos.FirstOrDefault(x => x.FileIo == fileName);

            if (ioExist != null)
            {
                context.FilesIos.Remove(ioExist);
                context.SaveChanges();
            }

        }

    }
}
