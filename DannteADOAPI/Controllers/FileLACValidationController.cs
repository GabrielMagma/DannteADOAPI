//using ADO.BL.Interfaces;
//using ADO.BL.Responses;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace DannteADOAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class FileLACValidationController : ControllerBase
//    {
//        readonly IFileLACValidationServices fileServices;

//        public FileLACValidationController(IFileLACValidationServices _fileServices)
//        {
//            fileServices = _fileServices;
//        }
//        /// <summary>
//        /// Servicio que toma el archivo de datos CSV LAC y lo valida, generando archivo de registros correctos y archivo de errores
//        /// </summary>        
//        /// <returns></returns>  
//        [HttpPost]
//        [Route(nameof(FileLACValidationController.ValidationLAC))]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> ValidationLAC(IFormFile file)
//        {
//            return await Task.Run(() =>
//            {
//                ResponseQuery<bool> response = new ResponseQuery<bool>();
//                fileServices.ValidationLAC(file, response);
//                return Ok(response);
//            });
//        }
//    }
//}
