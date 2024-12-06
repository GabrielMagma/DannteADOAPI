using Microsoft.AspNetCore.Mvc;
using ADO.BL.Responses;
using ADO.BL.Interfaces;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSPDController : Controller
    {
        readonly ISSPDServices SSPDServices;

        public SSPDController(ISSPDServices _sspdServices)
        {
            SSPDServices = _sspdServices;
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDController.ReadFileSspdOrginal))]
        public async Task<IActionResult> ReadFileSspdOrginal()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadFileSspdOrginal(response);
                return Ok(response);
            });
        }


        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDController.ReadSspdUnchanged))]
        public async Task<IActionResult> ReadSspdUnchanged()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadSspdUnchanged(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDController.ReadSSpdContinuesInsert))]
        public async Task<IActionResult> ReadSSpdContinuesInsert()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadSSpdContinuesInsert(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDController.ReadSSpdContinuesUpdate))]
        public async Task<IActionResult> ReadSSpdContinuesUpdate()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadSSpdContinuesUpdate(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDController.ReadSspdUpdate))]
        public async Task<IActionResult> ReadSspdUpdate()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadSspdUpdate(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Este endpoint procesa archivos *.csv y realiza operaciones de inserción o actualización en la base de datos.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDController.ReadSspdDelete))]
        public async Task<IActionResult> ReadSspdDelete()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
                SSPDServices.ReadSspdDelete(response);
                return Ok(response);
            });
        }
    }
}
