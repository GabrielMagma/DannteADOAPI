using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFilePolesProcessingServices
    {
        public Task<ResponseQuery<bool>> ReadFilesPoles(PolesValidationDTO request, ResponseQuery<bool> response);
    }
}
