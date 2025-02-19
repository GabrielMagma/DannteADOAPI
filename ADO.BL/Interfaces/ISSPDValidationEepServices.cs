using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface ISSPDValidationEepServices
    {

        public Task<ResponseEntity<List<StatusFileDTO>>> ValidationSSPD(LacValidationDTO request, ResponseEntity<List<StatusFileDTO>> response);

    }
}
