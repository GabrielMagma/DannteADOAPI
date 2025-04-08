using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using DannteADOAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileAssetValidationController : ControllerBase
    {
        readonly IFileAssetValidationServices fileAssetValidationServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileAssetValidationController(IFileAssetValidationServices _fileAssetValidationServices, IHubContext<NotificationHub> hubContext)
        {
            fileAssetValidationServices = _fileAssetValidationServices;
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
        [Route(nameof(FileAssetValidationController.UploadFile))]        
        public async Task<IActionResult> UploadFile(FileAssetsValidationDTO request)
        {
            ResponseQuery<string> response = new ResponseQuery<string>();
            await fileAssetValidationServices.UploadFile(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}
