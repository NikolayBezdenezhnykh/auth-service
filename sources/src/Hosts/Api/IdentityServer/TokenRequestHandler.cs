using Microsoft.IdentityModel.JsonWebTokens;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;
using System.Collections.Immutable;
using System.Security.Claims;
using OpenIddict.Abstractions;
using Application.Interfaces;

namespace Api.IdentityServer
{
    public class TokenRequestHandler : IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        private readonly IAuthService _authService;
        private static readonly ImmutableArray<string> TokenDestinations = ImmutableArray.Create(new[] { Destinations.AccessToken });

        public TokenRequestHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async ValueTask HandleAsync(HandleTokenRequestContext context)
        {
            if (!context.Request.IsPasswordGrantType())
            {
                context.Reject("Указанный grant type не поддерживается.");
                return;
            }

            // Validate the user credentials.
            var valid = await _authService.VerifyUserAsync(context.Request.Username, context.Request.Password);
            if (!valid)
            {
                context.Reject("Некорретный логин и/или пароль.");
                return;
            }

            var identity = new ClaimsIdentity(context.Request.GrantType);
            identity.AddClaim(new Claim(Claims.Subject, "login"));

            // Чтобы логин матчился на имя.
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/28dc4da0083e34a412b383c67f5c83e1d7678bb6/src/System.IdentityModel.Tokens.Jwt/ClaimTypeMapping.cs#L28
            // identity.AddClaim(new Claim(JwtRegisteredClaimNames.UniqueName, "login"));

            //foreach (var role in roles)
            //{
            //  identity.AddClaim(new Claim(Claims.Role, role));
            //}

            identity.SetScopes(context.Request.GetScopes());
            identity.SetResources(context.Request.Resources);
            identity.SetDestinations(_ => TokenDestinations);

            context.Principal = new ClaimsPrincipal(identity);

            return;
        }
    }
}
