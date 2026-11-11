using Mapster;
using VirtualShot.Features.Party;

namespace VirtualShot.Services;

/// <summary>
/// In-memory live presence for party rooms: who is connected right now,
/// their stream state and per-session shot counts. Backed by SignalR connections.
/// </summary>
public sealed class PresenceService
{
    private readonly object _gate = new();
    private readonly Dictionary<string, RoomPresence> _rooms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (string Code, int UserId)> _connections = new();

    public string? Join(string code, int roomId, int hostId, int userId, string nickname, string emoji, string connectionId)
    {
        lock (_gate)
        {
            var normalized = code.ToUpperInvariant();
            if (!_rooms.TryGetValue(normalized, out var room) || room.RoomId != roomId)
            {
                room = new RoomPresence { RoomId = roomId, HostUserId = hostId };
                _rooms[normalized] = room;
            }

            string? replaced = null;
            if (room.Participants.TryGetValue(userId, out var existing))
                replaced = existing.ConnectionId;

            room.Participants[userId] = new Participant
            {
                UserId = userId,
                Nickname = nickname,
                Emoji = emoji,
                ConnectionId = connectionId,
                AudioOn = existing?.AudioOn ?? true,
                VideoOn = existing?.VideoOn ?? true,
                SessionShots = existing?.SessionShots ?? 0,
                IsHost = userId == hostId,
            };

            if (replaced is not null
                && _connections.TryGetValue(replaced, out var previous)
                && previous.UserId == userId)
            {
                _connections.Remove(replaced);
            }

            _connections[connectionId] = (normalized, userId);
            return replaced;
        }
    }

    public bool IsIn(string code, int userId)
    {
        lock (_gate)
            return _rooms.TryGetValue(code, out var room) && room.Participants.ContainsKey(userId);
    }

    public string? ConnectionOf(string code, int userId)
    {
        lock (_gate)
            return _rooms.TryGetValue(code, out var room) && room.Participants.TryGetValue(userId, out var participant)
                ? participant.ConnectionId
                : null;
    }

    public ParticipantDto? Get(string code, int userId)
    {
        lock (_gate)
            return _rooms.TryGetValue(code, out var room) && room.Participants.TryGetValue(userId, out var participant)
                ? participant.Adapt<ParticipantDto>()
                : null;
    }

    public List<ParticipantDto> List(string code)
    {
        lock (_gate)
            return _rooms.TryGetValue(code, out var room)
                ? room.Participants.Values.Select(p => p.Adapt<ParticipantDto>()).ToList()
                : [];
    }

    public int RoomShots(string code)
    {
        lock (_gate)
            return _rooms.TryGetValue(code, out var room) ? room.TotalShots : 0;
    }

    /// <summary>Removes a connection (e.g. on disconnect). Returns the affected room code and user id.</summary>
    public (string? Code, int UserId) Leave(string connectionId)
    {
        lock (_gate)
        {
            if (!_connections.Remove(connectionId, out var entry))
                return (null, 0);

            if (_rooms.TryGetValue(entry.Code, out var room))
            {
                if (room.Participants.TryGetValue(entry.UserId, out var participant)
                    && participant.ConnectionId == connectionId)
                {
                    room.Participants.Remove(entry.UserId);
                }

                if (room.Participants.Count == 0)
                    _rooms.Remove(entry.Code);
            }

            return (entry.Code, entry.UserId);
        }
    }

    /// <summary>Explicit leave by user (LeaveRoom hub call).</summary>
    public bool Leave(int userId, string code)
    {
        lock (_gate)
        {
            if (!_rooms.TryGetValue(code, out var room)
                || !room.Participants.Remove(userId, out var participant))
            {
                return false;
            }

            _connections.Remove(participant.ConnectionId);
            if (room.Participants.Count == 0)
                _rooms.Remove(code);
            return true;
        }
    }

    public bool SetStreamState(string code, int userId, bool audioOn, bool videoOn)
    {
        lock (_gate)
        {
            if (!_rooms.TryGetValue(code, out var room)
                || !room.Participants.TryGetValue(userId, out var participant))
            {
                return false;
            }

            participant.AudioOn = audioOn;
            participant.VideoOn = videoOn;
            return true;
        }
    }

    public bool TryReact(string code, int userId)
    {
        lock (_gate)
        {
            if (!_rooms.TryGetValue(code, out var room)
                || !room.Participants.TryGetValue(userId, out var participant))
            {
                return false;
            }

            if (DateTime.UtcNow - participant.LastReactionAt < TimeSpan.FromMilliseconds(500))
                return false;

            participant.LastReactionAt = DateTime.UtcNow;
            return true;
        }
    }

    public bool TryStartRound(string code, int userId)
    {
        lock (_gate)
        {
            if (!_rooms.TryGetValue(code, out var room) || !room.Participants.ContainsKey(userId))
                return false;

            if (DateTime.UtcNow - room.LastRoundAt < TimeSpan.FromSeconds(5))
                return false;

            room.LastRoundAt = DateTime.UtcNow;
            return true;
        }
    }

    public (int UserShots, int RoomShots) AddShot(string code, int userId)
    {
        lock (_gate)
        {
            if (!_rooms.TryGetValue(code, out var room)
                || !room.Participants.TryGetValue(userId, out var participant))
            {
                return (0, 0);
            }

            participant.SessionShots++;
            room.TotalShots++;
            return (participant.SessionShots, room.TotalShots);
        }
    }
}

public sealed class RoomPresence
{
    public int RoomId { get; init; }

    public int HostUserId { get; init; }

    public Dictionary<int, Participant> Participants { get; } = new();

    public int TotalShots { get; set; }

    public DateTime LastRoundAt { get; set; }
}

public sealed class Participant
{
    public int UserId { get; init; }

    public string Nickname { get; init; } = "";

    public string Emoji { get; init; } = "😎";

    public string ConnectionId { get; set; } = "";

    public bool AudioOn { get; set; } = true;

    public bool VideoOn { get; set; } = true;

    public int SessionShots { get; set; }

    public bool IsHost { get; init; }

    public DateTime LastReactionAt { get; set; }
}
