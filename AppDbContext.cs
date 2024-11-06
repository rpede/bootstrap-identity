using Microsoft.EntityFrameworkCore;

namespace bootstrap_identity;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
