using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetValidationServices
    {
        public Task<ResponseQuery<bool>> ReadFilesAssets(FileAssetsValidationDTO request, ResponseQuery<bool> response);
    }
}
