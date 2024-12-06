using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileTT2ValidationServices
    {

        public ResponseQuery<bool> ValidationTT2(IFormFile file, ResponseQuery<bool> response);

    }
}
