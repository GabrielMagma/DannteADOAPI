using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IRayosCSVServices
    {

        public ResponseEntity<List<string>> SearchDataCSV(ResponseEntity<List<string>> response);

        public ResponseEntity<List<string>> SearchDataExcel(ResponseEntity<List<string>> response);

    }
}
