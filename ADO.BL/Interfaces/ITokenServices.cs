using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ITokenServices
    {

        public ResponseQuery<string> CreateToken(ResponseQuery<string> response);

    }
}
