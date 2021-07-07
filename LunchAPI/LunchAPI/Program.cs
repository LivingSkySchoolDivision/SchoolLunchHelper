using Azure.Security.KeyVault.Secrets;
using lunch_project.Classes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;



namespace LunchAPI
{
    public static class Program
    {
        private static string GetKeyVaultEndpoint() => Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT");
        private static KeyVaultClient kvc;
        
        //fields for EF Core's designer
        private static string kve; //temporarily holds the keyvaultEndpoint for the designer, also use for running the API in the cmd for testing
        private static string ConnectionString; //temporarily holds the connection string for the designer
        private static bool inDesignerMode = false; //set to true if the program should run in designer mode - since the program doesn't start normally in designer mode, methods have to run differently
        private static IHost programHost; 


        public static void Main(string[] args) 
        {
            //CreateHostBuilder(args).Build().Run();
            if (!inDesignerMode)
            {
                Console.WriteLine("not in designer mode - Main()");
                programHost = CreateHostBuilder(args).Build(); //initialize the host. CreateHostBuilder has to initialize the fields for the other methods to use
                MakeDbContext().Wait();
            }
            else //DEBUG
            {
                Console.WriteLine("in designer mode - Main()");
            }
            
            //MakeDbContext().Wait(); //blocks the thread until the data context is initialized, if anything happens before this is done there will be errors
            //printSecrets_DEBUG().Wait(); //DEBUG
            programHost.Run(); //after the other methods are finished, run the host builder. Blocks the main thread until shutdown
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    string keyVaultEndpoint;
                    if (inDesignerMode)
                    {
                        keyVaultEndpoint = kve;
                        kve = null;
                    }
                    else
                    {
                        keyVaultEndpoint = GetKeyVaultEndpoint();
                    }
                    if (keyVaultEndpoint == null) //DEBUG - this is here so the API can run from the cmd (not in debug mode so it can't read from launchSettings.json)
                    {
                        ExeConfigurationFileMap customConfigFileMap = new ExeConfigurationFileMap();
                        customConfigFileMap.ExeConfigFilename = "EfCoreDesignerSettings.config";
                        Configuration customConfig = ConfigurationManager.OpenMappedExeConfiguration(customConfigFileMap, ConfigurationUserLevel.None);
                        AppSettingsSection appSettings = (customConfig.GetSection("appSettings") as AppSettingsSection);
                        keyVaultEndpoint = appSettings.Settings["keyvaultEndpoint"].Value;
                        kve = keyVaultEndpoint;
                    }

                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                    {
                        Console.WriteLine("Retrieving configuration from Azure Key Vault (" + GetKeyVaultEndpoint() + ")");
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));
                        builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());

                        kvc = keyVaultClient;
                        Console.WriteLine("test"); //DEBUG
                        
                    }

                });


        #region EF Core designer methods
        /**<remarks>This is only to be used in DataDbContextFactory for EF Core's designer. Call SaveConnectionString() first</remarks>
         */
        public static string GetConnectionString()
        {
            string connectionString = ConnectionString;
            ConnectionString = null; //the connection string isn't needed anymore
            return connectionString;
        }


        public static async Task SaveConnectionStringAsync(string keyVaultEndpoint)
        {
            inDesignerMode = true;
            kve = keyVaultEndpoint;
            programHost = CreateHostBuilder(null).Build();
            //var keyVaultEndpoint = GetKeyVaultEndpoint(); //this returns null because env vars haven't been loaded from launchSettings.json - ef core's designer won't initialize settings with launchSettings.json
            Console.WriteLine("test2"); //DEBUG
            var connectionStringResponse = await kvc.GetSecretAsync(keyVaultEndpoint, "ConnectionStrings--InternalDatabase");
            Console.WriteLine("test3"); //DEBUG
            string connectionString = connectionStringResponse.Value; 
            Console.WriteLine("test4"); //DEBUG
            ConnectionString = connectionString;
        }
        #endregion


        //makes a new datadbcontext using the contextInjector
        private static async Task MakeDbContext()
        {
            var keyVaultEndpoint = GetKeyVaultEndpoint();
            if (keyVaultEndpoint == null)
            {
                keyVaultEndpoint = kve; //when not in debug mode, the program can't read launchSettings.json to get the environment variables
            }
            var connectionStringResponse = await kvc.GetSecretAsync(keyVaultEndpoint, "ConnectionStrings--InternalDatabase");
            string connectionString = connectionStringResponse.Value;
            ContextInjector.Init(connectionString);
        }


        //makes a new datadbcontext and returns it
        private static async Task<DataDbContext> CreateDataContext()
        {
            var keyVaultEndpoint = GetKeyVaultEndpoint();
            var connectionStringResponse = await kvc.GetSecretAsync(keyVaultEndpoint, "ConnectionStrings--InternalDatabase");
            string connectionString = connectionStringResponse.Value;
            return new DataDbContext(connectionString);
        }




        public static async Task printSecrets_DEBUG() //DEBUG
        {
            //KeyVaultSecret secret = client.GetSecret("<mySecret>");
            //string secretValue = secret.Value;
            var keyVaultEndpoint = GetKeyVaultEndpoint();
            var secrets = kvc.GetSecretsAsync(keyVaultEndpoint);
            var secret1 = kvc.GetSecretAsync(keyVaultEndpoint, "ConnectionStrings--InternalDatabase"); 
            var secret2 = await secret1;
            var secret3 = secret2.Value; //this is the correct one
            Trace.WriteLine("secrets: " + secrets);
            Trace.WriteLine("secret1: " + secret1);
            Trace.WriteLine("secret2: " + secret2);
            Trace.WriteLine("secret3: " + secret3);

            var UriSecret = kvc.GetSecretAsync(keyVaultEndpoint, "LSSDLunch--APIURI"); 
            var UriSecret2 = await UriSecret;
            var UriSecret3 = UriSecret2.Value;
            Trace.WriteLine("UriSecret1: " + UriSecret);
            Trace.WriteLine("UriSecret2: " + UriSecret2);
            Trace.WriteLine("UriSecret3: " + UriSecret3);
            Trace.WriteLine("key vault endpoint: " + GetKeyVaultEndpoint());

        }
    }
}
