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
    public class FilePolesProcessingController : ControllerBase
    {
        readonly IFilePolesProcessingServices polesServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FilePolesProcessingController(IFilePolesProcessingServices _polesServices, IHubContext<NotificationHub> hubContext)
        {
            polesServices = _polesServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma el archivo de postes o apoyos y los valida
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FilePolesValidationController.ReadFilesPoles))]        
        public async Task<IActionResult> ReadFilesPoles(PolesValidationDTO request)
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "El procesado de registros de postes se está ejecutando");
            await polesServices.ReadFilesPoles(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
