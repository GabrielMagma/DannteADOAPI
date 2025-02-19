using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface ITC1ValidationServices
    {

        public Task<ResponseEntity<List<StatusFileDTO>>> ValidationTC1(TC1ValidationDTO request, ResponseEntity<List<StatusFileDTO>> response);

    }
}
