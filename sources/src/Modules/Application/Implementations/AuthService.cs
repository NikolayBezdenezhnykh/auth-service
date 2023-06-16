using Application.Dtos;
using Application.Interfaces;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Application.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserServiceClient _userServiceClient;
        private readonly UserCredentialDbContext _dbContext;

        public AuthService(UserCredentialDbContext dbContext, IUserServiceClient userServiceClient)
        {
            _dbContext = dbContext;
            _userServiceClient = userServiceClient;
        }

        public async Task RegisterUserAsync(UserDto userDto)
        {
            var userCredential = new UserCredential()
            {
                Login = userDto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            _dbContext.UserCredentials.Add(userCredential);
            await _dbContext.SaveChangesAsync();

            userDto.Password = null;

            await _userServiceClient.CreateUserAsync(userDto);
        }

        public async Task<bool> VerifyUserAsync(string login, string password)
        {
            var user = await _dbContext.UserCredentials.SingleOrDefaultAsync(u => u.Login == login);
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }
}
