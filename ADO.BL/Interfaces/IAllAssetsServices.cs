using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IAllAssetsServices
    {

        public ResponseEntity<List<AllAssetDTO>> SearchData(ResponseEntity<List<AllAssetDTO>> response);

    }
}
