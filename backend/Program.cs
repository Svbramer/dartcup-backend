using NHost.Core;
using NHost.Database;
using NHost.Services;
using NHost.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DartCupBackend.Services;
using DartCupBackend.Controllers;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add NHost services
builder.Services.AddNHost(
    nhostBuilder =>
    {
        nhostBuilder.AddAuthentication();
        nhostBuilder.AddDatabase();
        nhostBuilder.AddStorage();
    }
);

// Add services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();

// Add controllers
builder.Services.AddControllers();

// Add SignalR for realtime
builder.Services.AddSignalR();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

// Use NHost
app.UseNHost();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<DartCupHub>("/hub");

// Test endpoint
app.MapGet("/", () => "Dart Cup Backend is running!");

app.Run();

// SignalR Hub for realtime updates
public class DartCupHub : Hub
{
    public async Task SendMatchUpdate(string matchId, string message)
    {
        await Clients.All.SendAsync("MatchUpdated", new { MatchId = matchId, Message = message });
    }
    
    public async Task SendLeagueUpdate(string leagueId, string message)
    {
        await Clients.All.SendAsync("LeagueUpdated", new { LeagueId = leagueId, Message = message });
    }
    
    public async Task SendChatMessage(string userId, string userName, string message)
    {
        await Clients.All.SendAsync("ChatMessage", new { UserId = userId, UserName = userName, Message = message, Timestamp = DateTime.UtcNow });
    }

    public async Task JoinRoom(string code)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, code.ToUpperInvariant());
    }

    public async Task LeaveRoom(string code)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, code.ToUpperInvariant());
    }

    public async Task SendSignal(string code, string payload)
    {
        await Clients.OthersInGroup(code.ToUpperInvariant()).SendAsync("Signal", payload);
    }

    public async Task Notify(string code, string eventName, string payload)
    {
        await Clients.OthersInGroup(code.ToUpperInvariant()).SendAsync(eventName, payload);
    }
}
