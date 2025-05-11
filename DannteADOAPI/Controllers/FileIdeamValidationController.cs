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
    public class FileIdeamValidationController : ControllerBase
    {
        readonly IFileIdeamValidationServices fileServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileIdeamValidationController(IFileIdeamValidationServices _fileServices, IHubContext<NotificationHub> hubContext)
        {
            fileServices = _fileServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma el nombre de un archivo de datos CSV guardado en una ruta específica del programa, lo convierte al formato de datos requerido
        /// y lo guarda en Base de datos
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileIdeamValidationController.CreateFileCSV))]        
        public async Task<IActionResult> CreateFileCSV(RayosValidationDTO request)
        {
            await AddMessage(true, "El archivo de Rayos empieza el proceso de validación");
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await fileServices.CreateFileCSV(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
