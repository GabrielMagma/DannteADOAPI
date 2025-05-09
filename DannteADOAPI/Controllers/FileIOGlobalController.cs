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
    public class FileIOGlobalController : ControllerBase
    {
        readonly IFileIOServices fileIOServices;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FileIOGlobalController(IFileIOServices _fileIOServices, IHubContext<NotificationHub> hubContext)
        {
            fileIOServices = _fileIOServices;
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        /// <summary>
        /// Servicio que toma un archivo de datos desde un archivo XLSX en las páginas 2 y 4, guardado en una ruta específica del programa 
        /// y lo guarda en Base de datos de testing en las tablas file_io y file_io_complete, importante llenar los campos de userId, year, month para el sistema de colas
        /// </summary>        
        /// <returns></returns>  
        [HttpPost]
        [Route(nameof(FileIOGlobalController.UploadIO))]        
        public async Task<IActionResult> UploadIO(IOsValidationDTO iosValidation)
        {
            
            ResponseQuery<string> response = new ResponseQuery<string>();
            await fileIOServices.UploadIO(iosValidation, response);
            await AddMessage(response.Success, response.Message);
            return Ok(response);
            
        }
    }
}
