using Microsoft.EntityFrameworkCore;

namespace Application.Infrastructure.Data;

public class AppData : DbContext
{
    public AppData(DbContextOptions<AppData> options) : base(options) { }
}
