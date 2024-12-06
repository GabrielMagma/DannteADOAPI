using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ISSPDServices
    {

        public ResponseQuery<List<string>> ReadFileSspdOrginal(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSspdUnchanged(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSSpdContinuesInsert(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSSpdContinuesUpdate(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSspdUpdate(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> ReadSspdDelete(ResponseQuery<List<string>> response);

    }
}
