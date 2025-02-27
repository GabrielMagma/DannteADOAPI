using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileAssetServices
    {

        public Task<ResponseQuery<string>> CreateFile(ResponseQuery<string> response);

    }
}
