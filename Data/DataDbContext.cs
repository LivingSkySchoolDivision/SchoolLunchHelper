using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lunch_project.Classes
{
    /**<summary>This DbContext class creates the database structure based on the model classes, 
     * migrations will be based off of it</summary>
     */
    public class DataDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<School> Schools { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) //automatically called for each instance
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(connectionString)
                //base.OnConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Fluent API stuff
        }

    }
}
