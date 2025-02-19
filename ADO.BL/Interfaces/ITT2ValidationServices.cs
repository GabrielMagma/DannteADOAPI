using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface ITT2ValidationServices
    {

        public Task<ResponseEntity<List<StatusFileDTO>>> ValidationTT2(TT2ValidationDTO request, ResponseEntity<List<StatusFileDTO>> response);

    }
}
