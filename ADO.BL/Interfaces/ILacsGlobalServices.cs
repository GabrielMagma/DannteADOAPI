using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ILacsGlobalServices
    {

        public Task<ResponseQuery<List<string>>> ReadFileLacOrginal(ResponseQuery<List<string>> response);

    }
}
