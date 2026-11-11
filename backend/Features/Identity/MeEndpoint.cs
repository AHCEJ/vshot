using FastEndpoints;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VirtualShot.Database;
using VirtualShot.Services;

namespace VirtualShot.Features.Identity;

public sealed class MeEndpoint(AppDbContext db) : EndpointWithoutRequest<IdentityResponse>
{
    public override void Configure()
    {
        Get("api/identity/me");
        Policies("authed");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = await db.Users.FindAsync([User.GetUserId()], ct);
        if (user is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(user.Adapt<IdentityResponse>(), ct);
    }
}
