namespace mark.davison.common.persistence;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseRedis(this IServiceCollection services, RedisAppSettings redis, string name, bool productionMode)
    {
        if (!string.IsNullOrEmpty(redis.HOST))
        {
            var config = new ConfigurationOptions
            {
                EndPoints = { redis.HOST + ":" + redis.PORT },
                Password = redis.PASSWORD
            };

            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(config);
            services
                .AddStackExchangeRedisCache(_ =>
                {
                    _.InstanceName = name + "_" + (productionMode ? "PROD_" : "DEV_");
                    _.Configuration = connectionMultiplexer.Configuration;
                })
                .AddSingleton(connectionMultiplexer);
        }

        return services;
    }

    public static IServiceCollection UseDatabase<TDbContext>(this IServiceCollection services, bool productionMode, DatabaseAppSettings databaseAppSettings, params Type[] migrationTypes)
        where TDbContext : DbContextBase<TDbContext>
    {
        if (databaseAppSettings.DATABASE_TYPE == DatabaseType.Sqlite)
        {
            if (databaseAppSettings.CONNECTION_STRING.Equals("RANDOM", StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists("C:/temp"))
                {
                    databaseAppSettings.CONNECTION_STRING = $"Data Source=C:/temp/{Guid.NewGuid()}.db";
                }
                else
                {
                    databaseAppSettings.CONNECTION_STRING = $"Data Source={Guid.NewGuid()}.db";
                }
            }

            services.AddDbContextFactory<TDbContext>(_ =>
            {
                _.UseSqlite(
                    databaseAppSettings.CONNECTION_STRING,
                    _ => _.MigrationsAssembly(GetMigrationAssembly(DatabaseType.Sqlite, migrationTypes)));
                if (!productionMode)
                {
                    _.EnableSensitiveDataLogging();
                    _.EnableDetailedErrors();
                }
            });
        }
        else if (databaseAppSettings.DATABASE_TYPE == DatabaseType.Postgres)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var conn = new NpgsqlConnectionStringBuilder
            {
                IncludeErrorDetail = !productionMode,
                Host = databaseAppSettings.DB_HOST,
                Database = databaseAppSettings.DB_DATABASE,
                Port = databaseAppSettings.DB_PORT,
                Username = databaseAppSettings.DB_USERNAME,
                Password = databaseAppSettings.DB_PASSWORD
            };

            services.AddDbContextFactory<TDbContext>(_ => _
                .UseNpgsql(
                    conn.ConnectionString,
                    _ => _.MigrationsAssembly(GetMigrationAssembly(DatabaseType.Postgres, migrationTypes))));
        }
        else
        {
            throw new ArgumentException($"DATABASE_TYPE is invalid: {databaseAppSettings.DATABASE_TYPE}");
        }

        services.AddScoped<IDbContext<TDbContext>>(_ => _.GetRequiredService<TDbContext>());

        return services;
    }

    public static IServiceCollection UseCoreDbContext<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContextBase<TDbContext>
    {
        services.AddScoped<IDbContext>(_ => _.GetRequiredService<IDbContext<TDbContext>>());

        return services;
    }

    private static string? GetMigrationAssembly(DatabaseType databaseType, params Type[] types)
    {
        var migrationAssemblyType = types.SelectMany(_ => _.Assembly.ExportedTypes).Where(_ =>
        {
            var attribute = _.GetCustomAttributes(typeof(DatabaseMigrationAssemblyAttribute), false).OfType<DatabaseMigrationAssemblyAttribute>().FirstOrDefault();

            return attribute != null && attribute.DatabaseType == databaseType;
        }).FirstOrDefault();

        var name = migrationAssemblyType?.Assembly.GetName();

        return name?.Name;
    }
}
