using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileIdeamValidationServices
    {

        public Task<ResponseQuery<bool>> CreateFileCSV(RayosValidationDTO request, ResponseQuery<bool> response);

    }
}
