namespace mark.davison.common.server.sample.api.Setup;

public static class SampleAppDependencyInjection
{
    public static IServiceCollection UseSampleApp(this IServiceCollection services, AppSettings appSettings, Func<HttpClient>? client)
    {
        services.AddScoped<ICurrentUserContext>(_ => new CurrentUserContext());

        services.AddDbContextFactory<TestDbContext>(_ => _
            .UseSqlite($"Data Source={Guid.NewGuid()}.db"));
        services.AddTransient<IRepository>(_ =>
            new TestRepository(
                _.GetRequiredService<IDbContextFactory<TestDbContext>>(),
                _.GetRequiredService<ILogger<TestRepository>>())
            );
        services.AddTransient<IReadonlyRepository>(_ =>
            new TestRepository(
                _.GetRequiredService<IDbContextFactory<TestDbContext>>(),
                _.GetRequiredService<ILogger<TestRepository>>())
            );

        services.AddSingleton<IHttpRepository>(_ =>
        {
            var options = SerializationHelpers.CreateStandardSerializationOptions();
            if (client == null)
            {
                return new SampleHttpRepository(appSettings.API_ORIGIN, options);
            }
            return new SampleHttpRepository(appSettings.API_ORIGIN, client(), options);
        });

        return services;
    }
}
