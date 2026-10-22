using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogAPI.Data;

// Used only by the EF Core CLI tools at design time (migrations/scripts). Does NOT connect.
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=fcg_catalog;Username=postgres;Password=postgres")
            .Options;

        return new CatalogDbContext(options);
    }
}
