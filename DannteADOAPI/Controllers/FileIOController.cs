using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileIOController : ControllerBase
    {
        readonly IFileIOServices fileIOServices;

        public FileIOController(IFileIOServices _fileIOServices)
        {
            fileIOServices = _fileIOServices;
        }
        /// <summary>
        /// Servicio que toma el nombre de un archivo de datos CSV guardado en una ruta específica del programa, lo convierte al formato de datos requerido
        /// y lo guarda en Base de datos
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileIOController.UploadIO))]        
        public async Task<IActionResult> UploadIO(IOsValidationDTO iosValidation)
        {
            return await Task.Run(() =>
            {
                ResponseQuery<string> response = new ResponseQuery<string>();
                fileIOServices.UploadIO(iosValidation, response);
                return Ok(response);
            });
        }
    }
}
