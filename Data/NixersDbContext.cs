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
        public DbSet<Invoice> Invoices { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

        }
    }
}
