using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RamalesController : ControllerBase
    {
        readonly IRamalesServices ramalesServices;

        public RamalesController(IRamalesServices _ramalesServices)
        {
            ramalesServices = _ramalesServices;
        }
        /// <summary>
        /// Servicio que toma los datos desde un archivo csv, los filtra, guarda en un csv y los almacena en la tabla file_io_temp y file_io_temp_detail de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(RamalesController.SearchData))]
        public async Task<IActionResult> SearchData()
        {
            return await Task.Run(() =>
            {
                ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
                ramalesServices.SearchData(response);
                return Ok(response);
            });
        }
       
    }
}