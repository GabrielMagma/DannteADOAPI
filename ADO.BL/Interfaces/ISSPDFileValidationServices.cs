using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface ISSPDFileValidationServices
    {

        public Task<ResponseQuery<List<string>>> ReadFileSspdOrginal(LacValidationDTO request, ResponseQuery<List<string>> response);

    }
}
