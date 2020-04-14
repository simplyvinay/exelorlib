using Exelor.Infrastructure.Auditing;
using Exelor.Infrastructure.Auth.Authentication;
using Exelor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.Api.Domain;

namespace Sample.Api.Data
{
    public class SampleDbContext : ApplicationDbContext
    {
        public SampleDbContext(
            DbContextOptions options,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher,
            ILoggerFactory loggerFactory,
            IOptions<AuditSettings> auditSettings) : base(options,
            currentUser,
            passwordHasher,
            loggerFactory,
            auditSettings)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var navigation = builder.Entity<User>()
                .Metadata.FindNavigation(nameof(User.RefreshTokens));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Entity<RefreshToken>()
                .HasOne(d => d.User)
                .WithMany(e => e.RefreshTokens)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasMany(a => a.Roles)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            builder.Entity<Role>()
                .HasMany(a => a.Users)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId);

            builder.Entity<Role>()
                .Property("_permissionsInRole")
                .HasColumnName("PermissionsInRole")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Entity<UserRole>()
                .ToTable("UserRole")
                .HasKey(r => new {r.UserId, r.RoleId});
        }
    }
}