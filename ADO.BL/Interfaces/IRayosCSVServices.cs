using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IRayosCSVServices
    {

        public Task<ResponseEntity<List<string>>> SearchDataCSV(RayosValidationDTO request, ResponseEntity<List<string>> response);

        public Task<ResponseEntity<List<string>>> SearchDataExcel(RayosValidationDTO request, ResponseEntity<List<string>> response);

    }
}
