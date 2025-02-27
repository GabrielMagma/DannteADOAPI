using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileTT2ValidationController : ControllerBase
    {
        readonly IFileTT2ValidationServices fileServices;

        public FileTT2ValidationController(IFileTT2ValidationServices _fileServices)
        {
            fileServices = _fileServices;
        }
        /// <summary>
        /// Servicio que toma el archivo de datos CSV TT2 y lo valida, generando archivo de registros correctos y archivo de errores
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileTT2ValidationController.ValidationTT2))]
        public async Task<IActionResult> ValidationTT2()
        {
            
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await fileServices.ValidationTT2(response);
            return Ok(response);
            
        }
    }
}
