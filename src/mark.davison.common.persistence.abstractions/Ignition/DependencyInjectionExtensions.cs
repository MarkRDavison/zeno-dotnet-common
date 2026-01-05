namespace mark.davison.common.persistence.Ignition;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDatabase<TDbContext>(this IServiceCollection services, bool productionMode, DatabaseAppSettings databaseAppSettings, params Type[] migrationTypes)
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

            services.AddDbContextFactory<TDbContext>((sp, options) =>
            {
                options
                    .UseSqlite(
                        databaseAppSettings.CONNECTION_STRING,
                        o => o.MigrationsAssembly(GetMigrationAssembly(DatabaseType.Sqlite, migrationTypes)))
                    .UseSeeding((context, _) =>
                    {
                        var ds = sp.GetService<IDataSeeder>();

                        if (ds is not null)
                        {
                            ds.SeedDataAsync(context, CancellationToken.None).GetAwaiter().GetResult();
                        }
                    })
                    .UseAsyncSeeding(async (context, _, cancellationToken) =>
                        {
                            var ds = sp.GetService<IDataSeeder>();

                            if (ds is not null)
                            {
                                await ds.SeedDataAsync(context, cancellationToken);
                            }
                        });
                if (!productionMode)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });
        }
        else if (databaseAppSettings.DATABASE_TYPE == DatabaseType.Postgres)
        {
            var conn = new NpgsqlConnectionStringBuilder
            {
                IncludeErrorDetail = !productionMode,
                Host = databaseAppSettings.DB_HOST,
                Database = databaseAppSettings.DB_DATABASE,
                Port = databaseAppSettings.DB_PORT,
                Username = databaseAppSettings.DB_USERNAME,
                Password = databaseAppSettings.DB_PASSWORD,
                GssEncryptionMode = GssEncryptionMode.Disable
            };


            services.AddDbContextFactory<TDbContext>((sp, options) =>
            {
                options
                    .UseNpgsql(
                        conn.ConnectionString,
                        _ => _.MigrationsAssembly(GetMigrationAssembly(DatabaseType.Postgres, migrationTypes)))
                    .UseSeeding((context, _) =>
                    {
                        var ds = sp.GetService<IDataSeeder>();

                        if (ds is not null)
                        {
                            ds.SeedDataAsync(context, CancellationToken.None).GetAwaiter().GetResult();
                        }
                    })
                    .UseAsyncSeeding(async (context, _, cancellationToken) =>
                    {
                        var ds = sp.GetService<IDataSeeder>();

                        if (ds is not null)
                        {
                            await ds.SeedDataAsync(context, cancellationToken);
                        }
                    });
            });
        }
        else
        {
            throw new ArgumentException($"DATABASE_TYPE is invalid: {databaseAppSettings.DATABASE_TYPE}");
        }

        services.AddScoped<IDbContext<TDbContext>>(_ => _.GetRequiredService<TDbContext>());

        return services;
    }

    public static IServiceCollection AddCoreDbContext<TDbContext>(this IServiceCollection services)
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
