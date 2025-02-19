//using ADO.BL.Responses;
//using ADO.BL.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PruebaConexionController : Controller
//    {
//        /// <summary>
//        /// Este endpoint busca archivos LAC.csv ORIGINALES en la carpeta especificada, particionandolos en archivos: _unchanged, _continues, _continuesInvalid, _closed, _closedInvalid
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(PruebaConexionController.testConexion))]
//        public async Task<IActionResult> testConexion()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
//                response.Message = "todo ok";
//                return Ok(response);
//            });
//        }
//    }
//}
