//using ADO.BL.Interfaces;
//using ADO.BL.Responses;
//using Microsoft.AspNetCore.Mvc;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ExcelCSVCompensacionesEEPController : ControllerBase
//    {
//        readonly IExcelCSVCompensacionesEEPServices excelCSVCompensacionesEEPServices;

//        public ExcelCSVCompensacionesEEPController(IExcelCSVCompensacionesEEPServices _excelCSVCompensacionesEEPServices)
//        {
//            excelCSVCompensacionesEEPServices = _excelCSVCompensacionesEEPServices;
//        }
//        /// <summary>
//        /// Servicio que toma los archivos excel de compensaciones de una ruta y los convierte a csv en un formato en específico
//        /// </summary>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(ExcelCSVCompensacionesEEPController.Convert))]
//        public async Task<IActionResult> Convert()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<string> response = new ResponseQuery<string>();
//                excelCSVCompensacionesEEPServices.Convert(response);
//                return Ok(response);
//            });
//        }
//    }
//}
