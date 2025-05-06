using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using news_project_mvc.Models;
using System;

namespace news_project_mvc.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Setting> Settings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");
                entity.HasMany(c => c.Articles)
                      .WithOne(a => a.Category)
                      .HasForeignKey(a => a.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasIndex(a => a.Slug).IsUnique();
                entity.HasIndex(a => a.AuthorId);
                entity.HasIndex(a => a.CategoryId);
                entity.HasIndex(a => a.PublishedDate);
                entity.HasIndex(a => a.IsPublished);
                entity.Property(a => a.IsPublished).HasDefaultValue(false);
                entity.Property(a => a.ViewCount).HasDefaultValue(0);
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(a => a.Author)
                      .WithMany()
                      .HasForeignKey(a => a.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Setting>(entity =>
            {
            });
        }
    }
}
