using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace lunch_project
{
    public sealed class LunchContext: DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        //transactions should not be loaded from the database, only sent

        public LunchContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(connectionString)
                //optionsBuilder.UseLazyLoadingProxies();
                //base.OnConfiguring(optionsBuilder);
            }
        }

        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired(true);
            });

            modelBuilder.Entity<FoodItem>(entity =>
            {
                entity.Property(e => e.ID)
                    .IsRequired(true);
            });
        }
        */

    }
}
