using Api.IdentityServer;
using Application.Implementations;
using Application.Interfaces;
using auth_service.IdentityServer;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddApiVersioning();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddUserPostgreStorage(builder.Configuration);
            builder.Services.AddIdentityServerConfiguration(builder.Configuration);
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();
            builder.Services.Configure<UserServiceClientConfig>(options => builder.Configuration.GetSection("UserService").Bind(options));

            var app = builder.Build();
            if (args.Length > 0 && args[0] == "update")
            {
                await UpdateDb(app);
                return;
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }

        private static async Task UpdateDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserCredentialDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}