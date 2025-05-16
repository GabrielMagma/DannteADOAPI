using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ADO.BL.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        //private readonly ITestServices _testMessage;
        private readonly IFileAssetValidationServices _assetValidation;
        private readonly IFileAssetProcessingServices _assetProcessing;
        private readonly TimeSpan horaEjecutar = new TimeSpan(20, 30, 0); // 4:00 PM
        public Worker(ILogger<Worker> logger,
            //ITestServices testMessage,
            IFileAssetValidationServices assetValidation,
            IFileAssetProcessingServices assetProcessing,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            //_testMessage = testMessage;
            _assetValidation = assetValidation;
            _assetProcessing = assetProcessing;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //await _hubContext.Clients.All.SendAsync("Receive", true, $"mensaje de prueba desde worker");
                //await _testMessage.SendTest();
                //await Task.Delay(1000, stoppingToken);
                
                // Ejecutar tarea
                _logger.LogInformation("Ejecutando tarea programada a las: {time}", DateTimeOffset.Now);

                var reloj = Stopwatch.StartNew();
                // Aquí va tu lógica                
                await _hubContext.Clients.All.SendAsync("Receive", true, $"Servicio de assets se está ejecutando");
                ResponseQuery<bool> response = new ResponseQuery<bool>();
                ResponseQuery<bool> responseProcessing = new ResponseQuery<bool>();
                var request = new FileAssetsValidationDTO()
                {
                    UserId = 0,
                    NombreArchivo = null
                };
                var respValidation = await _assetValidation.ReadFilesAssets(request, response);
                await _hubContext.Clients.All.SendAsync("Receive", true, $"{respValidation.Message}");
                var respProcessing = await _assetProcessing.ReadFilesAssets(request, responseProcessing);
                await _hubContext.Clients.All.SendAsync("Receive", true, $"{respProcessing.Message}");
                //await _testMessage.SendTest();
                //await Task.Delay(5000);

                reloj.Stop();

                DateTime ahora = DateTime.Now;
                DateTime siguienteEjecucion = DateTime.Today.Add(horaEjecutar);

                if (ahora > siguienteEjecucion)
                
                {
                    //siguienteEjecucion = ahora.AddSeconds(30);
                    siguienteEjecucion = siguienteEjecucion.AddDays(1);
                }

                TimeSpan delay = siguienteEjecucion.AddMilliseconds(-reloj.ElapsedMilliseconds) - ahora;

                // Esperar 1 minuto hasta la próxima ejecución
                await Task.Delay(delay, stoppingToken);

            }
        }

    }
}
