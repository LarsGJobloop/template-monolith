using Microsoft.EntityFrameworkCore;

namespace ExampleService.Infrastructure.AppDbContext;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }
}
