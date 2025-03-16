using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetModifiedServices
    {
        public Task<ResponseQuery<string>> UploadFile(FileAssetsValidationDTO request, ResponseQuery<string> response);
    }
}
