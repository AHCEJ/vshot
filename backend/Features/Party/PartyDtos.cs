namespace VirtualShot.Features.Party;

public sealed class ParticipantDto
{
    public int UserId { get; init; }

    public string Nickname { get; init; } = "";

    public string Emoji { get; init; } = "😎";

    public bool AudioOn { get; init; }

    public bool VideoOn { get; init; }

    public int SessionShots { get; init; }

    public bool IsHost { get; init; }
}

public sealed class ChatMessageDto
{
    public int UserId { get; init; }

    public string Nickname { get; init; } = "";

    public string Emoji { get; init; } = "";

    public string Text { get; init; } = "";

    public DateTime At { get; init; }
}

public sealed class ShotTakenDto
{
    public int UserId { get; init; }

    public string Nickname { get; init; } = "";

    public string Emoji { get; init; } = "";

    public string Drink { get; init; } = "";

    public int UserSessionShots { get; init; }

    public int RoomTotalShots { get; init; }
}

/// <summary>WebRTC signaling payload relayed between two peers through the hub.</summary>
public sealed class SignalPayload
{
    public string Kind { get; init; } = "";

    public string? Sdp { get; init; }

    public string? Candidate { get; init; }
}
