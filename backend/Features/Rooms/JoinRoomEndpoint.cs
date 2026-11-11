using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Entities;
using VirtualShot.Services;

namespace VirtualShot.Features.Rooms;

public sealed class JoinRoomEndpoint(AppDbContext db) : Endpoint<JoinRoomRequest, RoomResponse>
{
    public override void Configure()
    {
        Post("api/rooms/join");
        Policies("authed");
    }

    public override async Task HandleAsync(JoinRoomRequest req, CancellationToken ct)
    {
        var code = req.Code.Trim().ToUpperInvariant();
        var room = await db.Rooms
            .Include(r => r.Host)
            .FirstOrDefaultAsync(r => r.Code == code, ct);

        if (room is null)
        {
            ThrowError("Lobby not found. Double-check the lobby number.", 404);
            return;
        }

        var userId = User.GetUserId();
        if (!await db.RoomMembers.AnyAsync(m => m.RoomId == room.Id && m.UserId == userId, ct))
        {
            db.RoomMembers.Add(new RoomMember { RoomId = room.Id, UserId = userId });
            await db.SaveChangesAsync(ct);
        }

        await Send.OkAsync(room.Adapt<RoomResponse>(), ct);
    }
}
