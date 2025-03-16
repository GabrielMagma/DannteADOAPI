
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetModifiedDataAccess
    {
        public Task SaveData(List<AllAsset> request);       
    }
}
