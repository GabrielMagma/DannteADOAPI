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
    public class SSPDFileProcessingController : Controller
    {
        readonly ISSPDFileProcessingServices SSPDServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public SSPDFileProcessingController(ISSPDFileProcessingServices _sspdServices, IHubContext<NotificationHub> hubContext)
        {
            SSPDServices = _sspdServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Este endpoint busca archivos SSPD.csv ORIGINALES en la carpeta especificada, particionandolos en archivos _unchanged, _continuesInsert, _continuesUpdate, _continuesInvalid, _closed, _closedInvalid, _delete, _update
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDFileProcessingController.ReadFilesSspd))]
        public async Task<IActionResult> ReadFilesSspd(LacValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "SSPD se está procesando");
            await SSPDServices.ReadFilesSspd(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }

    }
}
