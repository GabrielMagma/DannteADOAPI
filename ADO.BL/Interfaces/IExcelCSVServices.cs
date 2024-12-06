using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IExcelCSVServices
    {

        public ResponseQuery<string> ProcessXlsx(ResponseQuery<string> response);

    }
}
