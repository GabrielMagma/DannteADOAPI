using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileIOValidationServices
    {
        public Task<ResponseQuery<bool>> ReadFilesIos(IOsValidationDTO iosValidation, ResponseQuery<bool> response);
    }
}
