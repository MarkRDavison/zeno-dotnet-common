using IdentityModel.OidcClient.Browser;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace mark.davison.common.desktop.components.Auth;

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
        using (var listener = new LoopbackHttpListener(Port, _path!))
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