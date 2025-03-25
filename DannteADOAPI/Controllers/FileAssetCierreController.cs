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
        /// Servicio de pruebas para el archivo antiguo de assets, no usar
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
