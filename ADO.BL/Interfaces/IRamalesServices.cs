using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IRamalesServices
    {

        public Task<ResponseEntity<List<string>>> SearchData(RamalesValidationDTO request, ResponseEntity<List<string>> response);        

    }
}
