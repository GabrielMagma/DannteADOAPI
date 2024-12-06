using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : Controller
    {
        readonly IAssetsServices assetsServices;

        public AssetsController(IAssetsServices _assetsServices)
        {
            assetsServices = _assetsServices;
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(AssetsController.ReadAssets))]
        public async Task<IActionResult> ReadAssets()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                assetsServices.ReadAssets(response);
                return Ok(response);
            });
        }

    }
}
