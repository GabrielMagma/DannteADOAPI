using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueGlobalController : Controller
    {
        readonly IQueueGlobalServices queueGlobalServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public QueueGlobalController(IQueueGlobalServices _queueGlobalServices, IHubContext<NotificationHub> hubContext)
        {
            queueGlobalServices = _queueGlobalServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Este endpoint lanza el proceso completo de colas
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(QueueGlobalController.TestFunction))]
        public async Task<IActionResult> TestFunction(QueueValidationDTO request)
        {

            ResponseQuery<string> response = new ResponseQuery<string>();
            await queueGlobalServices.LaunchQueue(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
