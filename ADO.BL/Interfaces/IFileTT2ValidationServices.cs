using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileTT2ValidationServices
    {

        public Task<ResponseQuery<bool>> ValidationTT2(TT2ValidationDTO request, ResponseQuery<bool> response);

    }
}
