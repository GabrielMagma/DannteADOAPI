using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface ILACValidationEepServices
    {

        public Task<ResponseEntity<List<StatusFileDTO>>> ValidationLAC(LacValidationDTO request, ResponseEntity<List<StatusFileDTO>> response);

    }
}
