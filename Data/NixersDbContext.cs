using Microsoft.EntityFrameworkCore;
// using NixersDB.Models;

namespace NixersDB
{
    public class NixersDbContext : DbContext
    {
        public NixersDbContext(DbContextOptions<NixersDbContext> options) : base(options) { }

        public DbSet<UserData> UserData { get; set; }
    }
}
