using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TT2Controller : Controller
    {
        readonly ITT2Services TT2Services;

        public TT2Controller(ITT2Services _tt2Services)
        {
            TT2Services = _tt2Services;
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TT2Controller.CompleteTT2Originals))]
        public async Task<IActionResult> CompleteTT2Originals()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                TT2Services.CompleteTT2Originals(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TT2Controller.CreateTT2Files))]
        public async Task<IActionResult> CreateTT2Files()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                TT2Services.CreateTT2Files(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TT2Controller.UpdateAllAssetByTT2))]
        public async Task<IActionResult> UpdateAllAssetByTT2()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                TT2Services.UpdateAllAssetByTT2(response);
                return Ok(response);
            });
        }
    }
}
