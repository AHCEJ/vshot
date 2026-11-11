using FastEndpoints;
using FluentValidation;

namespace VirtualShot.Features.Identity;

public sealed class GuestRequest
{
    public string Nickname { get; init; } = "";

    public string? Emoji { get; init; }
}

public sealed class GuestRequestValidator : Validator<GuestRequest>
{
    public GuestRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Pick a nickname first.")
            .MinimumLength(2).WithMessage("Nickname needs at least 2 characters.")
            .MaximumLength(32);

        RuleFor(x => x.Emoji)
            .MaximumLength(8);
    }
}

public sealed class UpdateProfileRequest
{
    public string Nickname { get; init; } = "";

    public string Emoji { get; init; } = "";
}

public sealed class UpdateProfileRequestValidator : Validator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Pick a nickname first.")
            .MinimumLength(2).MaximumLength(32);

        RuleFor(x => x.Emoji)
            .NotEmpty().MaximumLength(8);
    }
}

public sealed class IdentityResponse
{
    public int UserId { get; init; }

    public string Nickname { get; init; } = "";

    public string Emoji { get; init; } = "";

    public long LifetimeShots { get; init; }
}
