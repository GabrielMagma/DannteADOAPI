using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileServices
    {

        public Task<ResponseQuery<string>> CreateFileCSV(string name, ResponseQuery<string> response);

    }
}
