using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IExcelCSVCompensacionesESSAServices
    {

        public ResponseQuery<string> Convert(ResponseQuery<string> response);

    }
}
