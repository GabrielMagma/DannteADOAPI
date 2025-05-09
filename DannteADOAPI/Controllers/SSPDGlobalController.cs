﻿using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSPDGlobalController : Controller
    {
        readonly ISSPDGlobalServices SSPDServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public SSPDGlobalController(ISSPDGlobalServices _sspdServices, IHubContext<NotificationHub> hubContext)
        {
            SSPDServices = _sspdServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Este endpoint busca archivos SSPD.csv ORIGINALES en la carpeta especificada, particionandolos en archivos _unchanged, _continuesInsert, _continuesUpdate, _continuesInvalid, _closed, _closedInvalid, _delete, _update
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(SSPDGlobalController.ReadFileSspdOrginal))]
        public async Task<IActionResult> ReadFileSspdOrginal(LacValidationDTO request)
        {
            ResponseQuery<List<string>> response = new ResponseQuery<List<string>>();
            await SSPDServices.ReadFileSspdOrginal(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }

    }
}
