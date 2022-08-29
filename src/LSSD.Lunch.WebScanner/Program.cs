using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using LSSD.Lunch.WebScanner.Services;
using LSSD.MongoDB;
using LSSD.Lunch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddLocalization();

builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<FoodItemService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddSingleton<MongoDbConnection>(x => new MongoDbConnection(builder.Configuration.GetConnectionString("InternalDatabase")));
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

app.UseRequestLocalization("en-CA");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
