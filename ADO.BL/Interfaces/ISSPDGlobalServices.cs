using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ISSPDGlobalServices
    {

        public Task<ResponseQuery<List<string>>> ReadFileSspdOrginal(ResponseQuery<List<string>> response);

    }
}
