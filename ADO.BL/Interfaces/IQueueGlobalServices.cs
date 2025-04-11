using ADO.BL.DTOs;
using ADO.BL.Responses;

namespace ADO.BL.Interfaces
{
    public interface IQueueGlobalServices
    {

        public Task<ResponseQuery<string>> LaunchQueue(QueueValidationDTO request, ResponseQuery<string> response);

    }
}
