using Microsoft.AspNetCore.SignalR;

namespace DannteADOAPI.Helper
{
    public class NotificationHub : Hub
    {
        public async Task Send(bool Success, string Message)
        {
            await Clients.All.SendAsync("Receive", Success, Message);
        }
    }
}
