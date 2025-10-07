using Microshop.Authentication.Data;
using Microshop.Authentication.Data.Model;
using Microshop.Authentication.Data.ViewModel;
using Microshop.Authentication.Service.IRepository;
using Microshop.SharedLibrary.Response;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Microshop.Authentication.Service.Repository;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserRepository(AppDbContext dbContext,  UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }
    
    public async Task<Response> RegisterUser(RegisterVM model)
    {
        ApplicationUser newUser = new()
        {
            Email = model.Email,
            UserName = model.UserName,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            LastName = model.LastName,
            Role = model.Role,
            SecurityStamp = Guid.NewGuid().ToString(),
            MobileNumber = model.MobileNumber
        };
        var result = await _userManager.CreateAsync(newUser, model.Password);
        
        if(!result.Succeeded)
            return new Response(false, result.Errors.ToString());

        switch (model.Role)
        {
            case "Admin":
                await _userManager.AddToRoleAsync(newUser,UserRole.Admin);
                break;
            default:
                await _userManager.AddToRoleAsync(newUser, UserRole.User);
                break;
        }

        return new Response(true, $"{model.UserName} has been registered");
    }

    public Task<Response> LoginUser(LoginVM model)
    {
        throw new NotImplementedException();
    }

    public  async Task<Response> FindUserByEmail(string email)
    {
       throw new NotImplementedException();
    }
}