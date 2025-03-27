using ADO.BL.DataEntities;
using ADO.BL.DTOs;

namespace ADO.BL.Interfaces
{
    public interface IAllAssetOracleDataAccess
    {
        public Task<Boolean> SaveData(List<AllAsset> request);

        public Task<Boolean> UpdateData(List<AllAssetDTO> request);        
    }
}
