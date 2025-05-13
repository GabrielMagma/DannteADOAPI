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
    public class FileCompensacionesValidationController : ControllerBase
    {
        readonly IFileCompensacionesValidationServices fileCompServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileCompensacionesValidationController(IFileCompensacionesValidationServices _fileCompServices, IHubContext<NotificationHub> hubContext)
        {
            fileCompServices = _fileCompServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma un archivo de datos desde un archivo XLSX de compensaciones y lo valida
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileCompensacionesValidationController.ReadFilesComp))]        
        public async Task<IActionResult> ReadFilesComp(CompsValidationDTO request)
        {
            
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "Compensaciones se está Validando");
            await fileCompServices.ReadFilesComp(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
