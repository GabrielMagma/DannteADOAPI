using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IAllAssetOracleServices
    {
        public Task<ResponseEntity<List<AllAssetDTO>>> SearchData(ResponseEntity<List<AllAssetDTO>> response);
    }
}
