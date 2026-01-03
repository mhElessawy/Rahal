using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<RahalWebContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Security setting
    options.Cookie.IsEssential = true; // GDPR compliance
    options.Cookie.Name = ".RahalWeb.Session"; // Custom cookie name
});
// Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<RahalWebContext>();

// Session configuration (added these lines)
builder.Services.AddDistributedMemoryCache(); // Required for session

builder.Services.AddHttpContextAccessor(); // .NET 6+

// Removed duplicate AddControllersWithViews()
builder.Services.AddControllersWithViews();
 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// Important: Authentication before Authorization
app.UseAuthentication(); // You were missing this!
app.UseAuthorization();

// Add session middleware (must be after UseRouting and before Map endpoints)
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PasswordDatums}/{action=login}/{id?}");
app.MapRazorPages();

app.Run();