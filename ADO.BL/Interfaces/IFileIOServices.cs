using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileIOServices
    {
        public Task<ResponseQuery<string>> UploadIO(IOsValidationDTO iosValidation, ResponseQuery<string> response);
    }
}
