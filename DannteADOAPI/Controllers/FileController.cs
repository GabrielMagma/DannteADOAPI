using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        readonly IFileServices fileServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileController(IFileServices _fileServices, IHubContext<NotificationHub> hubContext)
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
        /// <param name="name">Nombre del archivo</param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileController.CreateFileCSV))]        
        public async Task<IActionResult> CreateFileCSV([FromBody] string name)
        {           
            ResponseQuery<string> response = new ResponseQuery<string>();
            await fileServices.CreateFileCSV(name, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
