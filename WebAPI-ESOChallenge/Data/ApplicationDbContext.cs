using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebAPI_ESOChallenge.Features.Authentication.Models;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cosmetic> Cosmetics { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configuration can be done here
        }
    }
}