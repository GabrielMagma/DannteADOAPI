using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IAssetsServices
    {

        public ResponseQuery<List<string>> ReadAssets(ResponseQuery<List<string>> response);

    }
}
