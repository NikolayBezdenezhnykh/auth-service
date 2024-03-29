using Api.IdentityServer;
using Application.Implementations;
using Application.Interfaces;
using auth_service.IdentityServer;
using Infrastructure;
using Infrastructure.KafkaProducer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddUserPostgreStorage(builder.Configuration);
            if (args.Length > 0 && args[0] == "update")
            {
                await UpdateDb(builder.Build());
                return;
            }

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddApiVersioning();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddIdentityServerConfiguration(builder.Configuration, builder.Environment);
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
            builder.Services.Configure<KafkaProducerConfig>(options => builder.Configuration.GetSection("KafkaProducer").Bind(options));

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }

        private static async Task UpdateDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}