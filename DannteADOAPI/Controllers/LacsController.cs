using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;
using System.Drawing;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LacsController : Controller
    {
        readonly ILacsServices lacsServices;

        public LacsController(ILacsServices _lacsServices)
        {
            lacsServices = _lacsServices;
        }

        /// <summary>
        /// Este endpoint busca archivos LAC.csv ORIGINALES en la carpeta especificada, particionandolos en archivos: _unchanged, _continues, _continuesInvalid, _closed, _closedInvalid
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsController.ReadFileLacOrginal))]
        public async Task<IActionResult> ReadFileLacOrginal()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                lacsServices.ReadFileLacOrginal(response);
                return Ok(response);
            });
        }


        /// <summary>
        /// Este endpoint busca archivos _unchanged en la carpeta especificada cuyo información es referente a registros SIN ALTERACIONES que se deben ADICIONAR a los LACs con estado 1
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsController.ReadSspdUnchanged))]
        public async Task<IActionResult> ReadSspdUnchanged()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                lacsServices.ReadSspdUnchanged(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint busca archivos _continues.csv en la carpeta especificada cuyo información es referente a registros SIN CIERRE y valor provisional 23:59:59 con fecha de incio actual que se deben ADICIONAR a los LACs esperando CIERRE
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsController.ReadSSpdContinues))]
        public async Task<IActionResult> ReadSSpdContinues()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                lacsServices.ReadSSpdContinues(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint busca archivos _update.csv en la carpeta especificada cuyo información es referente a registros SIN INCIO que ACTUALIZAN a los LACs con CIERRE.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(LacsController.ReadSspdUpdate))]
        public async Task<IActionResult> ReadSspdUpdate()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                lacsServices.ReadSspdUpdate(response);
                return Ok(response);
            });
        }
    }
}
