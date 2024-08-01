using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendImage(string user, string imageUrl)
    {
        await Clients.All.SendAsync("ReceiveImage", user, imageUrl);
    }
} 
