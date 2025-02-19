//using ADO.BL.Interfaces;
//using ADO.BL.Responses;
//using Microsoft.AspNetCore.Mvc;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ExcelCSVCompensacionesESSAController : ControllerBase
//    {
//        readonly IExcelCSVCompensacionesESSAServices excelCSVCompensacionesESSAServices;

//        public ExcelCSVCompensacionesESSAController(IExcelCSVCompensacionesESSAServices _excelCSVCompensacionesESSAServices)
//        {
//            excelCSVCompensacionesESSAServices = _excelCSVCompensacionesESSAServices;
//        }
//        /// <summary>
//        /// Servicio que toma los archivos excel de compensaciones de una ruta y los convierte a csv en un formato en específico
//        /// </summary>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(ExcelCSVCompensacionesESSAController.Convert))]
//        public async Task<IActionResult> Convert()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<string> response = new ResponseQuery<string>();
//                excelCSVCompensacionesESSAServices.Convert(response);
//                return Ok(response);
//            });
//        }
//    }
//}
