namespace mark.davison.common.server.Endpoints;

public static class EndpointHelpers
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureRemoteLinq();

    public static Task<JsonObject?> ExtractBody(HttpRequest request)
    {
        return ExtractBody(request, _options);
    }

    public static async Task<JsonObject?> ExtractBody(HttpRequest request, JsonSerializerOptions options)
    {
        JsonObject? body = null;

        try
        {
            using var streamWriter = new StreamReader(request.Body, Encoding.UTF8);
            body = await JsonSerializer.DeserializeAsync<JsonObject>(streamWriter.BaseStream, options);
        }
        catch (Exception) { }

        return body;
    }

    public static Expression<Func<T, bool>>? GenerateWhereClause<T>(
        IQueryCollection query,
        JsonObject? body)
        where T : BaseEntity
    {
        IDictionary<Type, Func<StringValues, object>> typeCoersions = new Dictionary<Type, Func<StringValues, object>>
        {
            { typeof(Guid), _ => new Guid(_.ToString()) },
            { typeof(long), _ => long.Parse(_.ToString()) },
            { typeof(int), _ => int.Parse(_.ToString()) },
            { typeof(string), _ => _.ToString() },
            { typeof(DateOnly), _ => DateOnly.Parse(_) },
        };

        if (body != null && body.ContainsKey("where"))
        {
            var expressionText = body["where"]!.GetValue<string>();
            var deserialized = JsonSerializer
                .Deserialize<Remote.Linq.Expressions.Expression>(
                    expressionText,
                    _options);
            return deserialized?.ToLinqExpression() as Expression<Func<T, bool>>;
        }
        else
        {
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
        }

        return null;
    }

    public static string GenerateIncludesClause(IQueryCollection query)
    {
        if (query.ContainsKey("include"))
        {
            return query["include"];
        }

        return string.Empty;
    }
}
