using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using DannteADOAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LacsFileProcessController : Controller
    {
        readonly ILacsFileProcessServices lacsServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LacsFileProcessController(ILacsFileProcessServices _lacsServices, IHubContext<NotificationHub> hubContext)
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
        [Route(nameof(LacsFileProcessController.ReadFileLacOrginal))]
        public async Task<IActionResult> ReadFileLacOrginal(LacValidationDTO request)
        {

            ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
            await lacsServices.ReadFileLacOrginal(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
