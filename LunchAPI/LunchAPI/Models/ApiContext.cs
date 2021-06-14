using LunchAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchAPI.Models
{
    public class ApiContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<School> Schools { get; set; }


        public ApiContext(DbContextOptions<ApiContext> options) : base(options) //do not use this to directly create an instance of this class
        {
        }

        /**<summary>Configures connection to the database, automatically called for each instance</summary>
         */
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(connectionString)
                //base.OnConfiguring(optionsBuilder);
            }
        }


    }
}
