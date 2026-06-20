using System.Security.Claims;

namespace mark.davison.common.authentication.server.abstractions.Services;

public interface ICurrentUserContext
{
    Task<ClaimsPrincipal> PopulateFromPrincipal(ClaimsPrincipal principal, string provider);
    void PopulateFromIds(Guid userId, Guid tenantId, IEnumerable<string> roles, bool authenticated);
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid TenantId { get; }
    bool HasRole(string role);
}
