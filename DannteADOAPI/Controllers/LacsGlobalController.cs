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
    public class LacsGlobalController : Controller
    {
        readonly ILacsGlobalEssaServices lacsServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LacsGlobalController(ILacsGlobalEssaServices _lacsServices, IHubContext<NotificationHub> hubContext)
        {
            lacsServices = _lacsServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Este endpoint busca archivos LAC.csv ORIGINALES en la carpeta especificada, particionandolos en archivos: _unchanged, _continues, _continuesInvalid, _closed, _closedInvalid
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsGlobalController.ReadFileLacOrginal))]
        public async Task<IActionResult> ReadFileLacOrginal(LacValidationDTO request)
        {

            ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
            await lacsServices.ReadFileLacOrginal(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
