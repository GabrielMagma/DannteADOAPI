using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ILacsFileProcessServices
    {

        public Task<ResponseQuery<bool>> ReadFilesLacs(LacValidationDTO request, ResponseQuery<bool> response);

    }
}
