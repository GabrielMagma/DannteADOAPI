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
    public class FileAssetController : ControllerBase
    {
        readonly IFileAssetServices fileServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileAssetController(IFileAssetServices _fileServices, IHubContext<NotificationHub> hubContext)
        {
            fileServices = _fileServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma un archivo de datos CSV all_asset guardado en una ruta específica del programa y lo guarda en Base de datos
        /// en la tabla all_asset para essa, importante llenar los campos de UserId, year, month para el sistema de colas.
        /// </summary>
        /// <param name = "userId">id del usuario que registra el documento, por definir</param>
        /// <param name = "year">año a reportar, importante para sistema de colas</param>
        /// <param name = "month">mes a reportar, importante para sistema de colas</param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileAssetController.CreateFile))]        
        public async Task<IActionResult> CreateFile(AssetValidationDTO request)
        {            
            ResponseQuery<string> response = new ResponseQuery<string>();
            await fileServices.CreateFile(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}
