namespace mark.davison.common.server.Notifications.Matrix;

public class MatrixNotificationService : IMatrixNotificationService
{
    private readonly MatrixNotificationSettings _settings;
    private readonly IMatrixClient _matrixClient;

    public MatrixNotificationService(
        IOptions<MatrixNotificationSettings> options,
        IMatrixClient matrixClient)
    {
        _settings = options.Value;
        _matrixClient = matrixClient;
    }

    public NotificationSettings Settings => _settings;

    public async Task<Response> SendNotification(string message)
    {
        return await _matrixClient.SendMessage(_settings.ROOMID, message);
    }
}
