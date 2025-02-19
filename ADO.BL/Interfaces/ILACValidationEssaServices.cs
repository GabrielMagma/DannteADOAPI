using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface ILACValidationEssaServices
    {

        public ResponseEntity<List<StatusFileDTO>> ValidationLAC(LacValidationDTO request, ResponseEntity<List<StatusFileDTO>> response);

    }
}
