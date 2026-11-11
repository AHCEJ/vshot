using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Entities;
using VirtualShot.Services;

namespace VirtualShot.Features.Rooms;

/// <summary>Creates a party lobby and returns its lobby number (share code).</summary>
public sealed class CreateRoomEndpoint(AppDbContext db) : Endpoint<CreateRoomRequest, RoomResponse>
{
    public override void Configure()
    {
        Post("api/rooms");
        Policies("authed");
    }

    public override async Task HandleAsync(CreateRoomRequest req, CancellationToken ct)
    {
        var hostId = User.GetUserId();
        var hostNickname = User.Identity?.Name ?? "host";
        var name = string.IsNullOrWhiteSpace(req.Name)
            ? $"{hostNickname}'s party"
            : req.Name.Trim();

        var code = RoomCodeGenerator.Generate();
        for (var attempt = 0; await db.Rooms.AnyAsync(r => r.Code == code, ct); attempt++)
        {
            if (attempt >= 5)
            {
                ThrowError("Could not generate a unique lobby number. Please try again.");
                return;
            }
            code = RoomCodeGenerator.Generate();
        }

        var room = new Room { Code = code, Name = name, HostId = hostId };
        room.Members.Add(new RoomMember { UserId = hostId });
        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);

        await db.Entry(room).Reference(r => r.Host).LoadAsync(ct);

        await Send.OkAsync(room.Adapt<RoomResponse>(), ct);
    }
}
