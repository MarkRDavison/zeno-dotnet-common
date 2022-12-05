namespace mark.davison.common.test;

public static class RemoteLinqHelpers
{

    public static Expression<Func<T, bool>>? ExtractExpression<T>(this QueryParameters q)
        where T : class
    {
        var options = new JsonSerializerOptions().ConfigureRemoteLinq();
        var body = JsonSerializer.Deserialize<JsonObject>(q.CreateBody(), options);
        var expressionText = body!["where"]!.GetValue<string>();
        var deserialized = JsonSerializer
            .Deserialize<Remote.Linq.Expressions.Expression>(
                expressionText,
                options);
        return deserialized?.ToLinqExpression() as Expression<Func<T, bool>>;
    }

}
