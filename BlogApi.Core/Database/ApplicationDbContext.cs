using BlogApi.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Core.Database
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Author>().ToTable("Authors");

            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.Guid).HasDefaultValueSql("NEWID()");

                entity.HasIndex(e => e.Guid).IsUnique();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            builder.Entity<UserRole>(entity =>
            {
                entity.HasOne(ur => ur.Role).WithMany(r => r!.UserRoles).HasForeignKey(ur => ur.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.User).WithMany(u => u!.UserRoles).HasForeignKey(ur => ur.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Blog>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

                entity.Property(e => e.Slug).IsRequired();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });


            builder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);

                entity.Property(e => e.Slug).IsRequired();
            });

            builder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Title).IsRequired();

                entity.Property(e => e.Content).IsRequired();

                entity.HasOne(n => n.Recipient).WithMany(u => u.Notifications).HasForeignKey(n => n.RecipientId);
            });
        }
    }
}
