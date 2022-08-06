namespace mark.davison.common.server.test.Framework;

public interface ICommonWebApplicationFactory<TSettings>
{
    public HttpClient CreateClient();
    public Func<IRepository, Task> SeedDataFunc { get; set; }
    public IServiceProvider ServiceProvider { get; }
}