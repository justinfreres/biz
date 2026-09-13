using FlowBridge.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBridge.Web.Data;

public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options)
{
    public DbSet<ServiceOption> ServiceOptions => Set<ServiceOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var services = modelBuilder.Entity<ServiceOption>();
        services.ToTable("service_options");
        services.HasKey(service => service.Id);
        services.Property(service => service.Slug).HasMaxLength(80).IsRequired();
        services.Property(service => service.Practice).HasMaxLength(80).IsRequired();
        services.Property(service => service.Title).HasMaxLength(120).IsRequired();
        services.Property(service => service.Summary).HasMaxLength(600).IsRequired();
        services.Property(service => service.BulletPoints).HasMaxLength(1800).IsRequired();
        services.Property(service => service.Audience).HasMaxLength(300).IsRequired();
        services.Property(service => service.IsPublished).HasDefaultValue(true);
        services.HasIndex(service => service.Slug).IsUnique().HasDatabaseName("IX_service_options_slug");
        services.HasIndex(service => new { service.IsPublished, service.SortOrder })
            .HasDatabaseName("IX_service_options_published_sort_order");
    }
}
