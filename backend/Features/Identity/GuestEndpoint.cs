using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Entities;

namespace VirtualShot.Features.Identity;

/// <summary>Nickname-only join: creates a lightweight user and signs them in with a cookie.</summary>
public sealed class GuestEndpoint(AppDbContext db) : Endpoint<GuestRequest, IdentityResponse>
{
    public override void Configure()
    {
        Post("api/identity/guest");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GuestRequest req, CancellationToken ct)
    {
        var user = new User
        {
            Nickname = req.Nickname.Trim(),
            Emoji = string.IsNullOrWhiteSpace(req.Emoji) ? "😎" : req.Emoji.Trim(),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        await IdentitySignIn.SignInAsync(HttpContext, user);

        await Send.OkAsync(user.Adapt<IdentityResponse>(), ct);
    }
}
