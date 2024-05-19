namespace mark.davison.common.server.Endpoints;

public static class GetEndpoints
{
    public static IEndpointRouteBuilder UseGet<T>(this IEndpointRouteBuilder endpoints) where T : BaseEntity, new()
    {
        var entityName = typeof(T).Name.ToLowerInvariant();
        endpoints.MapGet(
            $"/api/{entityName}",
            async (HttpContext context, ILogger<T> logger, CancellationToken cancellationToken) =>
            {
                using (logger.ProfileOperation(LogLevel.Trace, $"GET /api/{entityName}"))
                {
                    var body = await EndpointHelpers.ExtractBody(context.Request);
                    var where = EndpointHelpers.GenerateWhereClause<T>(context.Request.Query, body);
                    var include = EndpointHelpers.GenerateIncludesClause(context.Request.Query);

                    var dbContext = context.RequestServices.GetRequiredService<IDbContext>();

                    var entities = await dbContext.Set<T>().Include(include).Where(where).ToListAsync(cancellationToken);
                    return Results.Ok(entities);
                }
            });

        return endpoints;
    }

    public static IEndpointRouteBuilder UseGetById<T>(this IEndpointRouteBuilder endpoints) where T : BaseEntity, new()
    {
        var entityName = typeof(T).Name.ToLowerInvariant();
        endpoints.MapGet(
            $"/api/{entityName}/{{id}}",
            async ([FromRoute] Guid id, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken) =>
            {
                using (logger.ProfileOperation(context: $"GET api/{entityName}/{id}"))
                {
                    var dbContext = context.RequestServices.GetRequiredService<IDbContext>();

                    var entity = await dbContext.Set<T>().FindAsync(id, cancellationToken);
                    if (entity == null)
                    {
                        return Results.NotFound();
                    }
                    return Results.Ok(entity);
                }
            });

        return endpoints;
    }
}
