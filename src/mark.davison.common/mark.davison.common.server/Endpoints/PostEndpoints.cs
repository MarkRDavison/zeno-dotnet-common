namespace mark.davison.common.server.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPost<T>(this IEndpointRouteBuilder endpoints) where T : BaseEntity, new()
    {
        var entityName = typeof(T).Name.ToLowerInvariant();
        endpoints.MapPost(
            $"/api/{entityName}",
            async ([FromBody] T entity, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken) =>
            {
                using (logger.ProfileOperation(context: $"POST api/{entityName}"))
                {
                    return await PostEntity<T>(entity, context, cancellationToken);
                }
            });

        return endpoints;
    }

    public static async Task<IResult> PostEntity<T>(T entity, HttpContext context, CancellationToken cancellationToken)
        where T : BaseEntity, new()
    {
        var dbContext = context.RequestServices.GetRequiredService<IDbContext>();
        var currentUserContext = context.RequestServices.GetRequiredService<ICurrentUserContext>();
        var entityDefaulter = context.RequestServices.GetService<IEntityDefaulter<T>>();

        if (entityDefaulter != null)
        {
            await entityDefaulter.DefaultAsync(entity, currentUserContext.CurrentUser);
        }

        var e = await dbContext.UpsertEntityAsync<T>(entity, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(e);
    }
}
