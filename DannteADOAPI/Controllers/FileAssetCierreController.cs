using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileAssetCierreController : ControllerBase
    {
        readonly IFileAssetCierreServices fileAssetCierreServices;

        public FileAssetCierreController(IFileAssetCierreServices _fileAssetCierreServices)
        {
            fileAssetCierreServices = _fileAssetCierreServices;
        }
        /// <summary>
        /// Servicio que toma el nombre de un archivo de datos CSV guardado en una ruta específica del programa, lo convierte al formato de datos requerido
        /// y lo guarda en Base de datos
        /// </summary>        
        /// <returns></returns> 
        /// 
        [HttpPost]
        [Route(nameof(FileAssetCierreController.UploadFile))]        
        public async Task<IActionResult> UploadFile(FileAssetsValidationDTO request)
        {
            ResponseQuery<string> response = new ResponseQuery<string>();
            await fileAssetCierreServices.UploadFile(request, response);
            return Ok(response);            
        }
    }
}
