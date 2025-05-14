using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IFileTrafosQValidationServices
    {        

        public Task<ResponseQuery<bool>> ReadFileTrafos(TrafosValidationDTO request, ResponseQuery<bool> response);

    }
}
