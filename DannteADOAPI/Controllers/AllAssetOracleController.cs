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
        [Route(nameof(AllAssetOracleController.SearchData))]
        public async Task<IActionResult> SearchData(string table)
        {
            
            ResponseEntity<List<AllAssetDTO>> response = new ResponseEntity<List<AllAssetDTO>>();
            await allAssetOracleServices.SearchData(table, response);
            return Ok(response);
            
        }        
    }
}
