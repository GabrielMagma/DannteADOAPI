using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFilePodasProcessingServices
    {        

        public Task<ResponseQuery<bool>> ReadFilePodas(PodasValidationDTO request, ResponseQuery<bool> response);

    }
}
