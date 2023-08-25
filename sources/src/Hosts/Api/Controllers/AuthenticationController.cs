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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Azure.Core;
using System.Security.Principal;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            var userId = await _authService.RegisterUserAsync(userDto);
            return Ok(new { Id = userId });
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Verify([FromBody] UserVerifyDto userDto)
        {
            if (await _authService.VerifyUserAsync(userDto))
            {
                return Ok();
            }
                
            return BadRequest("Некорректный пароль и/или код.");
        }

        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (!request.IsPasswordGrantType() 
                && !request.IsRefreshTokenGrantType()
                && !request.IsClientCredentialsGrantType())
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidRequest,
                    ErrorDescription = "Указанный grant type не поддерживается."
                });
            }

            ClaimsIdentity identity;
            if (request.IsClientCredentialsGrantType())
            {
                if (!request.ClientId.EndsWith("service"))
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = Errors.InvalidRequest,
                        ErrorDescription = "Указанный grant type для данного клиента не поддерживается."
                    });
                }

                identity = ClaimsIdentityForService(request);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            User user;            

            if (request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                if(!Guid.TryParse(result.Principal.GetClaim(Claims.Subject), out Guid userId))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Refresh token не валиден."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                user = await _authService.GetUserAsync(userId);
                if (user == null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Refresh token не валиден."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                if (user.Status != (int)UserStatus.Activated)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Пользователь заблокирован."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                identity = ClaimsIdentityForUser(request, user);
                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            user = await _authService.GetUserAsync(request.Username);
            if(user == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidGrant,
                    ErrorDescription = "Некорретный email."
                });
            }

            if (user.Status != (int)UserStatus.Activated)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Пользователь не активен."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            
            var isValid = _authService.VerifyPassword(user, request.Password);
            if (!isValid)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidGrant,
                    ErrorDescription = "Некорретный пароль."
                });
            }

            identity = ClaimsIdentityForUser(request, user);
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private ClaimsIdentity ClaimsIdentityForUser(OpenIddictRequest request, User user)
        {
            var identity = new ClaimsIdentity(request.GrantType);
            identity.AddClaim(new Claim(Claims.Subject, user.UserId.ToString()));

            // Чтобы логин матчился на имя.
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/28dc4da0083e34a412b383c67f5c83e1d7678bb6/src/System.IdentityModel.Tokens.Jwt/ClaimTypeMapping.cs#L28
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.UniqueName, user.Email));

            foreach (var role in user.Roles)
            {
                identity.AddClaim(new Claim(Claims.Role, role.Name));
            }

            identity.SetResources(request.Resources);
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(_ => ImmutableArray.Create(new[] { Destinations.AccessToken }));
 
            return identity;
        }

        private ClaimsIdentity ClaimsIdentityForService(OpenIddictRequest request)
        {
            var identity = new ClaimsIdentity(request.GrantType);
            identity.AddClaim(new Claim(Claims.Subject, request.ClientId));

            identity.SetResources(request.Resources);
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(_ => ImmutableArray.Create(new[] { Destinations.AccessToken }));

            return identity;
        }
    }
}
