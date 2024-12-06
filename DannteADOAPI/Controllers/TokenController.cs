using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        readonly ITokenServices tokenServices;

        public TokenController(ITokenServices _tokenServices)
        {
            tokenServices = _tokenServices;
        }
        /// <summary>
        /// Servicio que genera token de acceso        
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TokenController.CreateToken))]        
        public async Task<IActionResult> CreateToken()
        {
            return await Task.Run(() =>
            {
                ResponseQuery<string> response = new ResponseQuery<string>();
                tokenServices.CreateToken(response);
                return Ok(response);
            });
        }
    }
}
