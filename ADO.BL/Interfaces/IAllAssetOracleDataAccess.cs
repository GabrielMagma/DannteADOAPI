using ADO.BL.DataEntities;
using ADO.BL.DTOs;

namespace ADO.BL.Interfaces
{
    public interface IAllAssetOracleDataAccess
    {
        public Boolean SearchData(List<AllAsset> request);

        public Boolean UpdateData(List<AllAssetDTO> request);

        public List<AllAsset> GetListAllAsset();

        public List<AllAsset> GetListAllAssetNews();
    }
}
