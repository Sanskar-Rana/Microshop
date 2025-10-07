using Microshop.Authentication.Data.Model;
using Microshop.Authentication.Data.ViewModel.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Microshop.Authentication.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
    }
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public static async Task SeedRoles(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(Data.ViewModel.UserRole.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Data.ViewModel.UserRole.Admin));
            }

            if (!await roleManager.RoleExistsAsync(Data.ViewModel.UserRole.User))
            {
                await roleManager.CreateAsync(new IdentityRole(Data.ViewModel.UserRole.User));
            }
        }
    }
}