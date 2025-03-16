using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IPolesEepServices
    {
        public Task<ResponseQuery<bool>> ValidationFile(PolesValidationDTO request, ResponseQuery<bool> response);
    }
}
