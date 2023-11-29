using CSI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Infrastructure.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
            Users = Set<User>();
            Departments = Set<Department>();
<<<<<<< Updated upstream
            SalesAnalytics = Set<SalesAnalytics>();
=======
            CustomerCodes = Set<CustomerCodes>();
            Category = Set<Category>();
            Analytics = Set<Analytics>();
            Prooflist = Set<Prooflist>();
            Locations = Set<Location>();
            Status = Set<Status>();
>>>>>>> Stashed changes
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
<<<<<<< Updated upstream
        public DbSet<SalesAnalytics> SalesAnalytics { get; set; }
=======
        public DbSet<CustomerCodes> CustomerCodes { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Analytics> Analytics { get; set; }
        public DbSet<Prooflist> Prooflist { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Status> Status { get; set; }
>>>>>>> Stashed changes

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .ToTable("tbl_user");

            modelBuilder.Entity<Department>()
<<<<<<< Updated upstream
              .ToTable("tbl_department_code");

            modelBuilder.Entity<SalesAnalytics>()
             .ToTable("tbl_sales_analytics");
=======
            .ToTable("tbl_department_code");

            modelBuilder.Entity<CustomerCodes>()
            .ToTable("tbl_customer");

            modelBuilder.Entity<Category>()
            .ToTable("tbl_category");

            modelBuilder.Entity<Analytics>()
            .ToTable("tbl_analytics");

            modelBuilder.Entity<Prooflist>()
            .ToTable("tbl_prooflist");

            modelBuilder.Entity<Location>()
            .ToTable("tbl_location");

            modelBuilder.Entity<Status>()
            .ToTable("tbl_status");
>>>>>>> Stashed changes
        }
    }
}
