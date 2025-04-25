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
    public class TC1FileValidationController : Controller
    {
        readonly ITC1FileValidationServices TC1Services;
        private readonly IHubContext<NotificationHub> _hubContext;
        
        public TC1FileValidationController(ITC1FileValidationServices _tc1Services, IHubContext<NotificationHub> hubContext)
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
        [Route(nameof(TC1FileValidationController.ReadFilesTc1))]
        public async Task<IActionResult> ReadFilesTc1(TC1ValidationDTO request)
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "Tc1 se está Validando");
            await TC1Services.ReadFilesTc1(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);

        }

        
    }
}
