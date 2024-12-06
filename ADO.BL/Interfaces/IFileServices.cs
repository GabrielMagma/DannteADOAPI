using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileServices
    {

        public ResponseQuery<string> CreateFileCSV(string name, ResponseQuery<string> response);

    }
}
