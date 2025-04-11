using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ADO.Access.Access
{
    public class FileAssetModifiedDataAccess : IFileAssetModifiedDataAccess
    {
        protected DannteTestingContext context;

        private readonly IMapper mapper;

        public FileAssetModifiedDataAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        public async Task SaveData(List<AllAsset> request)
        {
            context.AllAssets.AddRange(request);
            context.SaveChanges();
        }

        public async Task<List<AllAssetDTO>> SearchData(string request)
        {
            var splitlist = request.Replace("'", "").Split(',');
            const int batchSize = 40000; // Tamaño del lote 
                                        // Paginar la lista de IDs de contratos
            int totalBatches = (int)Math.Ceiling((double)splitlist.Length / batchSize);
            var listEmpty = new List<AllAssetDTO>();
            for (int batch = 0; batch < totalBatches; batch++)
            {
                var partialIds = splitlist.Skip(batch*batchSize).Take(batchSize);
                var result = context.AllAssets.AsNoTracking().Where(x => partialIds.Contains(x.CodeSig))
                .Select(x => new AllAssetDTO()
                {
                    Id = x.Id,
                    CodeSig = x.CodeSig,
                    Uia = x.Uia,
                    Fparent = x.Fparent,
                    DateInst = x.DateInst,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    Poblation = x.Poblation,
                    Group015 = x.Group015,
                    Year = x.Year,
                    Month = x.Month,
                }).ToList();

                listEmpty.AddRange(result);
            }
            
            
            return listEmpty;
        }

    }
}
