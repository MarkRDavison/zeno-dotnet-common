namespace mark.davison.common.Repository;

public class QueryParameters : Dictionary<string, string>
{
    public string CreateQueryString()
    {
        string uri = string.Empty;
        if (this.Any())
        {
            uri += "?";
            uri += string.Join("&", this.Select((kv) => $"{kv.Key.ToLowerInvariant()}={kv.Value}"));
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