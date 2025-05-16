using ADO.BL.Helper;
using ADO.BL.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace ADO.BL.Services
{
    public class TestServices : ITestServices
    {

        private readonly IHubContext<NotificationHub> _hubContext;
        public TestServices(IConfiguration configuration,
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;

        }

        public async Task<bool> SendTest()
        {
            await _hubContext.Clients.All.SendAsync("Receive", true, $"mensaje de prueba desde servicio injectado");
            return true;
        }
    }
}
