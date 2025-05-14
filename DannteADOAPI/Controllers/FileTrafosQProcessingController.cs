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
    public class FileTrafosQProcessingController : ControllerBase
    {
        readonly IFileTrafosQProcessingServices trafosQServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileTrafosQProcessingController(IFileTrafosQProcessingServices _trafosQServices, IHubContext<NotificationHub> hubContext)
        {
            trafosQServices = _trafosQServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        
        /// <summary>
        /// Servicio que toma los datos desde un archivo csv y lo guarda en la bd.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(FileTrafosQValidationController.ReadFileTrafos))]
        public async Task<IActionResult> ReadFileTrafos(TrafosValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "El servicio de Transformadores quemados inicia el proceso de cración de registros");
            await trafosQServices.ReadFileTrafos(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}