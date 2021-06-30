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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;



namespace LunchAPI
{
    public class Program
    {

        private static string GetKeyVaultEndpoint() => Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT");
        private static KeyVaultClient kvc;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            printSecrets_DEBUG().Wait(); //DEBUG - this doesnt print when called here
            //MakeDbContext().Wait(); 
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    var keyVaultEndpoint = GetKeyVaultEndpoint();
                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                    {
                        Console.WriteLine("Retrieving configuration from Azure Key Vault (" + GetKeyVaultEndpoint() + ")");
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));
                        builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());

                        kvc = keyVaultClient; 
                        printSecrets_DEBUG().Wait(); //DEBUG
                    }
                });

        /*
        private async Task<DataDbContext> CreateDataContext()
        {
            var keyVaultEndpoint = GetKeyVaultEndpoint();
            var connectionStringResponse = await kvc.GetSecretAsync(keyVaultEndpoint, "");
            string connectionString = connectionStringResponse.Value;
            return new DataDbContext(connectionString);
        }
        */


        private static async Task MakeDbContext()
        {
            var keyVaultEndpoint = GetKeyVaultEndpoint();
            var connectionStringResponse = await kvc.GetSecretAsync(keyVaultEndpoint, "ConnectionStrings--InternalDatabase");
            string connectionString = connectionStringResponse.Value;
            ContextInjector.Init(connectionString);
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

        }
    }
}
