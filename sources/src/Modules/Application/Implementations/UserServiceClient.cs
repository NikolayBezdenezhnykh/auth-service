using Application.Dtos;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Application.Implementations
{
    public class UserServiceClient : IUserServiceClient
    {
        private const string _apiCreateUserPath = "api/v1.0/user";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserServiceClientConfig _userServiceClientConfig;

        public UserServiceClient(
            IHttpClientFactory httpClientFactory,
            IOptions<UserServiceClientConfig> options) 
        { 
            _httpClientFactory = httpClientFactory;
            _userServiceClientConfig = options.Value;
        }

        public async Task CreateUserAsync(UserDto userDto)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var uri = new Uri(new Uri(_userServiceClientConfig.ServiceUrl), _apiCreateUserPath);
            using var httpResponseMessage = await httpClient.PostAsJsonAsync(uri, userDto);

            httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}
