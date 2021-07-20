using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lunch_project.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;

namespace LunchAPI
{
    public class DataDbContextFactory : IDesignTimeDbContextFactory<DataDbContext>
    {
        public DataDbContext CreateDbContext(string[] args)
        {
            //DEBUG start
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                Console.WriteLine("in development mode"); //this prints in the designer and in run with debug mode
            }
            else
            {
                Console.WriteLine("not in development mode");
            }
            //DEBUG end

            var optionsBuilder = new DbContextOptionsBuilder<DataDbContext>();
            Program.SaveConnectionStringAsync(ReadKeyvaultEndpoint()).Wait(); //waits for the connection string to be saved in a field
            string connectionString = Program.GetConnectionString(); //synchronous method to get the connection string from the program
            Console.WriteLine("test5"); //DEBUG
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("LunchAPI"));
            Console.WriteLine("test6"); //DEBUG

            return new DataDbContext(optionsBuilder.Options);
        }

        
        private string ReadKeyvaultEndpoint() 
        {
            ExeConfigurationFileMap customConfigFileMap = new ExeConfigurationFileMap();
            customConfigFileMap.ExeConfigFilename = "EfCoreDesignerSettings.config";
            Configuration customConfig = ConfigurationManager.OpenMappedExeConfiguration(customConfigFileMap, ConfigurationUserLevel.None);
            AppSettingsSection appSettings = (customConfig.GetSection("appSettings") as AppSettingsSection);
            string keyvaultEndpoint = appSettings.Settings["keyvaultEndpoint"].Value;

            Console.WriteLine(keyvaultEndpoint); //DEBUG
            return keyvaultEndpoint;
        }
        

    }
}
