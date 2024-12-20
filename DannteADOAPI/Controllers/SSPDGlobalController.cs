using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;
using System.Drawing;
using Microsoft.Win32;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSPDGlobalController : Controller
    {
        readonly ISSPDGlobalServices SSPDServices;

        public SSPDGlobalController(ISSPDGlobalServices _sspdServices)
        {
            SSPDServices = _sspdServices;
        }

        /// <summary>
        /// Este endpoint busca archivos SSPD.csv ORIGINALES en la carpeta especificada, particionandolos en archivos _unchanged, _continuesInsert, _continuesUpdate, _continuesInvalid, _closed, _closedInvalid, _delete, _update
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDGlobalController.ReadFileSspdOrginal))]
        public async Task<IActionResult> ReadFileSspdOrginal()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadFileSspdOrginal(response);
                return Ok(response);
            });
        }

    }
}
