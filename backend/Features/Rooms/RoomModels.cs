using FastEndpoints;
using FluentValidation;
using VirtualShot.Features.Party;

namespace VirtualShot.Features.Rooms;

public sealed class CreateRoomRequest
{
    public string? Name { get; init; }
}

public sealed class CreateRoomRequestValidator : Validator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(64);
    }
}

public sealed class JoinRoomRequest
{
    public string Code { get; init; } = "";
}

public sealed class JoinRoomRequestValidator : Validator<JoinRoomRequest>
{
    public JoinRoomRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Enter the lobby number.")
            .Length(4, 8).WithMessage("Lobby numbers are 6 characters.");
    }
}

public sealed class RoomResponse
{
    public string Code { get; init; } = "";

    public string Name { get; init; } = "";

    public string HostNickname { get; init; } = "";
}

public sealed class RoomDetailResponse
{
    public string Code { get; init; } = "";

    public string Name { get; init; } = "";

    public string HostNickname { get; init; } = "";

    public List<ParticipantDto> Participants { get; set; } = [];

    public int RoomTotalShots { get; set; }
}
