using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WebAPI_ESOChallenge.Data.Migrations
{
    public partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("WebAPI_ESOChallenge.Features.Authentication.Models.ApplicationUser", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("varchar(255)");

                b.Property<string>("Email")
                    .HasColumnType("varchar(255)")
                    .HasMaxLength(256);

                b.Property<string>("NormalizedEmail")
                    .HasColumnType("varchar(255)")
                    .HasMaxLength(256);

                b.Property<string>("NormalizedUserName")
                    .HasColumnType("varchar(255)")
                    .HasMaxLength(256);

                b.Property<string>("PasswordHash")
                    .HasColumnType("longtext");

                b.Property<string>("UserName")
                    .HasColumnType("varchar(255)")
                    .HasMaxLength(256);

                b.HasKey("Id");

                b.ToTable("AspNetUsers");
            });

            modelBuilder.Entity("WebAPI_ESOChallenge.Features.Cosmetics.Models.Cosmetic", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("varchar(255)");

                b.Property<string>("Name")
                    .HasColumnType("longtext");

                b.Property<string>("Description")
                    .HasColumnType("longtext");

                b.Property<string>("Rarity")
                    .HasColumnType("longtext");

                b.HasKey("Id");

                b.ToTable("Cosmetics");
            });
        }
    }
}