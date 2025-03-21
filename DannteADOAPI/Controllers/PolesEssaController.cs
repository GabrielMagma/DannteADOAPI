﻿using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class PolesEssaController : ControllerBase
    {
        readonly IPolesEssaServices polesEssaServices;

        public PolesEssaController(IPolesEssaServices _polesEssaServices)
        {
            polesEssaServices = _polesEssaServices;
        }
        /// <summary>
        /// Servicio que toma el archivo de datos CSV LAC y lo valida, generando archivo de registros correctos y archivo de errores
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(PolesEssaController.ValidationFile))]        
        public async Task<IActionResult> ValidationFile(PolesValidationDTO request)
        {

            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await polesEssaServices.ValidationFile(request, response);
            return Ok(response);
            
        }
    }
}
