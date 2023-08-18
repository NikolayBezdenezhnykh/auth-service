using Application.Dtos;
using Application.Interfaces;
using Domain;
using Infrastructure;
using Infrastructure.KafkaProducer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Application.Implementations
{
    public class AuthService : IAuthService
    {
        private const string _templateUserActivation = "UserActivation";

        private readonly IKafkaProducer _kafkaProducer;
        private readonly UserDbContext _dbContext;

        public AuthService(UserDbContext dbContext, IKafkaProducer kafkaProducer)
        {
            _dbContext = dbContext;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<Guid> RegisterUserAsync(UserRegisterDto userDto)
        {
            var user = new User()
            {
                Email = userDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Status = (int)UserStatus.Registered,
                UserId = Guid.NewGuid(),
            };

            var roles = await _dbContext.Roles.SingleAsync(r => r.Name == ConstantRoles.Client);
            user.Roles.Add(roles);

            var code = new Random().Next(1000, 9999).ToString();
            user.UserVerifications.Add(new UserVerification()
            {
                Code = BCrypt.Net.BCrypt.HashPassword(code),
                ExpiresDateAt = DateTime.UtcNow.AddMinutes(10),
            });

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // отправляем событие клиент зарегистрировался
            var userRegisteredMsg = new UserRegisterMessage()
            {
                Email = user.Email,
                EmailTo = user.Email,
                Template = _templateUserActivation,
                UserId = user.UserId,
                DynamicParams = new Dictionary<string, string>()
                {
                    ["{{code}}"] = code,
                }
            };

            await _kafkaProducer.PublishMessageAsync(JsonConvert.SerializeObject(userRegisteredMsg));

            return user.UserId;
        }

        public async Task<User> GetUserAsync(string email)
        {
            var user = await _dbContext.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email);

            return user;
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId);

            return user;
        }

        public async Task<bool> VerifyUserAsync(UserVerifyDto userDto)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserVerifications)
                .SingleOrDefaultAsync(u => u.Email == userDto.Email);

            if (user == null) return false;

            var userVerification = user.UserVerifications.Where(v => v.ExpiresDateAt >= DateTime.UtcNow).OrderByDescending(o => o.Id).FirstOrDefault();
            if (userVerification != null
                && BCrypt.Net.BCrypt.Verify(userDto.Code, userVerification.Code))
            {
                userVerification.IsCompleted = true;
                user.Status = (int)UserStatus.Activated;
            }

            await _dbContext.SaveChangesAsync();

            return user.Status == (int)UserStatus.Activated;
        }

        public bool VerifyPassword(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }
}
