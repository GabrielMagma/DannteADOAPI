using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;
using ADO.BL.DTOs;
using Microsoft.AspNetCore.SignalR;
using DannteADOAPI.Helper;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TT2FileProcessingController : Controller
    {
        readonly ITT2FileProcessingServices TT2Services;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TT2FileProcessingController(ITT2FileProcessingServices _tt2Services, IHubContext<NotificationHub> hubContext)
        {
            TT2Services = _tt2Services;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Genera un archivo *_completed.csv añadiendo code_sig basado en la tabla all_asset.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TT2FileProcessingController.CompleteTT2Originals))]
        public async Task<IActionResult> CompleteTT2Originals(TT2ValidationDTO request)
        {
            ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
            await TT2Services.CompleteTT2Originals(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}
