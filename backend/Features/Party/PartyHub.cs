using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Entities;
using VirtualShot.Services;

namespace VirtualShot.Features.Party;

/// <summary>
/// Real-time party hub: presence, chat, reactions, synchronized shot rounds,
/// shot logging and WebRTC signaling relay for the video mesh.
/// </summary>
[Authorize(Policy = "authed")]
public sealed class PartyHub(AppDbContext db, PresenceService presence) : Hub
{
    private static string GroupName(string code) => $"room:{code.ToUpperInvariant()}";

    private int UserId => Context.User.GetUserId();

    private string Nickname => Context.User?.Identity?.Name ?? "guest";

    private string Emoji => Context.User.GetEmoji();

    public async Task JoinRoom(string code)
    {
        var normalized = Normalize(code);
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Code == normalized, Context.ConnectionAborted);
        if (room is null)
        {
            await Clients.Caller.SendAsync("RoomError", "Lobby not found.", Context.ConnectionAborted);
            return;
        }

        var replacedConnection = presence.Join(
            normalized, room.Id, room.HostId, UserId, Nickname, Emoji, Context.ConnectionId);

        if (replacedConnection is not null)
            await Clients.Client(replacedConnection).SendAsync("SessionReplaced", Context.ConnectionAborted);

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(normalized), Context.ConnectionAborted);

        var me = presence.Get(normalized, UserId);
        if (me is null)
            return;

        await Clients.Caller.SendAsync("RoomState", presence.List(normalized), presence.RoomShots(normalized), Context.ConnectionAborted);
        await Clients.OthersInGroup(GroupName(normalized)).SendAsync("ParticipantJoined", me, Context.ConnectionAborted);
    }

    public async Task LeaveRoom(string code)
    {
        var normalized = Normalize(code);
        if (!presence.Leave(UserId, normalized))
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(normalized));
        await Clients.Group(GroupName(normalized)).SendAsync("ParticipantLeft", UserId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var (code, userId) = presence.Leave(Context.ConnectionId);
        if (code is not null)
            await Clients.Group(GroupName(code)).SendAsync("ParticipantLeft", userId);

        await base.OnDisconnectedAsync(exception);
    }

    public Task SendChat(string code, string text)
    {
        var normalized = Normalize(code);
        if (!presence.IsIn(normalized, UserId))
            return Task.CompletedTask;

        var message = (text ?? "").Trim();
        if (message.Length == 0)
            return Task.CompletedTask;
        if (message.Length > 500)
            message = message[..500];

        var dto = new ChatMessageDto
        {
            UserId = UserId,
            Nickname = Nickname,
            Emoji = Emoji,
            Text = message,
            At = DateTime.UtcNow,
        };
        return Clients.Group(GroupName(normalized)).SendAsync("ChatMessage", dto);
    }

    public Task SetStreamState(string code, bool audioOn, bool videoOn)
    {
        var normalized = Normalize(code);
        if (!presence.SetStreamState(normalized, UserId, audioOn, videoOn))
            return Task.CompletedTask;

        return Clients.OthersInGroup(GroupName(normalized))
            .SendAsync("StreamStateChanged", UserId, audioOn, videoOn);
    }

    public Task SendReaction(string code, string emoji)
    {
        var normalized = Normalize(code);
        if (!presence.TryReact(normalized, UserId))
            return Task.CompletedTask;

        var reaction = (emoji ?? "").Trim();
        if (reaction.Length is < 1 or > 8)
            return Task.CompletedTask;

        return Clients.Group(GroupName(normalized)).SendAsync("Reaction", UserId, reaction);
    }

    public Task SendSignal(string code, int targetUserId, SignalPayload payload)
    {
        var normalized = Normalize(code);
        if (payload is null)
            return Task.CompletedTask;

        var targetConnection = presence.ConnectionOf(normalized, targetUserId);
        if (targetConnection is null)
            return Task.CompletedTask;

        if (payload.Sdp?.Length > 200_000 || payload.Candidate?.Length > 10_000)
            return Task.CompletedTask;

        return Clients.Client(targetConnection).SendAsync("SignalReceived", UserId, payload);
    }

    public async Task StartRound(string code, string drink)
    {
        var normalized = Normalize(code);
        if (!presence.TryStartRound(normalized, UserId))
            return;

        await Clients.Group(GroupName(normalized))
            .SendAsync("RoundStarted", SanitizeDrink(drink), Nickname);
    }

    public async Task TakeShot(string code, string drink)
    {
        var normalized = Normalize(code);
        if (!presence.IsIn(normalized, UserId))
            return;

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Code == normalized, Context.ConnectionAborted);
        if (room is null)
            return;

        var user = await db.Users.FindAsync([UserId], Context.ConnectionAborted);
        if (user is null)
            return;

        var safeDrink = SanitizeDrink(drink);
        db.Shots.Add(new ShotEntry { RoomId = room.Id, UserId = UserId, Drink = safeDrink });
        user.LifetimeShots++;
        await db.SaveChangesAsync(Context.ConnectionAborted);

        var (userShots, roomShots) = presence.AddShot(normalized, UserId);
        var dto = new ShotTakenDto
        {
            UserId = UserId,
            Nickname = Nickname,
            Emoji = Emoji,
            Drink = safeDrink,
            UserSessionShots = userShots,
            RoomTotalShots = roomShots,
        };
        await Clients.Group(GroupName(normalized)).SendAsync("ShotTaken", dto);
    }

    private static string Normalize(string? code)
        => (code ?? "").Trim().ToUpperInvariant();

    private static string SanitizeDrink(string? drink)
    {
        var trimmed = (drink ?? "").Trim();
        if (trimmed.Length == 0)
            return "cocktail";
        return trimmed.Length > 16 ? trimmed[..16] : trimmed;
    }
}
