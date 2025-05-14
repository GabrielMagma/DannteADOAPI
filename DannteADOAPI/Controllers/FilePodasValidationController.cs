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
    public class FilePodasValidationController : ControllerBase
    {
        readonly IFilePodasValidationServices podasServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FilePodasValidationController(IFilePodasValidationServices _podasServices, IHubContext<NotificationHub> hubContext)
        {
            podasServices = _podasServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        
        /// <summary>
        /// Servicio que toma los datos desde un archivo xlsx, lo valida y lo guarda en un archivo csv.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(FilePodasValidationController.ReadFilePodas))]
        public async Task<IActionResult> ReadFilePodas(PodasValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "El servicio de podas inicia el proceso de validación");
            await podasServices.ReadFilePodas(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}