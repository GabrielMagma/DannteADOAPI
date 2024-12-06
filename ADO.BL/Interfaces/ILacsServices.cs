using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ILacsServices
    {

        public ResponseQuery<List<string>> ReadFileLacOrginal(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSspdUnchanged(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSSpdContinues(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSspdUpdate(ResponseQuery<List<string>> response);

    }
}
