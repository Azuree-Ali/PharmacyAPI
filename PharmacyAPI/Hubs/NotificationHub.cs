using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PharmacyAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
