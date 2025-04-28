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
    public class AllAssetOracleController : ControllerBase
    {
        readonly IAllAssetOracleServices allAssetOracleServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AllAssetOracleController(IAllAssetOracleServices _AllAssetOracleServices, IHubContext<NotificationHub> hubContext)
        {
            allAssetOracleServices = _AllAssetOracleServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma los datos de las tablas Spard_Transfor, Spard_Switch y Spard_Recloser, los filtra y los almacena en 
        /// un archivo excel y lo pasa a procesamiento posterior
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(AllAssetOracleController.SearchData))]
        public async Task<IActionResult> SearchData()
        {
            
            ResponseEntity<List<AllAssetDTO>> response = new ResponseEntity<List<AllAssetDTO>>();
            await allAssetOracleServices.SearchData(response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }        
    }
}
