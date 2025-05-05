using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using DannteADOAPI.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class PolesEepController : ControllerBase
    {
        readonly IPolesEepServices polesEepServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PolesEepController(IPolesEepServices _polesEepServices, IHubContext<NotificationHub> hubContext)
        {
            polesEepServices = _polesEepServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma el archivo de postes o apoyos eep, los valida y guarda en la base de datos en la tabla correspondiente,
        /// importante llenar el valor de userId, year y month para el sistema de colas
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(PolesEepController.ValidationFile))]        
        public async Task<IActionResult> ValidationFile(PolesValidationDTO request)
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "Poles se está ejecutando");
            await polesEepServices.ValidationFile(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
