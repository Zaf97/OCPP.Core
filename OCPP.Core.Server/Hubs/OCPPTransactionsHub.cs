using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace OCPP.Core.Server.Hubs
{
    public class OCPPTransactionsHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}