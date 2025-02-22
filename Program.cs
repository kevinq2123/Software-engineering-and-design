using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using RochaBlogs.Configuration;
using RochaBlogs.Data;
using RochaBlogs.Models;
using RochaBlogs.Services;
using RochaBlogs.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);


IConnectionService connectionService = new DefaultConnectionService();

DefaultConnectionService connectionS = new DefaultConnectionService();

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//options.UseNpgsql(connectionService.GetConnectionString(builder.Configuration)));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseNpgsql(builder.Configuration.GetConnectionString("BlogDb")));



builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register Identity class for authentication
builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add services to the container.

// Register DataService 
builder.Services.AddScoped<DataService>();

// Register BlogSearchService
builder.Services.AddScoped<BlogSearchService>();

// Register pre-configured instance of MailSettings class
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Register EmailService
builder.Services.AddScoped<IBlogEmailSender, EmailService>();

// Register ImageService
builder.Services.AddScoped<IImageService, DefaultImageService>();

// Register SlugService
builder.Services.AddScoped<ISlugService, DefaultSlugService>();

var app = builder.Build();

var dataService = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataService>();
await dataService.ManageDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseMigrationsEndPoint();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
