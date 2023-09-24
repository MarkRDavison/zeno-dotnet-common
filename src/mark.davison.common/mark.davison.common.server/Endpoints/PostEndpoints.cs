namespace mark.davison.common.server.Endpoints;

public static class PostEndpoints
{
    public static void UsePost<T>(this IEndpointRouteBuilder endpoints) where T : BaseEntity, new()
    {
        var entityName = typeof(T).Name.ToLowerInvariant();
        endpoints.MapPost(
            $"/api/{entityName}",
            async ([FromBody] T entity, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken) =>
            {
                using (logger.ProfileOperation(context: $"POST api/{entityName}"))
                {
                    return await PostEntity<T>(entity, context, logger, cancellationToken);
                }
            });
    }

    public static async Task<IResult> PostEntity<T>(T entity, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken)
        where T : BaseEntity, new()
    {
        var repository = context.RequestServices.GetRequiredService<IRepository>();
        var currentUserContext = context.RequestServices.GetRequiredService<ICurrentUserContext>();
        var entityDefaulter = context.RequestServices.GetService<IEntityDefaulter<T>>();

        if (entityDefaulter != null)
        {
            await entityDefaulter.DefaultAsync(entity, currentUserContext.CurrentUser);
        }

        await using (repository.BeginTransaction())
        {
            var posted = await repository.UpsertEntityAsync(entity, cancellationToken);
            if (posted == null)
            {
                return Results.UnprocessableEntity();
            }

            return Results.Ok(posted);
        }
    }
}
