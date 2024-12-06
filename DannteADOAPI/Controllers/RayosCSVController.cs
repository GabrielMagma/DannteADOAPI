using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RayosCSVController : ControllerBase
    {
        readonly IRayosCSVServices rayosServices;

        public RayosCSVController(IRayosCSVServices _rayosServices)
        {
            rayosServices = _rayosServices;
        }
        /// <summary>
        /// Servicio que toma los datos desde un archivo csv, los filtra, guarda en un csv y los almacena en la tabla mp_lightning de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(RayosCSVController.SearchDataCSV))]
        public async Task<IActionResult> SearchDataCSV()
        {
            return await Task.Run(() =>
            {
                ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
                rayosServices.SearchDataCSV(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Servicio que toma los datos desde un archivo xlsx, los filtra, guarda en un csv y los almacena en la tabla mp_lightning de la base de datos
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(RayosCSVController.SearchDataExcel))]
        public async Task<IActionResult> SearchDataExcel()
        {
            return await Task.Run(() =>
            {
                ResponseEntity<List<string>> response = new ResponseEntity<List<string>>();
                rayosServices.SearchDataExcel(response);
                return Ok(response);
            });
        }
    }
}