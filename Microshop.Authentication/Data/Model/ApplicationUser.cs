using Microsoft.AspNetCore.Identity;

namespace Microshop.Authentication.Data.Model;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public long MobileNumber { get; set; }
    public string? Role { get; set; }
}