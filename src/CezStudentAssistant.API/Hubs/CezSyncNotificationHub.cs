using CezStudentAssistant.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CezStudentAssistant.API.Hubs;

[Authorize]
public sealed class CezSyncNotificationHub : Hub<ICezSyncNotificationClient>
{
}
