﻿namespace mark.davison.common.server.Authentication;

public static class ClaimsPrincipalHelpers
{
    public static User? ExtractUser(this ClaimsPrincipal claimsPrincipal, ClaimsAppSettings claimsSettings)
    {
        var idClaimValue = ExtractClaimValue(claimsPrincipal, claimsSettings.OIDC_ID_ATTRIBUTE);

        if (!Guid.TryParse(idClaimValue, out Guid id))
        {
            return null;
        }

        var firstNameClaimValue = ExtractClaimValue(claimsPrincipal, claimsSettings.OIDC_FIRST_NAME_ATTRIBUTE);
        var lastNameClaimValue = ExtractClaimValue(claimsPrincipal, claimsSettings.OIDC_LAST_NAME_ATTRIBUTE);
        var usernameClaimValue = ExtractClaimValue(claimsPrincipal, claimsSettings.OIDC_USERNAME_ATTRIBUTE);
        var emailClaimValue = ExtractClaimValue(claimsPrincipal, claimsSettings.OIDC_EMAIL_ATTRIBUTE);
        var adminClaimValue = claimsPrincipal.Claims.Any(_ => string.Equals(_.Type, "groups", StringComparison.OrdinalIgnoreCase) && _.Value == claimsSettings.OIDC_ADMIN_GROUP_NAME);

        return new User
        {
            Id = id,
            Sub = id,
            Admin = adminClaimValue,
            Email = emailClaimValue,
            First = firstNameClaimValue,
            Last = lastNameClaimValue,
            Username = usernameClaimValue,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
    }

    private static string ExtractClaimValue(ClaimsPrincipal claimsPrincipal, string claimName) => claimsPrincipal.Claims.FirstOrDefault(_ => _.Type == claimName)?.Value ?? string.Empty;
}
