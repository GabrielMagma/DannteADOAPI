
using ADO.BL.DataEntities;
using ADO.BL.DTOs;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetModifiedDataAccess
    {
        public Task SaveData(List<AllAsset> request);
        public Task<List<AllAssetDTO>> SearchData(string request);
    }
}
