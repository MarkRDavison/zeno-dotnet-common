namespace mark.davison.common.authentication.server.Services;

public class UserRoleService<TDbContext> : IUserRoleService
    where TDbContext : DbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IMemoryCache _cache;
    private readonly IDateService _dateService;
    private readonly ILogger<UserRoleService<TDbContext>> _logger;

    // TODO: Config?
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public UserRoleService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IMemoryCache cache,
        IDateService dateService,
        ILogger<UserRoleService<TDbContext>> logger)
    {
        _dbContextFactory = dbContextFactory;
        _cache = cache;
        _dateService = dateService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetRolesForUserAsync(Guid userId)
    {
        if (_cache.TryGetValue(userId, out IReadOnlyList<UserRoleDto>? roles))
        {
            return roles!;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        roles = await dbContext.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Select(ur => new UserRoleDto(ur.Id, userId, ur.Role!.Name))
            .ToListAsync();

        _cache.Set(userId, roles, CacheDuration);

        return roles;
    }

    public Task InvalidateUserRolesAsync(Guid userId)
    {
        _logger.LogInformation("Invalidating role cache for user {UserId}", userId);
        _cache.Remove(userId);
        return Task.CompletedTask;
    }

    public async Task EnsureUserHasRole(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var adminRole = await dbContext.Set<Role>().FirstAsync(r => r.Name == roleName, cancellationToken);
        var alreadyHasRole = await dbContext.Set<UserRole>().AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id, cancellationToken);
        if (!alreadyHasRole)
        {
            dbContext.Set<UserRole>().Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = adminRole.Id,
                Created = _dateService.Now,
                LastModified = _dateService.Now
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            await InvalidateUserRolesAsync(userId);
        }

    }
}
