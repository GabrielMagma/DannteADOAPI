using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ILacsFileProcessServices
    {

        public Task<ResponseQuery<List<string>>> ReadFileLacOrginal(LacValidationDTO request, ResponseQuery<List<string>> response);

    }
}
