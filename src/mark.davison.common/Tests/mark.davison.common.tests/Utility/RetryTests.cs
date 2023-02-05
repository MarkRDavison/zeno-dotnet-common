using System.Diagnostics;

namespace mark.davison.common.tests.Utility;

[TestClass]
public class RetryTests
{
    [TestMethod]
    public async Task Do_NoReturn_ReturnsIfActionSucceeds()
    {
        await Retry.Do(_ => Task.CompletedTask, TimeSpan.FromMicroseconds(1), CancellationToken.None);
    }

    [TestMethod]
    public async Task Do_WithReturns_ReturnsIfActionSucceeds()
    {
        var val = 1;
        var result = await Retry.Do(_ => Task.FromResult(val), TimeSpan.FromMicroseconds(1), CancellationToken.None);
        Assert.AreEqual(val, result);
    }

    [TestMethod]
    public async Task Do_NoReturn_CallsActionUntilItSucceeds()
    {
        int maxTimes = 5;
        int timesCalled = 0;
        await Retry.Do(_ =>
        {
            timesCalled++;
            if (timesCalled == maxTimes)
            {
                return Task.CompletedTask;
            }
            throw new InvalidOperationException();
        }, TimeSpan.FromMicroseconds(1), CancellationToken.None, maxTimes);

        Assert.AreEqual(maxTimes, timesCalled);
    }

    [TestMethod]
    public async Task Do_WithReturns_CallsActionUntilItSucceeds()
    {
        int maxTimes = 5;
        int timesCalled = 0;

        var val = 1;
        var result = await Retry.Do(_ =>
        {
            timesCalled++;
            if (timesCalled == maxTimes)
            {
                return Task.FromResult(val);
            }
            throw new InvalidOperationException();
        }, TimeSpan.FromMicroseconds(1), CancellationToken.None, maxTimes);

        Assert.AreEqual(val, result);
        Assert.AreEqual(maxTimes, timesCalled);
    }

    [TestMethod]
    public async Task Do_NoReturn_DoesNotCallActionIfCancellationRequested()
    {
        CancellationTokenSource cts = new();
        cts.Cancel();
        int timesCalled = 0;
        await Retry.Do(_ =>
        {
            timesCalled++;
            return Task.CompletedTask;

        }, TimeSpan.FromMicroseconds(1), cts.Token);

        Assert.AreEqual(0, timesCalled);
    }

    [TestMethod]
    public async Task Do_WithReturns_DoesNotCallActionIfCancellationRequested()
    {
        CancellationTokenSource cts = new();
        cts.Cancel();
        int timesCalled = 0;
        await Retry.Do(_ =>
        {
            timesCalled++;
            return Task.FromResult(1);

        }, TimeSpan.FromMicroseconds(1), cts.Token);

        Assert.AreEqual(0, timesCalled);
    }

    [TestMethod]
    public async Task Do_NoReturn_DelaysUsingTheRetryInterval()
    {
        int maxTimes = 5;
        int timesCalled = 0;
        var delay = TimeSpan.FromMilliseconds(5);

        Stopwatch sw = new();

        sw.Start();
        await Retry.Do(_ =>
        {
            timesCalled++;
            if (timesCalled == maxTimes)
            {
                return Task.CompletedTask;
            }
            throw new InvalidOperationException();
        }, delay, CancellationToken.None, maxTimes);

        Assert.IsTrue(sw.Elapsed > delay * (maxTimes - 1));
    }

    [TestMethod]
    public async Task Do_WithReturns_DelaysUsingTheRetryInterval()
    {
        int maxTimes = 5;
        int timesCalled = 0;
        var delay = TimeSpan.FromMilliseconds(5);

        var val = 1;
        Stopwatch sw = new();

        sw.Start();
        var result = await Retry.Do(_ =>
        {
            timesCalled++;
            if (timesCalled == maxTimes)
            {
                return Task.FromResult(val);
            }
            throw new InvalidOperationException();
        }, delay, CancellationToken.None, maxTimes);

        Assert.IsTrue(sw.Elapsed > delay * (maxTimes - 1));
    }

    [TestMethod]
    [ExpectedException(typeof(AggregateException))]
    public async Task Do_NoReturn_ThrowsAggregateException_MaxRetriesReached()
    {
        await Retry.Do(_ =>
        {
            throw new InvalidOperationException();
        }, TimeSpan.FromMicroseconds(1), CancellationToken.None);
    }

    [TestMethod]
    [ExpectedException(typeof(AggregateException))]
    public async Task Do_WithReturns_ThrowsAggregateException_MaxRetriesReached()
    {
        await Retry.Do<int>(_ =>
        {
            throw new InvalidOperationException();
        }, TimeSpan.FromMicroseconds(1), CancellationToken.None);
    }
}
