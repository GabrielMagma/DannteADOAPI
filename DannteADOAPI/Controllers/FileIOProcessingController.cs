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
    public class FileIOProcessingController : ControllerBase
    {
        readonly IFileIOProcessingServices fileIOServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileIOProcessingController(IFileIOProcessingServices _fileIOServices, IHubContext<NotificationHub> hubContext)
        {
            fileIOServices = _fileIOServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma un archivo de datos desde un archivo XLSX en las páginas 2 y 4, guardado en una ruta específica del programa 
        /// y lo guarda en Base de datos de testing en las tablas file_io y file_io_complete, importante llenar los campos de userId, year, month para el sistema de colas
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileIOProcessingController.ReadFilesIos))]        
        public async Task<IActionResult> ReadFilesIos(IOsValidationDTO iosValidation)
        {
            
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "IOs se está procesando");
            await fileIOServices.ReadFilesIos(iosValidation, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
