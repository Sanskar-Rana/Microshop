namespace Microshop.Authentication.Data.ViewModel.Token;

public class AuthResultVM
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expires { get; set; }
}