using Microsoft.Extensions.Options;
using OpenIddict.Server;
using System.Runtime;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace auth_service.IdentityServer
{
    public class TokenRequestValidator : IOpenIddictServerHandler<ValidateTokenRequestContext>
    {
        private readonly IdentityServerConfig _identityServerConfig;

        public TokenRequestValidator(IOptions<IdentityServerConfig> options)
        {
            _identityServerConfig = options.Value;
        }

        public ValueTask HandleAsync(ValidateTokenRequestContext context)
        {
            if (context.ClientId != _identityServerConfig.ClientId 
                || context.ClientSecret != _identityServerConfig.ClientSecret)
            {
                context.Reject("Клиент не зарегистрирован в системе");
            }
            return default;
        }
    }
}
