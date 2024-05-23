namespace mark.davison.common.server;

[ExcludeFromCodeCoverage]
public static class DependencyInversionExtensions
{
    public static IServiceCollection UseRateLimiter(this IServiceCollection services)
    {
        services.AddTransient<IRateLimitServiceFactory, RateLimitServiceFactory>();
        return services;
    }

    public static IServiceCollection UseNotificationHub(this IServiceCollection services)
    {
        services.AddSingleton<INotificationHub, NotificationHub>();
        return services;
    }

    public static IServiceCollection UseConsoleNotifications(this IServiceCollection services)
    {
        services.AddSingleton<IConsoleNotificationService, ConsoleNotificationService>();
        services.AddSingleton<INotificationService>(_ => _.GetRequiredService<IConsoleNotificationService>());
        return services;
    }

    public static IServiceCollection UseMatrixNotifications(this IServiceCollection services)
    {
        services.AddSingleton<IMatrixNotificationService, MatrixNotificationService>();
        services.AddSingleton<INotificationService>(_ => _.GetRequiredService<IMatrixNotificationService>());
        services.UseMatrixClient();
        return services;
    }
    public static IServiceCollection UseMatrixClient(this IServiceCollection services)
    {
        services.AddSingleton<IMatrixClient, MatrixClient>();
        services.AddHttpClient(MatrixConstants.HttpClientName);
        return services;
    }

    public static IServiceCollection UseRedisCache(this IServiceCollection services,
        RedisAppSettings redisSettings,
        string appName,
        bool productionMode)
    {

        if (string.IsNullOrEmpty(redisSettings.PASSWORD) ||
            string.IsNullOrEmpty(redisSettings.HOST))
        {
            services
                .AddDistributedMemoryCache();
        }
        else
        {
            var config = new ConfigurationOptions
            {
                EndPoints = { redisSettings.HOST + ":" + redisSettings.PORT },
                Password = redisSettings.PASSWORD
            };
            IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(config);
            services.AddStackExchangeRedisCache(_ =>
            {
                _.InstanceName = appName.Replace('-', '_') + "_" + (productionMode ? "prod_" : "dev_");
                _.Configuration = redis.Configuration;
            });
            services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtectionKeys");
            services.AddSingleton(redis);
        }
        return services;
    }

