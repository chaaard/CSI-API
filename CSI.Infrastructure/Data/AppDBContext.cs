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
            SalesAnalytics = Set<SalesAnalytics>();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SalesAnalytics> SalesAnalytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("tbl_user");

            modelBuilder.Entity<Department>()
              .ToTable("tbl_department_code");

            modelBuilder.Entity<SalesAnalytics>()
             .ToTable("tbl_sales_analytics");
        }
    }
}
