using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;

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
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
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
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
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
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
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
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
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
