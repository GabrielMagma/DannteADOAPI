using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileLACValidationServices
    {

        public ResponseQuery<bool> ValidationLAC(IFormFile file, ResponseQuery<bool> response);

    }
}
