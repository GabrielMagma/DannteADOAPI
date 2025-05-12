using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileRayosValidationServices
    {

        public Task<ResponseQuery<bool>> ReadFilesRayos(RayosValidationDTO request, ResponseQuery<bool> response);        

    }
}
