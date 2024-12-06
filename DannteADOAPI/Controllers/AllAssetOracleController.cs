using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllAssetOracleController : ControllerBase
    {
        readonly IAllAssetOracleServices allAssetOracleServices;

        public AllAssetOracleController(IAllAssetOracleServices _AllAssetOracleServices)
        {
            allAssetOracleServices = _AllAssetOracleServices;
        }
        /// <summary>
        /// Servicio que toma los datos de la tabla Spard_Transfor, los filtra y los almacena en la tabla All_Asset de la base de datos de Pereira
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(AllAssetOracleController.SearchDataTransfor))]
        public async Task<IActionResult> SearchDataTransfor()
        {
            return await Task.Run(() =>
            {
                ResponseEntity<List<AllAssetDTO>> response = new ResponseEntity<List<AllAssetDTO>>();
                allAssetOracleServices.SearchDataTransfor(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Servicio que toma los datos de la tabla Spard_Switch, los filtra y los almacena en la tabla All_Asset de la base de datos de Pereira
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(AllAssetOracleController.SearchDataSwitch))]
        public async Task<IActionResult> SearchDataSwitch()
        {
            return await Task.Run(() =>
            {
                ResponseEntity<List<AllAssetDTO>> response = new ResponseEntity<List<AllAssetDTO>>();
                allAssetOracleServices.SearchDataSwitch(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Servicio que toma los datos de la tabla Spard_Recloser, los filtra y los almacena en la tabla All_Asset de la base de datos de Pereira
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(AllAssetOracleController.SearchDataRecloser))]
        public async Task<IActionResult> SearchDataRecloser()
        {
            return await Task.Run(() =>
            {
                ResponseEntity<List<AllAssetDTO>> response = new ResponseEntity<List<AllAssetDTO>>();
                allAssetOracleServices.SearchDataRecloser(response);
                return Ok(response);
            });
        }
    }
}
