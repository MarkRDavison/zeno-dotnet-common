using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace mark.davison.example.console.app;

public class SystemBrowser : IBrowser
{
    public int Port { get; }
    private readonly string? _path;

    public SystemBrowser(int? port = null, string? path = null)
    {
        _path = path;

        if (!port.HasValue)
        {
            Port = GetRandomUnusedPort();
        }
        else
        {
            Port = port.Value;
        }
    }

    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default(CancellationToken))
    {
        using (var listener = new LoopbackHttpListener(Port, _path))
        {
            using (var process = OpenBrowser(options.StartUrl))
            {

                try
                {
                    var result = await listener.WaitForCallbackAsync();
                    if (String.IsNullOrWhiteSpace(result))
                    {
                        return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
                    }

                    return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
                }
                catch (TaskCanceledException ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
                }
                catch (Exception ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
                }
                finally
                {
                    process?.Kill(true);
                    process?.CloseMainWindow();
                    process?.Close();
                }
            }
        }
    }

    public static Process? OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Process.Start("open", url);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
}

public class LoopbackHttpListener : IDisposable
{
    const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

    IWebHost _host;
    TaskCompletionSource<string> _source = new TaskCompletionSource<string>();
    string _url;

    public string Url => _url;

    public LoopbackHttpListener(int port, string? path = null)
    {
        path = path ?? String.Empty;
        if (path.StartsWith("/")) path = path.Substring(1);

        _url = $"http://127.0.0.1:{port}/{path}";

        _host = WebHost.CreateDefaultBuilder()
            .UseKestrel()
            .UseUrls(_url)
            .Configure(Configure)
            .Build();
        _host.Start();
    }

    public void Dispose()
    {
        Console.WriteLine("DISPOSE");
        Task.Run(async () =>
        {
            await Task.Delay(500);
            _host.Dispose();
        });
    }

    void Configure(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            if (ctx.Request.Method == "GET")
            {
                await SetResultAsync(ctx.Request.QueryString.Value, ctx);
            }
            else if (ctx.Request.Method == "POST")
            {
                if (!ctx.Request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.StatusCode = 415;
                }
                else
                {
                    using (var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8))
                    {
                        var body = await sr.ReadToEndAsync();
                        await SetResultAsync(body, ctx);
                    }
                }
            }
            else
            {
                ctx.Response.StatusCode = 405;
            }
        });
    }

    private async Task SetResultAsync(string value, HttpContext ctx)
    {
        try
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
            await ctx.Response.Body.FlushAsync();

            _source.TrySetResult(value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());

            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
            await ctx.Response.Body.FlushAsync();
        }
    }

    public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
    {
        Task.Run(async () =>
        {
            await Task.Delay(timeoutInSeconds * 1000);
            _source.TrySetCanceled();
        });

        return _source.Task;
    }
}