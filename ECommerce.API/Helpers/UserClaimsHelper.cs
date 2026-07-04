using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.API.Helpers;

public static class UserClaimsHelper
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(id) || !int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");

        return userId;
    }
}
