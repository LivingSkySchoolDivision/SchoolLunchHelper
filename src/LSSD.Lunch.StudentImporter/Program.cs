using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace LSSD.Lunch.StudentImporter
{
    public class Program
    {
        static IConfiguration configuration = new ConfigurationBuilder()
                   .AddJsonFile($"appsettings.json", true, true)
                   .AddEnvironmentVariables()
                   .Build();
        static string dbConnectionString = string.Empty;
        static string azkvEndpoint = configuration["KEYVAULT_ENDPOINT"];

        public static void Main(string[] args)
        {
            loadDBConnectionStringFromKeyVault();

            // Check syntax

            // Read in the file

            // Get list of students from DB

            // Find new students

            // Find students to make inactive            
        }

        private static void loadDBConnectionStringFromKeyVault()
        {
            try
            {
                Console.WriteLine("> Connecting to Azure Key Vault...");
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(
                                new KeyVaultClient.AuthenticationCallback(
                                    azureServiceTokenProvider.KeyVaultTokenCallback));
                Console.WriteLine("> Retrieving secrets...");
                IConfiguration conf = new ConfigurationBuilder()
                    .AddAzureKeyVault(azkvEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager())
                    .Build();

                string connstr = conf.GetConnectionString("InternalDatabase");

                if (string.IsNullOrEmpty(connstr))
                {
                    Console.WriteLine("> ERROR: Couldn't find a connection string!");
                    Thread.Sleep(3000);
                }
                else
                {
                    Console.WriteLine("> Setting connection string...");
                    dbConnectionString = connstr;
                }
            } catch(Exception ex)
            {
                Console.WriteLine("> ERROR: " + ex.Message);
            }
        }
    }
}