using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TC1Controller : Controller
    {
        readonly ITC1Services TC1Services;

        public TC1Controller(ITC1Services _tc1Services)
        {
            TC1Services = _tc1Services;
        }

        /// <summary>
        /// Lee y carga los archivos TC1 para el año y mes proporcionados.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TC1Controller.ReadAssets))]
        public async Task<IActionResult> ReadAssets()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                TC1Services.ReadAssets(response);
                return Ok(response);
            });
        }
    }
}
