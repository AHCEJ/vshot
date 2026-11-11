using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Services;

namespace VirtualShot.Features.Identity;

public sealed class UpdateProfileEndpoint(AppDbContext db) : Endpoint<UpdateProfileRequest, IdentityResponse>
{
    public override void Configure()
    {
        Patch("api/identity");
        Policies("authed");
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([User.GetUserId()], ct);
        if (user is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        user.Nickname = req.Nickname.Trim();
        user.Emoji = req.Emoji.Trim();
        await db.SaveChangesAsync(ct);

        // Refresh the nickname/emoji claims used by the hub.
        await IdentitySignIn.SignInAsync(HttpContext, user);

        await Send.OkAsync(user.Adapt<IdentityResponse>(), ct);
    }
}
