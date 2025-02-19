using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class PolesEepController : ControllerBase
    {
        readonly IPolesEepServices polesEepServices;

        public PolesEepController(IPolesEepServices _polesEepServices)
        {
            polesEepServices = _polesEepServices;
        }
        /// <summary>
        /// Servicio que toma el archivo de datos CSV LAC y lo valida, generando archivo de registros correctos y archivo de errores
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(PolesEepController.ValidationFile))]        
        public async Task<IActionResult> ValidationFile()
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await polesEepServices.ValidationFile(response);
            return Ok(response);
            
        }
    }
}
