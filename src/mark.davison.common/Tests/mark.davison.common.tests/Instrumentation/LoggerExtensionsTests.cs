namespace mark.davison.common.tests.Instrumentation;

[TestClass]
public class LoggerExtensionsTests
{
    private readonly Mock<ILogger<LoggerExtensionsTests>> _logger = new(MockBehavior.Strict);

    public static Mock<ILogger<T>> VerifyLogging<T>(
        Mock<ILogger<T>> logger,
        Action<ILogger<T>> loggerAction,
        Func<string, bool> expectedMessageCallback,
        LogLevel expectedLogLevel = LogLevel.Debug,
        Times? times = null)
    {
        times ??= Times.Once();

        logger.Setup(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => expectedMessageCallback(v.ToString() ?? string.Empty)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

        loggerAction(logger.Object);

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => expectedMessageCallback(v.ToString() ?? string.Empty)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times.Value);

        return logger;
    }

    [TestMethod]
    public void ProfileOperation_MakesTwoCallsToTheLogger()
    {
        var logLevel = LogLevel.Trace;

        VerifyLogging(
            _logger,
            _ =>
            {
                using (_.ProfileOperation(logLevel))
                {

                }

            },
            _ => true,
            logLevel,
            Times.Exactly(2)
            );

    }
}
