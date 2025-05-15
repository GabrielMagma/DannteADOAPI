using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileIdeamProcessingServices
    {

        public Task<ResponseQuery<bool>> ReadFilesIdeam(RayosValidationDTO request, ResponseQuery<bool> response);

    }
}
