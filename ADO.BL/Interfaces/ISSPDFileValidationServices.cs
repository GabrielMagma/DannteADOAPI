using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ISSPDFileValidationServices
    {

        public Task<ResponseQuery<bool>> ReadFilesSspd(LacValidationDTO request, ResponseQuery<bool> response);

    }
}
