using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using VirtualShot.Entities;

namespace VirtualShot.Features.Identity;

public static class IdentitySignIn
{
    public const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

    public static ClaimsPrincipal CreatePrincipal(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nickname),
            new("emoji", user.Emoji),
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
    }

    public static Task SignInAsync(HttpContext httpContext, User user)
        => httpContext.SignInAsync(
            Scheme,
            CreatePrincipal(user),
            new AuthenticationProperties { IsPersistent = true });
}
