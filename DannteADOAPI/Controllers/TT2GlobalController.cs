using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TT2GlobalController : Controller
    {
        readonly ITT2GlobalServices TT2Services;

        public TT2GlobalController(ITT2GlobalServices _tt2Services)
        {
            TT2Services = _tt2Services;
        }

        /// <summary>
        /// Genera un archivo *_completed.csv añadiendo code_sig basado en la tabla all_asset.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TT2GlobalController.CompleteTT2Originals))]
        public async Task<IActionResult> CompleteTT2Originals()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                TT2Services.CompleteTT2Originals(response);
                return Ok(response);
            });
        }
    }
}
