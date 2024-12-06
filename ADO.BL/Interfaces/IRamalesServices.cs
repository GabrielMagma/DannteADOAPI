using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IRamalesServices
    {

        public ResponseEntity<List<string>> SearchData(ResponseEntity<List<string>> response);        

    }
}
