using CatalogAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<LibraryItem> LibraryItems => Set<LibraryItem>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(e =>
        {
            e.ToTable("games");
            e.HasKey(g => g.Id);
            e.Property(g => g.Title).IsRequired().HasMaxLength(200);
            e.Property(g => g.Description).HasMaxLength(2000);
            e.Property(g => g.Genre).HasMaxLength(100);
            e.Property(g => g.Price).HasColumnType("numeric(10,2)");
        });

        modelBuilder.Entity<LibraryItem>(e =>
        {
            e.ToTable("library_items");
            e.HasKey(l => l.Id);
            e.Property(l => l.GameTitle).IsRequired().HasMaxLength(200);
            e.Property(l => l.PricePaid).HasColumnType("numeric(10,2)");
            e.HasIndex(l => new { l.UserId, l.GameId }).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.GameTitle).IsRequired().HasMaxLength(200);
            e.Property(o => o.Price).HasColumnType("numeric(10,2)");
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
