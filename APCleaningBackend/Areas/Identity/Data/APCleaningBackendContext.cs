
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APCleaningBackend.Areas.Identity.Data;

public class APCleaningBackendContext : IdentityDbContext<ApplicationUser>
{
    public APCleaningBackendContext(DbContextOptions<APCleaningBackendContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

public DbSet<APCleaningBackend.Model.ServiceType> ServiceType { get; set; } = default!;

public DbSet<APCleaningBackend.Model.CleanerDetails> CleanerDetails { get; set; } = default!;

public DbSet<APCleaningBackend.Model.DriverDetails> DriverDetails { get; set; } = default!;

public DbSet<APCleaningBackend.Model.Booking> Booking { get; set; } = default!;

public DbSet<APCleaningBackend.Model.Product> Product { get; set; } = default!;

public DbSet<APCleaningBackend.Model.Notification> Notification { get; set; } = default!;
}
