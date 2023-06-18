using Application.Dtos;
using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Polly;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using System.Net;
using System.IdentityModel.Tokens.Jwt;

namespace auth_service.Controllers
{
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            await _authService.RegisterUserAsync(userDto);
            return CreatedAtAction(nameof(Exchange), null, new { username = userDto.Login });
        }

        [HttpPost("token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (!request.IsPasswordGrantType())
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidRequest,
                    ErrorDescription = "Указанный grant type не поддерживается."
                });
            }

            // Validate the user credentials.
            var valid = await _authService.VerifyUserAsync(request.Username, request.Password);
            if (!valid)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidGrant,
                    ErrorDescription = "Некорретный логин и/или пароль."
                });
            }

            var identity = new ClaimsIdentity(request.GrantType);
            identity.AddClaim(new Claim(Claims.Subject, request.Username));

            // Чтобы логин матчился на имя.
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/28dc4da0083e34a412b383c67f5c83e1d7678bb6/src/System.IdentityModel.Tokens.Jwt/ClaimTypeMapping.cs#L28
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.UniqueName, request.Username));

            //foreach (var role in roles)
            //{
            //  identity.AddClaim(new Claim(Claims.Role, role));
            //}

            identity.SetScopes(request.GetScopes());
            identity.SetResources(request.Resources);
            identity.SetDestinations(_ => ImmutableArray.Create(new[] { Destinations.AccessToken }));


            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
