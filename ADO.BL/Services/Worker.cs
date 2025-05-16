using ADO.BL.Helper;
using ADO.BL.Interfaces;
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
        private readonly ITestServices _testMessage;
        private readonly TimeSpan horaBandera = new TimeSpan(15, 24, 0); // 4:00 PM
        private readonly TimeSpan horaEjecutar = new TimeSpan(15, 43, 0); // 4:00 PM
        public Worker(ILogger<Worker> logger,
            ITestServices testMessage,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _testMessage = testMessage;
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
                await _hubContext.Clients.All.SendAsync("Receive", true, $"mensaje de prueba desde worker");
                await _testMessage.SendTest();
                await Task.Delay(5000);

                reloj.Stop();

                DateTime ahora = DateTime.Now;
                //DateTime ahora = DateTime.Today.Add(horaBandera);
                DateTime siguienteEjecucion = DateTime.Today.Add(horaEjecutar);

                if (ahora > siguienteEjecucion)
                //siguienteEjecucion = siguienteEjecucion.AddDays(1);
                {
                    //ahora = ahora.AddSeconds(30);
                    siguienteEjecucion = ahora.AddSeconds(30);
                }

                TimeSpan delay = siguienteEjecucion.AddMilliseconds(-reloj.ElapsedMilliseconds) - ahora;

                // Esperar 1 minuto hasta la próxima ejecución
                await Task.Delay(delay, stoppingToken);

            }
        }

    }
}
