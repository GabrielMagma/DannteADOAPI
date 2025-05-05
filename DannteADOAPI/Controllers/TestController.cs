using DannteADOAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Controllers
{
    public class TestController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public TestController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private async Task AddMessage(bool success, string message)
        {
            await _hubContext.Clients.All.SendAsync("Receive", success, message);
        }

        [HttpPost]
        [Route(nameof(TestController.notiHub))]
        public async Task<IActionResult> notiHub()
        {
            await AddMessage(true, "Mensaje de prueba para  segundo api");
            return Ok();

        }
    }
}
