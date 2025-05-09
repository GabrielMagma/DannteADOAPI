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
    public class LacsFileProcessingController : Controller
    {
        readonly ILacsFileProcessServices lacsServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LacsFileProcessingController(ILacsFileProcessServices _lacsServices, IHubContext<NotificationHub> hubContext)
        {
            lacsServices = _lacsServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Este endpoint busca archivos LAC.csv _unchanged, _continues, _continuesInvalid, _closed, _closedInvalid y los procesa
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsFileProcessingController.ReadFilesLacs))]
        public async Task<IActionResult> ReadFilesLacs(LacValidationDTO request)
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "Lacs se está procesando");
            await lacsServices.ReadFilesLacs(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
