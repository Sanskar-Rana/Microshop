using Microshop.Authentication.Data.ViewModel;
using Microshop.SharedLibrary.Response;

namespace Microshop.Authentication.Service.IRepository;

public interface IApplicationUserRepository
{
    Task<Response> RegisterUser(RegisterVM model);
    Task<Response> LoginUser(LoginVM model);
    Task<Response> FindUserByEmail(string email);
}