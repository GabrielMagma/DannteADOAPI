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
    public class FileRayosProcessingController : ControllerBase
    {
        readonly IFileRayosProcessingServices rayosServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileRayosProcessingController(IFileRayosProcessingServices _rayosServices, IHubContext<NotificationHub> hubContext)
        {
            rayosServices = _rayosServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma los datos desde un archivo csv, los filtra, guarda en un csv y los almacena en la tabla mp_lightning de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileRayosValidationController.ReadFilesRayos))]
        public async Task<IActionResult> ReadFilesRayos(RayosValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "Los archivos de rayos empiezan proceso de guardado de registros");
            await rayosServices.ReadFilesRayos(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
        }
        
    }
}