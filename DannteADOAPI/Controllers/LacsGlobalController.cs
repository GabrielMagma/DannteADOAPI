using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;
using System.Drawing;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LacsGlobalController : Controller
    {
        readonly ILacsGlobalServices lacsServices;

        public LacsGlobalController(ILacsGlobalServices _lacsServices)
        {
            lacsServices = _lacsServices;
        }

        /// <summary>
        /// Este endpoint busca archivos LAC.csv ORIGINALES en la carpeta especificada, particionandolos en archivos: _unchanged, _continues, _continuesInvalid, _closed, _closedInvalid
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsGlobalController.ReadFileLacOrginal))]
        public async Task<IActionResult> ReadFileLacOrginal()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                lacsServices.ReadFileLacOrginal(response);
                return Ok(response);
            });
        }
    }
}
