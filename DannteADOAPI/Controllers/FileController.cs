using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        readonly IFileServices fileServices;

        public FileController(IFileServices _fileServices)
        {
            fileServices = _fileServices;
        }
        /// <summary>
        /// Servicio que toma el nombre de un archivo de datos CSV guardado en una ruta específica del programa, lo convierte al formato de datos requerido
        /// y lo guarda en Base de datos
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileController.CreateFileCSV))]        
        public async Task<IActionResult> CreateFileCSV([FromBody] string name)
        {
            return await Task.Run(() =>
            {
                ResponseQuery<string> response = new ResponseQuery<string>();
                fileServices.CreateFileCSV(name, response);
                return Ok(response);
            });
        }
    }
}
