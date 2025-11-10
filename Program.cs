using Microsoft.AspNetCore.Identity;
using Reconova.Extensions;
using Reconova.Hubs;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
var wkhtmltoxPath = Path.Combine(Directory.GetCurrentDirectory(), "Libraries", "libwkhtmltox.dll");

if (File.Exists(wkhtmltoxPath))
{
    var context = new CustomAssemblyLoadContext();
    context.LoadUnmanagedLibrary(wkhtmltoxPath);
}

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInterfaceScopeServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<CallHub>("/callHub");

app.UseRouting();

app.UseSession();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}");

app.Run();
