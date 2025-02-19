//using ADO.BL.Interfaces;
//using ADO.BL.Responses;
//using Microsoft.AspNetCore.Mvc;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FileTC1ValidationController : ControllerBase
//    {
//        readonly IFileTC1ValidationServices fileServices;

//        public FileTC1ValidationController(IFileTC1ValidationServices _fileServices)
//        {
//            fileServices = _fileServices;
//        }
//        /// <summary>
//        /// Servicio que toma el archivo de datos CSV TC1 y lo valida, generando archivo de registros correctos y archivo de errores
//        /// </summary>        
//        /// <returns></returns>
//        [HttpPost]
//        [Route(nameof(FileTC1ValidationController.ValidationTC1))]
//        public async Task<IActionResult> ValidationTC1(IFormFile file)
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<bool> response = new ResponseQuery<bool>();
//                fileServices.ValidationTC1(file, response);
//                return Ok(response);
//            });
//        }
//    }
//}
