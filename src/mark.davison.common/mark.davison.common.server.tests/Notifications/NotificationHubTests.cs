namespace mark.davison.common.server.tests.Notifications;

[TestClass]
public class NotificationHubTests
{
    private readonly INotificationService _service1;
    private readonly INotificationService _service2;
    private readonly INotificationService _disabledService;

    private readonly NotificationSettings _service1Settings;
    private readonly NotificationSettings _service2Settings;
    private readonly NotificationSettings _disabledServiceSettings;

    private readonly NotificationHub _hub;

    public NotificationHubTests()
    {
        _service1 = Substitute.For<INotificationService>();
        _service2 = Substitute.For<INotificationService>();
        _disabledService = Substitute.For<INotificationService>();

        _service1Settings = Substitute.For<NotificationSettings>();
        _service2Settings = Substitute.For<NotificationSettings>();
        _disabledServiceSettings = Substitute.For<NotificationSettings>();

        _service1Settings.ENABLED.Returns(true);
        _service2Settings.ENABLED.Returns(true);
        _disabledServiceSettings.ENABLED.Returns(false);

        _service1.Settings.Returns(_service1Settings);
        _service2.Settings.Returns(_service2Settings);
        _disabledService.Settings.Returns(_disabledServiceSettings);

        _hub = new([_service1, _service2, _disabledService]);
    }

    [TestMethod]
    public async Task SendNotification_WillNotSendOnDisabledServices()
    {
        _service1.SendNotification(Arg.Any<string>()).Returns(new Response());
        _service2.SendNotification(Arg.Any<string>()).Returns(new Response());

        await _hub.SendNotification("MESSAGE");

        await _disabledService
            .DidNotReceive()
            .SendNotification(Arg.Any<string>());
    }

    [TestMethod]
    public async Task SendNotification_WithAllSuccess_ReturnsSuccess()
    {
        _service1.SendNotification(Arg.Any<string>()).Returns(new Response());
        _service2.SendNotification(Arg.Any<string>()).Returns(new Response());

        var response = await _hub.SendNotification("MESSAGE");

        Assert.IsTrue(response.Success);
    }

    [TestMethod]
    public async Task SendNotification_WithErrors_ReturnsError()
    {
        var error1 = "error 1";
        var error2 = "error 2";
        _service1.SendNotification(Arg.Any<string>()).Returns(new Response { Errors = [error1] });
        _service2.SendNotification(Arg.Any<string>()).Returns(new Response { Errors = [error2] });

        var response = await _hub.SendNotification("MESSAGE");

        Assert.IsFalse(response.Success);
        Assert.IsTrue(response.Errors.Contains(error1));
        Assert.IsTrue(response.Errors.Contains(error2));
    }

    [TestMethod]
    public async Task SendNotification_WithWarnings_ReturnsSuccessWithWarning()
    {
        var warning1 = "warning 1";
        var warning2 = "warning 2";
        _service1.SendNotification(Arg.Any<string>()).Returns(new Response { Warnings = [warning1] });
        _service2.SendNotification(Arg.Any<string>()).Returns(new Response { Warnings = [warning2] });

        var response = await _hub.SendNotification("MESSAGE");

        Assert.IsTrue(response.Success);
        Assert.IsTrue(response.Warnings.Contains(warning1));
        Assert.IsTrue(response.Warnings.Contains(warning2));
    }
}
