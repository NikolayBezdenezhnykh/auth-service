using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class InfrastructureHostingExtensions
    {
        public static IServiceCollection AddUserPostgreStorage(
                this IServiceCollection serviceCollection,
                IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            serviceCollection.AddDbContext<UserCredentialDbContext>(builder =>
            {
                builder.UseNpgsql(connectionString);
            });

            return serviceCollection;
        }
    }
}
