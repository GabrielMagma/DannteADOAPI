//using ADO.BL.Interfaces;
//using ADO.BL.Responses;
//using Microsoft.AspNetCore.Mvc;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ExcelCSVController : ControllerBase
//    {
//        readonly IExcelCSVServices excelCSVServices;

//        public ExcelCSVController(IExcelCSVServices _ExcelCSVServices)
//        {
//            excelCSVServices = _ExcelCSVServices;
//        }
//        /// <summary>
//        /// Servicio que toma los datos de un archivo excel y los convierte a csv
//        /// </summary>
//        /// <param></param>
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(ExcelCSVController.ProcessXlsx))]        
//        public async Task<IActionResult> ProcessXlsx()
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<string> response = new ResponseQuery<string>();
//                excelCSVServices.ProcessXlsx(response);
//                return Ok(response);
//            });
//        }
//    }
//}
