using Microsoft.EntityFrameworkCore;
using ExampleService.Domain.FeatureFlags;

namespace ExampleService.Infrastructure.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
}