    public static IServiceCollection UseRedisSession(this IServiceCollection services,
        AuthAppSettings authSettings,
        RedisAppSettings redisSettings,
        string appName,
        bool productionMode)
    {
        services
            .AddSession(o =>
            {
                o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.Name = authSettings.SESSION_NAME;
                o.Cookie.HttpOnly = true;
            });

        services
            .UseRedisCache(redisSettings, appName, productionMode);
        return services;
    }


    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services,
        AuthAppSettings authAppSettings)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authAppSettings.AUTHORITY,
                    ValidateAudience = !string.IsNullOrEmpty(authAppSettings.AUDIENCE),
                    ValidAudience = string.IsNullOrEmpty(authAppSettings.AUDIENCE) ? null : authAppSettings.AUDIENCE,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = false,
                    ClockSkew = TimeSpan.Zero,
                    SignatureValidator = (token, _) => new JsonWebToken(token),
                    RequireExpirationTime = true,
                };
            });

        return services;
    }

    public static IServiceCollection UseCookieOidcAuth(
        this IServiceCollection services,
        AuthAppSettings authAppSettings,
        ClaimsAppSettings claimsAppSettings,
        string apiEndpoint)
    {
        return services.UseCookieOidcAuth(authAppSettings, claimsAppSettings, _ => { }, apiEndpoint);
    }

    public static IServiceCollection UseCookieOidcAuth(
        this IServiceCollection services,
        AuthAppSettings authAppSettings,
        ClaimsAppSettings claimsAppSettings,
        Action<AuthenticationConfiguration> configure,
        string apiEndpoint)
    {
        var config = new AuthenticationConfiguration();
        configure(config);

        services
            .AddAuthentication(_ =>
            {
                _.DefaultScheme = AuthConstants.CookiesScheme;
                _.DefaultChallengeScheme = AuthConstants.OidcScheme;
                _.DefaultSignOutScheme = AuthConstants.OidcScheme;
            })
            .AddOpenIdConnect(AuthConstants.OidcScheme, _ =>
            {
                _.Authority = authAppSettings.AUTHORITY;
                _.ClientId = authAppSettings.CLIENT_ID;
                _.ClientSecret = authAppSettings.CLIENT_SECRET;

                _.TokenValidationParameters = new()
                {
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(0)
                };

                _.Events = new()
                {
                    OnRedirectToIdentityProvider = (ctx) =>
                    {
                        if (ctx.ProtocolMessage.RedirectUri.StartsWith(AuthConstants.HttpProto))
                        {
                            ctx.ProtocolMessage.RedirectUri = ctx.ProtocolMessage.RedirectUri.Replace(AuthConstants.HttpProto, AuthConstants.HttpsProto);
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async (Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext ctx) =>
                    {
                        if (ctx.Principal != null && ClaimsPrincipalHelpers.ExtractUser(ctx.Principal, claimsAppSettings) is User user)
                        {
                            var repository = ctx.HttpContext.RequestServices.GetRequiredService<IHttpRepository>();
                            var access = ctx.TokenEndpointResponse?.AccessToken;

                            if (!string.IsNullOrEmpty(access))
                            {
                                var createdAt = DateTime.UtcNow;
                                user.Created = createdAt;
                                var upsertedUser = await repository.UpsertEntityAsync<User>(user, HeaderParameters.Auth(access, null), CancellationToken.None);

                                if (upsertedUser?.Created == createdAt)
                                {
                                    if (config.OnUserCreated != null)
                                    {
                                        await config.OnUserCreated(ctx.HttpContext.RequestServices, upsertedUser);
                                    }
                                }
                            }
                        }
                    }
                };

                _.ResponseType = OpenIdConnectResponseType.Code;
                _.GetClaimsFromUserInfoEndpoint = true;
                _.SaveTokens = true;
                _.Scope.Clear();

                foreach (var scope in authAppSettings.SCOPES)
                {
                    _.Scope.Add(scope);
                }
            })
            .AddCookie(AuthConstants.CookiesScheme, _ =>
            {
                _.ExpireTimeSpan = TimeSpan.FromHours(16);
                _.SlidingExpiration = false;
                _.Cookie.Name = $"__{authAppSettings.SESSION_NAME}".ToLower();
                _.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                _.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                _.LogoutPath = AuthConstants.LogoutPath;
                _.LoginPath = AuthConstants.LoginPath;
            });

        services
            .AddSingleton<IHttpRepository>(_ =>
            {
                var options = SerializationHelpers.CreateStandardSerializationOptions();
                var factory = _.GetRequiredService<IHttpClientFactory>();
                return new HttpRepository(apiEndpoint, factory.CreateClient(AuthConstants.AuthHttpClient), options);
            })
            .AddHttpClient(AuthConstants.AuthHttpClient);


        return services;
    }

    public static IServiceCollection AddZenoAuthentication(this IServiceCollection services, Action<ZenoAuthOptions> setupAction)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        services.AddOptions();
        services.Configure(setupAction);
        services.AddHttpClient(ZenoAuthenticationConstants.ProxyHttpClientName);
        services.AddHttpClient(ZenoAuthenticationConstants.AuthClientName);
        services.AddTransient<IZenoAuthenticationSession, ZenoAuthenticationSession>();
        services.AddHostedService<OpenIdConnectResolver>();

        return services;
    }

    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthCheckServices<GenericApplicationHealthStateHostedService>();

        return services;
    }

    public static IServiceCollection AddHealthCheckServices<THealthHosted>(this IServiceCollection services)
        where THealthHosted : class, IApplicationHealthStateHostedService
    {
        services.AddSingleton<IApplicationHealthState, ApplicationHealthState>();

        services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>(StartupHealthCheck.Name)
            .AddCheck<LiveHealthCheck>(LiveHealthCheck.Name)
            .AddCheck<ReadyHealthCheck>(ReadyHealthCheck.Name);

        services.AddHostedService<THealthHosted>();

        return services;
    }

    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapHealthChecks("/health/startup", new HealthCheckOptions
            {
                Predicate = r => r.Name == StartupHealthCheck.Name
            })
            .AllowAnonymous();
        endpoints
            .MapHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name == LiveHealthCheck.Name
            })
            .AllowAnonymous();
        endpoints
            .MapHealthChecks("/health/readiness", new HealthCheckOptions
            {
                Predicate = r => r.Name == ReadyHealthCheck.Name
            })
            .AllowAnonymous();

        return endpoints;
    }

    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService
    {
        var config = new ScheduleConfig<T>();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
        }

        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }

}