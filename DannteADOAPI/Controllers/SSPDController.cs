//using Microsoft.AspNetCore.Mvc;
//using ADO.BL.Responses;
//using ADO.BL.Interfaces;
//using System.Drawing;
//using Microsoft.Win32;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class SSPDController : Controller
//    {
//        readonly ISSPDServices SSPDServices;

//        public SSPDController(ISSPDServices _sspdServices)
//        {
//            SSPDServices = _sspdServices;
//        }

//        /// <summary>
//        /// Este endpoint busca archivos SSPD.csv ORIGINALES en la carpeta especificada, particionandolos en archivos _unchanged, _continuesInsert, _continuesUpdate, _continuesInvalid, _closed, _closedInvalid, _delete, _update
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(SSPDController.ReadFileSspdOrginal))]
//        public async Task<IActionResult> ReadFileSspdOrginal()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                SSPDServices.ReadFileSspdOrginal(response);
//                return Ok(response);
//            });
//        }


//        /// <summary>
//        /// Este endpoint busca archivos _unchanged.csv en la carpeta especificada cuyo información es referente a registros SIN ALTERACIONES que se deben ADICIONAR a los LACs con estado 1.
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(SSPDController.ReadSspdUnchanged))]
//        public async Task<IActionResult> ReadSspdUnchanged()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                SSPDServices.ReadSspdUnchanged(response);
//                return Ok(response);
//            });
//        }

//        /// <summary>
//        /// Este endpoint busca archivos *_continues.csv en la carpeta especificada cuyo información es referente a registros SIN CIERRE y valor provisional 23:59:59 con fecha de fin de mes actual que se deben ADICIONAR a los LACs con estado 1.
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(SSPDController.ReadSSpdContinuesInsert))]
//        public async Task<IActionResult> ReadSSpdContinuesInsert()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                SSPDServices.ReadSSpdContinuesInsert(response);
//                return Ok(response);
//            });
//        }

//        /// <summary>
//        /// Este endpoint busca archivos _continues.csv en la carpeta especificada cuyo información es referente a registros SIN CIERRE y valor provisional 23:59:59 con fecha de fin de mes actual que se deben ACTUALIZAR a los LACs con estado 2.
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(SSPDController.ReadSSpdContinuesUpdate))]
//        public async Task<IActionResult> ReadSSpdContinuesUpdate()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                SSPDServices.ReadSSpdContinuesUpdate(response);
//                return Ok(response);
//            });
//        }

//        /// <summary>
//        /// Este endpoint busca archivos _update.csv en la carpeta especificada cuyo información es referente a registros SIN ALTERACIONES que ACTUALIZAN a los LACs con estado 2.
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(SSPDController.ReadSspdUpdate))]
//        public async Task<IActionResult> ReadSspdUpdate()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                SSPDServices.ReadSspdUpdate(response);
//                return Ok(response);
//            });
//        }

//        /// <summary>
//        /// Este endpoint busca archivos delete.csv en la carpeta especificada cuyo información es referente a registros con estado 3 que se deben ELIMINAR de los LACs
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(SSPDController.ReadSspdDelete))]
//        public async Task<IActionResult> ReadSspdDelete()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                SSPDServices.ReadSspdDelete(response);
//                return Ok(response);
//            });
//        }
//    }
//}
