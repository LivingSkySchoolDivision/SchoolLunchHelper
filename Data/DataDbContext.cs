using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lunch_project.Classes
{
    /**<summary>Used to create database migrations, gets data from and sends data to the database.</summary>
     */
    public class DataDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<School> Schools { get; set; }

        private string connectionString;

        
        /**<remarks>This is only meant to be used by ef core's designer, do not use this constructor manually</remarks>
         */
        /*
        public DataDbContext() //see https://go.microsoft.com/fwlink/?linkid=851728
        {
        }
        */

        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options)
        {
        }


        
        /**<remarks>This is the constructor that should be used</remarks>
         */
        public DataDbContext(string connectionString)
        {//this saves the connection string in a field for OnConfiguring to use
            this.connectionString = connectionString;
        }

        /**<summary>Configures connection to the database, automatically called for each instance</summary>
         */
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (connectionString != null)
                {
                    //optionsBuilder.UseSqlServer(connectionString);
                    optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("LunchAPI"));
                    connectionString = null; //the connection string isn't needed as a field anymore

                }
                else //if the connection string is null, configure the options for the designer to make migrations
                {
                    optionsBuilder.UseSqlServer(b => b.MigrationsAssembly("LunchAPI"));
                    //optionsBuilder.UseSqlServer();
                }
            }
            
            //optionsBuilder.UseInMemoryDatabase("testDb");

            /*
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(connectionString);
                //base.OnConfiguring(optionsBuilder);

            }
            */
        }



        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .Property(x => x.Balance)
                .HasComputedColumnSql("");//this is handled by the balance calculator
        }
        */



    }
}
