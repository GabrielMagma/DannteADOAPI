
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetDataAccess
    {

        public Task<Boolean> CreateFile(List<AllAsset> request);

    }
}
