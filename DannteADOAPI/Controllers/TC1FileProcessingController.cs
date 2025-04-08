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
    public class TC1FileProcessingController : Controller
    {
        readonly ITC1FileProcessingServices TC1Services;
        private readonly IHubContext<NotificationHub> _hubContext;
        
        public TC1FileProcessingController(ITC1FileProcessingServices _tc1Services, IHubContext<NotificationHub> hubContext)
        {
            TC1Services = _tc1Services;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Lee y carga los archivos TC1 para el año y mes proporcionados.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TC1FileProcessingController.ReadAssets))]
        public async Task<IActionResult> ReadAssets(TC1ValidationDTO request)
        {

            ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
            await TC1Services.ReadAssets(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);

        }

        
    }
}
