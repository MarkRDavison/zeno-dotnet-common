namespace mark.davison.common.authentication.server.Models;

public record UserDto(Guid Id, Guid TenantId, string Email, string DisplayName, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset LastModified);
