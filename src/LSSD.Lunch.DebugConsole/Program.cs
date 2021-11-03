using System;
using System.Collections.Generic;
using System.Linq;
using LSSD.Lunch.Reports;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using NetBarcode;

namespace LSSD.Lunch.DebugConsole
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

            createTestDocument();

            // loadDBConnectionStringFromKeyVault();

        }

        private static void createTestDocument()
        {
            List<Student> Students = new List<Student>()
            {
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1324567",
                    Name = "Jane Smith"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "7654321",
                    Name = "Bruce Wayne"
                },
                new Student() {
                    Id = Guid.NewGuid(),
                    StudentId = "1234567",
                    Name = "Bill Gates"
                },
            };


            using (ReportFactory formFactory = new ReportFactory()) 
            {
                string filename = formFactory.GenerateStudentIDCardSheet(Students);

                if (!string.IsNullOrEmpty(filename)) {
                    Console.WriteLine("Report created: " + filename);
                } else {
                    Console.WriteLine("Something went wrong, and the report could not be created.");
                }
                Console.WriteLine("Press any key to continue (will delete all reports created)...");
                Console.ReadKey();
            }
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
                Console.WriteLine("(Press any key to continue...)");
                Console.ReadKey();
            }
        }
    }
}