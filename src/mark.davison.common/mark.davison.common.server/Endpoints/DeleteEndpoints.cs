namespace mark.davison.common.server.Endpoints;

public static class DeleteEndpoints
{
    public static IEndpointRouteBuilder UseDelete<T>(this IEndpointRouteBuilder endpoints) where T : BaseEntity, new()
    {
        var entityName = typeof(T).Name.ToLowerInvariant();
        endpoints.MapDelete(
            $"/api/{entityName}/{{id}}",
            async ([FromRoute] Guid id, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken) =>
            {
                using (logger.ProfileOperation(LogLevel.Trace, $"DELETE /api/{entityName}/{id}"))
                {
                    return await DeleteEntity<T>(id, context, logger, cancellationToken);
                }
            });

        return endpoints;

    }
    public static async Task<IResult> DeleteEntity<T>(Guid id, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken)
        where T : BaseEntity, new()
    {
        var repository = context.RequestServices.GetRequiredService<IRepository>();

        await using (repository.BeginTransaction())
        {
            var entity = await repository.GetEntityAsync<T>(id, cancellationToken);
            if (entity == null)
            {
                return Results.NotFound();
            }

            var deletedEntity = await repository.DeleteEntityAsync(entity, cancellationToken);

            if (deletedEntity == null)
            {
                return Results.UnprocessableEntity();
            }
        }

        return Results.NoContent();
    }
}
