using System.ComponentModel.DataAnnotations.Schema;
using Microshop.Authentication.Data.Model;

namespace Microshop.Authentication.Data.ViewModel.Token;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public string JwtId { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; }
}