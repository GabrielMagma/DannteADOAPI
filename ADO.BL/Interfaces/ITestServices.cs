using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ITestServices
    {
        public Task<bool> SendTest();
    }
}
