using ADO.BL.DataEntities;
using ADO.BL.DTOs;

namespace ADO.BL.Interfaces
{
    public interface IAllAssetsDataAccess
    {

        public Boolean SearchData(List<AllAssetNew> request);

        public Boolean UpdateData(List<AllAssetDTO> request);

        public List<AllAsset> GetListAllAsset();

        public List<AllAssetNew> GetListAllAssetNews();

    }
}
