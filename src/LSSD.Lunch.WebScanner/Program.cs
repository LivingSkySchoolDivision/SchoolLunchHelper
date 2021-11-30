using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using LSSD.Lunch.WebScanner.Services;
using LSSD.MongoDB;
using LSSD.Lunch;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
                   .AddEnvironmentVariables()
                   .Build();

string GetKeyVaultEndpoint() => Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT") ?? string.Empty;

if (!string.IsNullOrEmpty(GetKeyVaultEndpoint())) 
{
    Console.WriteLine("Retrieving configuration from Azure Key Vault (" + GetKeyVaultEndpoint() + ")");
    var azureServiceTokenProvider = new AzureServiceTokenProvider();
    var keyVaultClient = new KeyVaultClient(
    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

    configuration = new ConfigurationBuilder()
                   .AddEnvironmentVariables()
                   .AddAzureKeyVault(GetKeyVaultEndpoint(), keyVaultClient, new DefaultKeyVaultSecretManager())
                   .Build();
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<FoodItemService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddSingleton<MongoDbConnection>(x => new MongoDbConnection(configuration.GetConnectionString("InternalDatabase")));
builder.Services.AddSingleton<IRepository<School>, MongoRepository<School>>();
builder.Services.AddSingleton<IRepository<Student>, MongoRepository<Student>>();
builder.Services.AddSingleton<IRepository<Transaction>, MongoRepository<Transaction>>();
builder.Services.AddSingleton<IRepository<FoodItem>, MongoRepository<FoodItem>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
