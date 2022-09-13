namespace mark.davison.common.persistence.Controllers;

public abstract class BaseController<T> : ControllerBase where T : BaseEntity, new()
{
    protected readonly ILogger _logger;
    protected readonly IRepository _repository;
    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly ICurrentUserContext _currentUserContext;

    public BaseController(
        ILogger logger,
        IRepository repository,
        IServiceScopeFactory serviceScopeFactory,
        ICurrentUserContext currentUserContext)
    {
        _logger = logger;
        _repository = repository;
        _serviceScopeFactory = serviceScopeFactory;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var where = GenerateWhereClause(HttpContext.Request.Query);
        var include = GenerateIncludesClause(HttpContext.Request.Query);

        using (_logger.ProfileOperation(context: $"GET api/{typeof(T).Name.ToLowerInvariant()}"))
        {
            return Ok(await _repository.GetEntitiesAsync<T>(where, include, cancellationToken));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        using (_logger.ProfileOperation(context: $"GET api/{typeof(T).Name.ToLowerInvariant()}/{id}"))
        {
            var cuc = HttpContext.RequestServices.GetService<ICurrentUserContext>();
            var entity = await _repository.GetEntityAsync<T>(id, cancellationToken);
            if (entity == null)
            {
                return new BadRequestResult();
            }
            return Ok(entity);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] T entity, CancellationToken cancellationToken)
    {
        using (_logger.ProfileOperation(context: $"POST api/{typeof(T).Name.ToLowerInvariant()}"))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var entityDefaulter = scope.ServiceProvider.GetService<IEntityDefaulter<T>>();

            if (entityDefaulter != null)
            {
                await entityDefaulter.DefaultAsync(entity, _currentUserContext.CurrentUser);
            }

            try
            {
                var savedEntity = await _repository.UpsertEntityAsync(entity, cancellationToken);
                if (savedEntity == null)
                {
                    return new BadRequestResult();
                }
                return Ok(savedEntity);
            }
            catch (AggregateException e)
            {
                return new BadRequestObjectResult(new
                {
                    Error = e.Message,
                    Validations = e.InnerExceptions.Select(e => e.Message).ToList()
                });
            }
        }
    }

    protected abstract void PatchUpdate(T persisted, T patched);

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] T patch, CancellationToken cancellationToken)
    {
        using (_logger.ProfileOperation(context: $"PATCH api/{typeof(T).Name.ToLowerInvariant()}/{id}"))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var entityDefaulter = scope.ServiceProvider.GetService<IEntityDefaulter<T>>();

            var entity = await _repository.GetEntityAsync<T>(id, cancellationToken);

            if (entity == null)
            {
                return new BadRequestResult();
            }

            PatchUpdate(entity, patch);

            if (entityDefaulter != null)
            {
                await entityDefaulter.DefaultAsync(entity, _currentUserContext.CurrentUser);
            }

            try
            {
                var savedEntity = await _repository.UpsertEntityAsync(patch, cancellationToken);
                if (savedEntity == null)
                {
                    return new BadRequestResult();
                }
                return Ok(savedEntity);
            }
            catch (AggregateException e)
            {
                return new BadRequestObjectResult(new
                {
                    Error = e.Message,
                    Validations = e.InnerExceptions.Select(e => e.Message).ToList()
                });
            }
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        using (_logger.ProfileOperation(context: $"DELETE api/{typeof(T).Name.ToLowerInvariant()}/{id}"))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var entity = await _repository.GetEntityAsync<T>(id, cancellationToken);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var deletedEntity = await _repository.DeleteEntityAsync(entity, cancellationToken);
            if (deletedEntity != null)
            {
                return Ok(deletedEntity);
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }

    protected string GenerateIncludesClause(IQueryCollection query)
    {
        return query["includes"];
    }

    protected Expression<Func<T, bool>>? GenerateWhereClause(IQueryCollection query)
    {
        // TODO: Only ands are supported - or is that the intention???
        IDictionary<Type, Func<StringValues, object>> typeCoersions = new Dictionary<Type, Func<StringValues, object>>
        {
            { typeof(Guid), _ => new Guid(_.ToString()) },
            { typeof(long), _ => long.Parse(_.ToString()) },
            { typeof(int), _ => int.Parse(_.ToString()) },
            { typeof(string), _ => _.ToString() },
        };

        var properties = typeof(T).GetProperties();

        var tParam = Expression.Parameter(typeof(T));

        List<BinaryExpression> lambdaParts = new();

        foreach (var q in query)
        {
            var p = properties.FirstOrDefault(_ => string.Equals(_.Name, q.Key, StringComparison.OrdinalIgnoreCase));
            if (p != null)
            {
                var argParam = Expression.Property(tParam, p.Name);
                var valParam = Expression.Constant(typeCoersions[p.PropertyType](q.Value));
                var eqParam = Expression.Equal(argParam, valParam);

                lambdaParts.Add(eqParam);
            }
        }


        if (lambdaParts.Count > 0)
        {
            Expression? where = lambdaParts[0];
            for (int i = 1; i < lambdaParts.Count; i++)
            {
                var rhs = lambdaParts[i + 0];
                where = Expression.AndAlso(where, rhs);
            }

            return Expression.Lambda<Func<T, bool>>(where, tParam);
        }

        return null;
    }

}