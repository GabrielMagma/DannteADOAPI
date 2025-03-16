using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileAssetController : ControllerBase
    {
        readonly IFileAssetServices fileServices;

        public FileAssetController(IFileAssetServices _fileServices)
        {
            fileServices = _fileServices;
        }
        /// <summary>
        /// Servicio que toma el nombre de un archivo de datos CSV all_asset guardado en una ruta específica del programa y lo guarda en Base de datos
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileAssetController.CreateFile))]        
        public async Task<IActionResult> CreateFile(AssetValidationDTO request)
        {            
                ResponseQuery<string> response = new ResponseQuery<string>();
                await fileServices.CreateFile(request, response);
                return Ok(response);            
        }
    }
}
