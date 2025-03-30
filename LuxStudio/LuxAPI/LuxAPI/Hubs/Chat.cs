using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LuxAPI.Hubs
{
public class ChatHub : Hub
{
    public async Task SendMessage(string collectionId, string senderEmail, string message)
    {
        //share message to all clients in the group
        await Clients.Group(collectionId).SendAsync("ReceiveMessage", senderEmail, message);
    }

    public async Task JoinCollection(string collectionId)
    {
        // user join a group based on the collection ID
        await Groups.AddToGroupAsync(Context.ConnectionId, collectionId);
    }

    public async Task LeaveCollection(string collectionId)
    {
        // user leave a group based on the collection ID
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, collectionId);
    }
}
}
