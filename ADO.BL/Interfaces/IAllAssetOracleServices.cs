using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IAllAssetOracleServices
    {
        public ResponseEntity<List<AllAssetDTO>> SearchDataTransfor(ResponseEntity<List<AllAssetDTO>> response);

        public ResponseEntity<List<AllAssetDTO>> SearchDataSwitch(ResponseEntity<List<AllAssetDTO>> response);

        public ResponseEntity<List<AllAssetDTO>> SearchDataRecloser(ResponseEntity<List<AllAssetDTO>> response);
    }
}
