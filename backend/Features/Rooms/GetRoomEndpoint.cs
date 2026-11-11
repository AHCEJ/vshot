using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Services;

namespace VirtualShot.Features.Rooms;

public sealed class GetRoomEndpoint(AppDbContext db, PresenceService presence) : EndpointWithoutRequest<RoomDetailResponse>
{
    public override void Configure()
    {
        Get("api/rooms/{code}");
        Policies("authed");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var code = (Route<string>("code") ?? "").Trim().ToUpperInvariant();
        var room = await db.Rooms
            .Include(r => r.Host)
            .FirstOrDefaultAsync(r => r.Code == code, ct);

        if (room is null)
        {
            ThrowError("Lobby not found.", 404);
            return;
        }

        var response = room.Adapt<RoomDetailResponse>();
        response.Participants = presence.List(code);
        response.RoomTotalShots = presence.RoomShots(code);

        await Send.OkAsync(response, ct);
    }
}
