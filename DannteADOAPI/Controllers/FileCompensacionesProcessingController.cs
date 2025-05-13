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
    public class FileCompensacionesProcessingController : ControllerBase
    {
        readonly IFileCompensacionesProcessingServices compServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileCompensacionesProcessingController(IFileCompensacionesProcessingServices _compServices, IHubContext<NotificationHub> hubContext)
        {
            compServices = _compServices;
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
        [Route(nameof(FileCompensacionesProcessingController.ReadFilesComp))]        
        public async Task<IActionResult> ReadFilesComp(CompsValidationDTO request)
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "El procesado de registros de Compensaciones se está ejecutando");
            await compServices.ReadFilesComp(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
