using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CSC595_Week04.Models;
using Microsoft.EntityFrameworkCore;
using CSC595_Week04.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IProductService, FakeProductService>();

builder.Services.AddDbContext<ProductDBContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("myDB"));
});

builder.Services.AddDefaultIdentity<ProductUser>(
    options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 5;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
        }
    ).AddEntityFrameworkStores < ProductDBContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});


var app = builder.Build();
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<ProductDBContext>();
//context.Database.EnsureDeleted();
context.Database.EnsureCreated();


if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithRedirects("/Home/Error");
}



app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.UseRouting();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "delete",
        pattern: "Product/DeleteConfirmed/{id?}",
        defaults: new { controller = "Product", action = "DeleteConfirmed" });
    /*
   endpoints.MapControllerRoute(
       name: "products",
       pattern: "Product/{ListAll}/{id?}",
       defaults: new { controller = "Product" });

   endpoints.MapControllerRoute(
       name: "product",
       pattern: "product/{id:int}",
       defaults: new { controller = "Product",action="ListAll" });

   endpoints.MapControllerRoute(
       name: "attribute-route",
       pattern: "attribute/route",
       defaults: new { controller = "Home", action = "AttributeRouteDemo" });
   */



/*
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Run(async context =>
{
    await context.Response.WriteAsync("Default route");
});
*/


app.Run();
