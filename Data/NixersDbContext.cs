using Microsoft.EntityFrameworkCore;
using NixersDB.Models;

namespace NixersDB
{
    public class NixersDbContext : DbContext
    {
        public NixersDbContext(DbContextOptions<NixersDbContext> options) : base(options) { }

        public DbSet<UserData> UserData { get; set; }
        public DbSet<CustomerData> CustomerData { get; set; }
        public DbSet<TradesmanData> TradesmanData { get; set; }
        public DbSet<JobApplications> JobApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
