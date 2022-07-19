namespace mark.davison.common.server.abstractions.Repository;

public class QueryParameters : Dictionary<string, string>
{
    public string CreateQueryString()
    {
        string uri = string.Empty;
        if (this.Any())
        {
            uri += "?";
            uri += string.Join("&", this.Select((kv) => $"{kv.Key}={kv.Value}"));
        }
        return uri;
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