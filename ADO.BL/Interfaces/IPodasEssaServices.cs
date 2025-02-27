using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IPodasEssaServices
    {        

        public Task<ResponseEntity<List<string>>> SaveDataExcel(ResponseEntity<List<string>> response);

    }
}
