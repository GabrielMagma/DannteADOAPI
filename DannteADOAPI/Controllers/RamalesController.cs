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
    public class RamalesController : ControllerBase
    {
        readonly IRamalesServices ramalesServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public RamalesController(IRamalesServices _ramalesServices, IHubContext<NotificationHub> hubContext)
        {
            ramalesServices = _ramalesServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma los datos desde un archivo csv, los filtra y los almacena en la tabla file_io_temp y file_io_temp_detail de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(RamalesController.SearchData))]
        public async Task<IActionResult> SearchData(RamalesValidationDTO request)
        {

            ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
            await ramalesServices.SearchData(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);        
        }
       
    }
}