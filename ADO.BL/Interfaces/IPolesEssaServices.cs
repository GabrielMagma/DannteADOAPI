using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IPolesEssaServices
    {
        public Task<ResponseQuery<bool>> ValidationFile(ResponseQuery<bool> response);
    }
}
