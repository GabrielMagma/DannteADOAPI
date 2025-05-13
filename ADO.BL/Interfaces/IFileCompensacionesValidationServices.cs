using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileCompensacionesValidationServices
    {
        public Task<ResponseQuery<bool>> ReadFilesComp(CompsValidationDTO request, ResponseQuery<bool> response);
    }
}
