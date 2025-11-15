using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebAPI_ESOChallenge.Features.Authentication.Models;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Data
{
    /// <summary>
    /// Context do banco de dados usando Entity Framework Core
    /// Herda de IdentityDbContext para suporte ao ASP.NET Core Identity
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Tabelas do banco
        public DbSet<Cosmetic> Cosmetics { get; set; } = null!;

        /// <summary>
        /// Configuração das entidades e relacionamentos
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========================================
            // Configuração da entidade Cosmetic
            // ========================================
            builder.Entity<Cosmetic>(entity =>
            {
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.Name)
                    .HasMaxLength(200)
                    .IsRequired();
                
                entity.Property(c => c.Description)
                    .HasMaxLength(1000);

                // Relacionamento 1:1 com CosmeticType (Owned Entity)
                entity.OwnsOne(c => c.Type, type =>
                {
                    type.WithOwner();
                    type.Property(t => t.Value).HasMaxLength(100);
                    type.Property(t => t.DisplayValue).HasMaxLength(100);
                });

                // Relacionamento 1:1 com CosmeticRarity (Owned Entity)
                entity.OwnsOne(c => c.Rarity, rarity =>
                {
                    rarity.WithOwner();
                    rarity.Property(r => r.Value).HasMaxLength(50);
                    rarity.Property(r => r.DisplayValue).HasMaxLength(50);
                });

                // Relacionamento 1:1 opcional com CosmeticSeries (Owned Entity)
                entity.OwnsOne(c => c.Series, series =>
                {
                    series.WithOwner();
                    series.Property(s => s.Value).HasMaxLength(100);
                    series.Property(s => s.Image).HasMaxLength(500);
                });

                // Relacionamento 1:1 com CosmeticImages (Owned Entity)
                entity.OwnsOne(c => c.Images, images =>
                {
                    images.WithOwner();
                    images.Property(i => i.SmallIcon).HasMaxLength(500);
                    images.Property(i => i.Icon).HasMaxLength(500);
                    images.Property(i => i.Featured).HasMaxLength(500);
                });
            });

            // ========================================
            // Configuração das tabelas do Identity
            // Renomear para nomes mais limpos
            // ========================================
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}