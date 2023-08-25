using auth_service.IdentityServer;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Runtime;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Api.IdentityServer
{
    /// <summary>
    /// Класс, содержащий метод для подключения и настройки Identity server.
    /// </summary>
    public static class IdentityServerConfigurationExtensions
    {
        /// <summary>
        /// Добавить в сервисы всё необходимое для работы сервиса как Identity server.
        /// </summary>
        /// <param name="services">DI контейнер.</param>
        /// <param name="configuration">Секция с настройками для сертификата.</param>
        /// <param name="environment">Секция с переменными окружения.</param>
        public static void AddIdentityServerConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services
                .AddOpenIddict()
                .AddServer(options =>
                {
                    // отключаем от хранилища (у нас своё)
                    options.EnableDegradedMode();

                    if (!environment.IsDevelopment())
                    {
                        options.SetIssuer(configuration.GetSection("IdentityServer:Issuer").Value);
                    }

                    // выбираем имя endpoint
                    options
                        .SetTokenEndpointUris("api/v1.0/auth/token");
                    //.SetIntrospectionEndpointUris("introspect");

                    options.RegisterClaims(JwtRegisteredClaimNames.UniqueName);

                    //options.RegisterScopes("scope");

                    options
                        .AllowClientCredentialsFlow()
                        .AllowPasswordFlow()
                        .AllowRefreshTokenFlow();

                    options
                        .DisableAccessTokenEncryption()
                        .AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();

                    // пока только http
                    options
                        .UseAspNetCore()
                        .DisableTransportSecurityRequirement()
                        .EnableTokenEndpointPassthrough();

                    // Register an event handler responsible for validating token requests.
                    options.AddEventHandler<ValidateTokenRequestContext>(builder => builder.UseScopedHandler<TokenRequestValidator>());

                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(10));
                    options.SetRefreshTokenLifetime(TimeSpan.FromMinutes(60));
                });

            services.Configure<IdentityServerConfig>(options => configuration.GetSection("IdentityServer").Bind(options));
        }
    }
}
