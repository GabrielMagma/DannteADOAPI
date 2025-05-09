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
    public class FileAssetProcessingController : ControllerBase
    {
        readonly IFileAssetProcessingServices fileAssetProcessingServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileAssetProcessingController(IFileAssetProcessingServices _fileAssetProcessingServices, IHubContext<NotificationHub> hubContext)
        {
            fileAssetProcessingServices = _fileAssetProcessingServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma un archivo de datos CSV guardado en una ruta específica del programa y lo guarda en Base de datos essa en la 
        /// tabla all_asset, importante llenar los campos userId, year, month para el sistema de colas y para la asignación de fecha de ingreso del asset
        /// </summary>        
        /// <returns></returns> 
        /// 
        [HttpPost]
        [Route(nameof(FileAssetProcessingController.ReadFilesAssets))]        
        public async Task<IActionResult> ReadFilesAssets(FileAssetsValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "Assest se está Procesando");
            await fileAssetProcessingServices.ReadFilesAssets(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}
