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
    public class PodasEssaController : ControllerBase
    {
        readonly IPodasEssaServices podasServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PodasEssaController(IPodasEssaServices _podasServices, IHubContext<NotificationHub> hubContext)
        {
            podasServices = _podasServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        
        /// <summary>
        /// Servicio que toma los datos desde un archivo xlsx y los almacena en la tabla Podas de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(PodasEssaController.SaveDataExcel))]
        public async Task<IActionResult> SaveDataExcel()
        {
            ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
            await podasServices.SaveDataExcel(response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}