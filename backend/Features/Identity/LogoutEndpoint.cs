using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VirtualShot.Features.Identity;

public sealed class LogoutEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("api/identity/logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await Send.NoContentAsync(ct);
    }
}
