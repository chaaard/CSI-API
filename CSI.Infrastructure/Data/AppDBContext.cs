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
            CustomerCodes = Set<CustomerCodes>();
            Category = Set<Category>();
            Analytics = Set<Analytics>();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<CustomerCodes> CustomerCodes { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Analytics> Analytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("tbl_user");

            modelBuilder.Entity<Department>()
                .ToTable("tbl_department_code");

            modelBuilder.Entity<CustomerCodes>()
                .ToTable("tbl_customer");

            modelBuilder.Entity<Category>()
                .ToTable("tbl_category");

            modelBuilder.Entity<Analytics>()
               .ToTable("tbl_analytics");
        }
    }
}
