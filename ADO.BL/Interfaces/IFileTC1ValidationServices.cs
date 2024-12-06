using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileTC1ValidationServices
    {

        public ResponseQuery<bool> ValidationTC1(IFormFile file, ResponseQuery<bool> response);

    }
}
