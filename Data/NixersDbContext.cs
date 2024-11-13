using Microsoft.EntityFrameworkCore;

namespace NixersDB  
{
    public class NixersDbContext : DbContext
    {
        public NixersDbContext(DbContextOptions<NixersDbContext> options) : base(options) { }

        // defining a DbSet for each model class to store in the db
        public DbSet<UserData> UserData { get; set; }
    }
}
