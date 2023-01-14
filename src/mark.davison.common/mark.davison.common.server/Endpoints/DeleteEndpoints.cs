namespace mark.davison.common.server.Endpoints;

public static class DeleteEndpoints
{
    public static void UseDelete<T>(this IEndpointRouteBuilder endpoints) where T : BaseEntity
    {
        var entityName = typeof(T).Name.ToLowerInvariant();
        endpoints.MapDelete(
            $"/api/{entityName}/{{id}}",
            async ([FromRoute] Guid id, HttpContext context, ILogger<T> logger, CancellationToken cancellationToken) =>
            {
                using (logger.ProfileOperation(LogLevel.Trace, $"DELETE /api/{entityName}/{id}"))
                {
                    var repository = context.RequestServices.GetRequiredService<IRepository>();

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

                    return Results.NoContent();
                }
            });

    }
}
