namespace mark.davison.common.persistence;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionExtensions
{
    public static void UseDatabase<TDbContext>(this IServiceCollection services, bool productionMode, DatabaseAppSettings databaseAppSettings)
        where TDbContext : DbContext
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
                    _ => _.MigrationsAssembly(databaseAppSettings.MigrationAssemblyNames[databaseAppSettings.DATABASE_TYPE]));
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
            var conn = new NpgsqlConnectionStringBuilder();
            conn.IncludeErrorDetail = !productionMode;
            conn.Host = databaseAppSettings.DB_HOST;
            conn.Database = databaseAppSettings.DB_DATABASE;
            conn.Port = databaseAppSettings.DB_PORT;
            conn.Username = databaseAppSettings.DB_USERNAME;
            conn.Password = databaseAppSettings.DB_PASSWORD;
            services.AddDbContextFactory<TDbContext>(_ => _
                .UseNpgsql(
                    conn.ConnectionString,
                    _ => _.MigrationsAssembly(databaseAppSettings.MigrationAssemblyNames[databaseAppSettings.DATABASE_TYPE])));
        }
        else
        {
            throw new ArgumentException($"DATABASE_TYPE is invalid: {databaseAppSettings.DATABASE_TYPE}");
        }
    }
}
