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
    public class FileTT2ValidationController : ControllerBase
    {
        readonly IFileTT2ValidationServices fileTT2Services;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileTT2ValidationController(IFileTT2ValidationServices _fileTT2Services, IHubContext<NotificationHub> hubContext)
        {
            fileTT2Services = _fileTT2Services;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma un archivo de datos CSV y lo ajusta agregando la columna code_sig si no la tiene
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileTT2ValidationController.ValidationTT2))]        
        public async Task<IActionResult> ValidationTT2(TT2ValidationDTO request)
        {
            
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await fileTT2Services.ValidationTT2(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
