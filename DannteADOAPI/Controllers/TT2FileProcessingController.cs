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
    public class TT2FileProcessingController : Controller
    {
        readonly ITT2FileProcessingServices TT2Services;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TT2FileProcessingController(ITT2FileProcessingServices _tt2Services, IHubContext<NotificationHub> hubContext)
        {
            TT2Services = _tt2Services;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Genera un archivo *_completed.csv añadiendo code_sig basado en la tabla all_asset.
        /// </summary>
        /// <param></param>
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(TT2FileProcessingController.ReadFilesTT2))]
        public async Task<IActionResult> ReadFilesTT2(TT2ValidationDTO request)
        {
            ResponseQuery<bool> response = new ResponseQuery<bool>();
            await AddMessage(true, "TT2 se está Procesando");
            await TT2Services.ReadFilesTT2(request, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);            
        }
    }
}
