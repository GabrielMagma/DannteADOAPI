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
    public class RayosCSVController : ControllerBase
    {
        readonly IRayosCSVServices rayosServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public RayosCSVController(IRayosCSVServices _rayosServices, IHubContext<NotificationHub> hubContext)
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
        [Route(nameof(RayosCSVController.SearchDataCSV))]
        public async Task<IActionResult> SearchDataCSV(RayosValidationDTO request)
        {
            ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
            await rayosServices.SearchDataCSV(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
        }

        /// <summary>
        /// Servicio que toma los datos desde un archivo xlsx, los filtra, guarda en un csv y los almacena en la tabla mp_lightning de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(RayosCSVController.SearchDataExcel))]
        public async Task<IActionResult> SearchDataExcel(RayosValidationDTO request)
        {
            ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
            await rayosServices.SearchDataExcel(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}