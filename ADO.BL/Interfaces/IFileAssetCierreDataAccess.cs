
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetCierreDataAccess
    {
        public Task SaveData(List<AllAsset> request);       
    }
}
