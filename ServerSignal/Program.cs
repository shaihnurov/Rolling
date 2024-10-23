using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.CookiePolicy;
using ServerSignal.Extensions;
using ServerSignal.Hub;
using ServerSignal.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddScoped<RentalCarHub>();
builder.Services.AddSingleton<SubscribeRentalCarsTableDependency>();
builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth/Login";
});

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.UseHsts();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapHub<RegisterHub>("/registerhub");
app.MapHub<AuthHub>("/authhub");
app.MapHub<UserProfileHub>("/userprofilehub");
app.MapHub<RentalCarHub>("/rentalcarhub");
app.MapHub<ChatHub>("/chat");

app.UseSqlTableDependency<SubscribeRentalCarsTableDependency>(connectionString);

await app.RunAsync();
