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
    public class TT2FileValidationController : Controller
    {
        readonly ITT2FileValidationServices TT2Services;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TT2FileValidationController(ITT2FileValidationServices _tt2Services, IHubContext<NotificationHub> hubContext)
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
        [Route(nameof(TT2FileValidationController.ReadFilesTT2))]
        public async Task<IActionResult> ReadFilesTT2(TT2ValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "TT2 se está Validando");
            await TT2Services.ReadFilesTT2(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}
