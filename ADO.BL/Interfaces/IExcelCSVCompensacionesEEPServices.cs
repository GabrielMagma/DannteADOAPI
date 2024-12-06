using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IExcelCSVCompensacionesEEPServices
    {

        public ResponseQuery<string> Convert(ResponseQuery<string> response);

    }
}
