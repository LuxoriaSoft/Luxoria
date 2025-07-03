using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LuxAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinCollection(string collectionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, collectionId);
        }

        public async Task LeaveCollection(string collectionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, collectionId);
        }
    }
}
