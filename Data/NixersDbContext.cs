using Microsoft.EntityFrameworkCore;


namespace NixersDB
{
    public class NixersDbContext : DbContext
    {
        public NixersDbContext(DbContextOptions<NixersDbContext> options) : base(options) { }

        public DbSet<UserData> UserData { get; set; }
        public DbSet<CustomerData> CustomerData { get; set; }
    }
}
