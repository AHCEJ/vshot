using System.Security.Claims;

namespace VirtualShot.Services;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal? principal)
    {
        var raw = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    public static string GetEmoji(this ClaimsPrincipal? principal)
        => principal?.FindFirstValue("emoji") ?? "😎";
}
