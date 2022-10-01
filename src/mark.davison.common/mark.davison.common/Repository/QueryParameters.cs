using Remote.Linq;
using Remote.Linq.Text.Json;
using System.Linq.Expressions;
using System.Text.Json.Nodes;

namespace mark.davison.common.Repository;

public class QueryParameters : Dictionary<string, string>
{
    public static readonly string[] BodyParameters = { "where", "project" };
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureRemoteLinq();

    public string CreateQueryString()
    {
        string uri = string.Empty;
        if (this.Any())
        {
            uri += "?";
            uri += string.Join("&", this.Where(_ => !BodyParameters.Contains(_.Key)).Select((kv) => $"{kv.Key.ToLowerInvariant()}={kv.Value}"));
        }
        return uri;
    }

    public string CreateBody()
    {
        var bodyParemeters = this.Where(_ => BodyParameters.Contains(_.Key)).ToList();

        if (bodyParemeters.Any())
        {
            var body = new JsonObject();

            foreach (var p in bodyParemeters)
            {
                body.Add(p.Key, JsonValue.Create(p.Value));
            }

            return JsonSerializer.Serialize(body);
        }

        return string.Empty;
    }

    public void Where<TEntity>(Expression<Func<TEntity, bool>> where)
        where TEntity : class, new()
    {
        this["where"] = JsonSerializer.Serialize(where.ToRemoteLinqExpression(), _options);
    }

    public void Include(string path)
    {
        if (!ContainsKey("include"))
        {
            this["include"] = path;
        }
        else
        {
            this["include"] += "|" + path;
        }
    }
}