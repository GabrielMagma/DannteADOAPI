using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetServices
    {
        public Task<ResponseQuery<string>> CreateFile(AssetValidationDTO request, ResponseQuery<string> response);
    }
}
