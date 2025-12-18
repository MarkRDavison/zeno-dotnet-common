namespace mark.davison.common.tests.Services;

public sealed class RateLimitServiceTests
{
    [Test]
    public async Task ExampleUsage()
    {
        var cooldown = TimeSpan.FromMilliseconds(10);
        var service = new RateLimitService(permitLimit: 2, cooldown);

        ConcurrentBag<int> completedThreads = [];

        const int Concurrent = 10;

        var tasks = Enumerable.Range(0, Concurrent).Select(async _ =>
        {
            Console.WriteLine("Thread {0} trying to go.", _);
            var lease = await service.WaitToProceedAsync();

            if (lease is null)
            {
                Console.WriteLine("Thread {0} could not go at all.", _);
                return;
            }

            var time = Random.Shared.Next(2, 10);
            Console.WriteLine("Thread {0} going, work will take {1} milliseconds.", _, time);

            await Task.Delay(TimeSpan.FromMilliseconds(time));

            Console.WriteLine("Thread {0} work done, releasing lease", _);
            await lease.DisposeAsync();
            completedThreads.Add(_);

        }).ToArray();

        await Task.WhenAll(tasks);

        foreach (var id in Enumerable.Range(0, Concurrent))
        {
            await Assert.That(completedThreads).Contains(id);
        }
    }

    [Test]
    public async Task SimpleSinglePermitTest_Queueing()
    {
        var cooldown = TimeSpan.FromMilliseconds(50);
        var service = new RateLimitService(permitLimit: 1, cooldown);

        // Acquire the only permit
        var lease = await service.WaitToProceedAsync();
        await Assert.That(lease).IsNotNull();

        // Immediately try again: it will queue, so it will not be null
        var secondTask = service.WaitToProceedAsync();

        // Dispose first lease to release permit
        await lease!.DisposeAsync();

        // Second acquisition should now complete
        var second = await secondTask;
        await Assert.That(second).IsNotNull();

        await second!.DisposeAsync();
    }

    [Test]
    public async Task TwoPermitsConcurrencyAndCooldown_Queueing()
    {
        var cooldown = TimeSpan.FromMilliseconds(50);
        var service = new RateLimitService(permitLimit: 2, cooldown);

        // Acquire both permits
        var first = await service.WaitToProceedAsync();
        var second = await service.WaitToProceedAsync();

        await Assert.That(first).IsNotNull();
        await Assert.That(second).IsNotNull();

        // Attempt third acquisition; will queue
        var thirdTask = service.WaitToProceedAsync();

        // Dispose first permit to trigger cooldown
        await first!.DisposeAsync();

        // Third acquisition completes after first cooldown
        var third = await thirdTask;
        await Assert.That(third).IsNotNull();

        // Dispose second permit
        await second!.DisposeAsync();

        // Now fourth acquisition is possible
        var fourth = await service.WaitToProceedAsync();
        await Assert.That(fourth).IsNotNull();

        // Dispose remaining leases
        await third!.DisposeAsync();
        await fourth!.DisposeAsync();
    }


    [Test]
    public async Task TryProceed_SimpleTest()
    {
        var cooldown = TimeSpan.FromMilliseconds(50);
        var service = new RateLimitService(permitLimit: 2, cooldown);

        // Acquire first two permits synchronously
        var first = service.TryProceed();
        var second = service.TryProceed();

        await Assert.That(first).IsNotNull();
        await Assert.That(second).IsNotNull();

        // Third attempt should fail
        var third = service.TryProceed();
        await Assert.That(third).IsNull();

        // Dispose first permit to start cooldown
        var disposeTask1 = first!.DisposeAsync();

        // Immediately trying again: still only 1 slot released, the other still held -> should fail
        var fourth = service.TryProceed();
        await Assert.That(fourth).IsNull();

        // Dispose second permit
        var disposeTask2 = second!.DisposeAsync();

        // Wait for cooldown to finish
        await disposeTask1;
        await disposeTask2;

        // Now both slots are available again
        var fifth = service.TryProceed();
        var sixth = service.TryProceed();
        var seventh = service.TryProceed(); // should fail, only 2 permits

        await Assert.That(fifth).IsNotNull();
        await Assert.That(sixth).IsNotNull();
        await Assert.That(seventh).IsNull();

        await fifth!.DisposeAsync();
        await sixth!.DisposeAsync();
    }

    [Test]
    public async Task TryProceed_ConcurrentUsage()
    {
        var cooldown = TimeSpan.FromMilliseconds(50);
        var service = new RateLimitService(permitLimit: 3, cooldown);

        var leases = new ConcurrentBag<IAsyncDisposable>();

        // Start 10 tasks trying to acquire permits simultaneously
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var lease = service.TryProceed();
            if (lease != null)
            {
                leases.Add(lease);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Only 3 leases should have been acquired
        await Assert.That(leases.Count).IsEqualTo(3);

        // Any additional TryProceed attempts fail
        var extra = service.TryProceed();
        await Assert.That(extra).IsNull();

        // Dispose all acquired leases to start cooldowns
        var disposeTasks = leases.Select(l => l.DisposeAsync().AsTask()).ToArray();
        await Task.WhenAll(disposeTasks);

        // Wait a little longer than cooldown
        await Task.Delay(60);

        // Now all permits should be available again
        var leasesAfterCooldown = new List<IAsyncDisposable>();
        for (int i = 0; i < 3; i++)
        {
            var lease = service.TryProceed();
            await Assert.That(lease).IsNotNull();
            leasesAfterCooldown.Add(lease!);
        }

        // No more permits available
        var noneLeft = service.TryProceed();
        await Assert.That(noneLeft).IsNull();

        foreach (var lease in leasesAfterCooldown)
        {
            await lease.DisposeAsync();
        }
    }

    [Test]
    public async Task WaitToProceedAsync_ConcurrentUsage_Fixed()
    {
        var cooldown = TimeSpan.FromMilliseconds(50);
        var service = new RateLimitService(permitLimit: 3, cooldown);

        var leases = new ConcurrentBag<IAsyncDisposable>();

        // Start 10 tasks trying to acquire permits simultaneously
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var lease = await service.WaitToProceedAsync();

            leases.Add(lease!);

            await Task.Delay(10);

            await lease.DisposeAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        // At most 3 leases should have been active at any time
        // We can't assert exact Count here because queued tasks wait, but we can assert all 10 tasks completed
        await Assert.That(leases.Count).IsEqualTo(10);

        // Now test that permits are reusable after cooldown
        var leasesAfterCooldown = new List<IAsyncDisposable>();
        for (int i = 0; i < 3; i++)
        {
            var lease = await service.WaitToProceedAsync();
            await Assert.That(lease).IsNotNull();
            leasesAfterCooldown.Add(lease!);
        }

        foreach (var lease in leasesAfterCooldown)
        {
            await lease.DisposeAsync();
        }
    }


}
