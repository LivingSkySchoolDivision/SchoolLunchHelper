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


        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) //do not use this to directly create an instance of this class
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
