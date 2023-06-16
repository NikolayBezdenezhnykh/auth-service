using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UserCredentialDbContext : DbContext
    {
        public DbSet<UserCredential> UserCredentials { get; set; }

        public UserCredentialDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserCredentialDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
