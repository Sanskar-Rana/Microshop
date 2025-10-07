using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microshop.Authentication.Data;
using Microshop.Authentication.Data.Model;
using Microshop.Authentication.Data.ViewModel;
using Microshop.Authentication.Data.ViewModel.Token;
using Microshop.Authentication.Service.IRepository;
using Microshop.SharedLibrary.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Microshop.Authentication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthenticationController(UserManager<ApplicationUser> userManager, AppDbContext dbContext, IApplicationUserRepository userRepository, IConfiguration configuration, TokenValidationParameters tokenValidationParameters)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _userRepository = userRepository;
        _configuration = configuration;
        _tokenValidationParameters = tokenValidationParameters;
 
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterVM model)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        var response = await _userRepository.RegisterUser(model);
        
        if(!response.Flag)
            return BadRequest(response);
        return Ok(response);

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginVM model)
    {
        if(!ModelState.IsValid)
            return BadRequest();

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return NotFound();
        
        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (result)
        {
            var tokenValue = await GenerateTokenAsync(user, "");
            return Ok(tokenValue);
        }
        
        return Unauthorized();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM model)
    {
        try
        {
            var result = await VerifyAndGenerateTokenAsync(model);
            if(result == null)
                return BadRequest("Invalid Tokens");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<AuthResultVM> VerifyAndGenerateTokenAsync(TokenRequestVM model)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var tokenInVerification = jwtTokenHandler.ValidateToken(model.Token,_tokenValidationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                if (result == false)
                    return null;
            }
            
            var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value);
            var expiryDate = UnixTimeStampToDateTimeUTC(utcExpiryDate);

            if (expiryDate > DateTime.UtcNow)
                throw new Exception("Token has not expired yet");
            
            var dbRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(n => n.Token == model.RefreshToken);
            if (dbRefreshToken == null)
            {
                throw new Exception("Refresh token doesn't exist in the database");
            }
            else
            {
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                
                if(dbRefreshToken.JwtId != jti)
                    throw new Exception("Refresh token doesn't match");

                if (dbRefreshToken.Expires <= DateTime.UtcNow)
                    throw new Exception("Your refresh token has been expired");
                
                if(dbRefreshToken.IsRevoked)
                    throw new  Exception("Your refresh token has been revoked");

                var userData = await _userManager.FindByIdAsync(dbRefreshToken.UserId);
                var newTokenReponse = GenerateTokenAsync(userData, model.RefreshToken);

                return await newTokenReponse;
            }

        }
        catch (SecurityTokenArgumentException)
        {
            var dbRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(n => n.Token == model.RefreshToken);
            var userData = await _userManager.FindByIdAsync(dbRefreshToken.UserId);
            var newTokenResponse = GenerateTokenAsync(userData, model.RefreshToken);
            
            return await newTokenResponse;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private DateTime UnixTimeStampToDateTimeUTC(long unixTimeStamp)
    {
        var dateTimeValue = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);
        dateTimeValue = dateTimeValue.AddSeconds(unixTimeStamp);
        return dateTimeValue;
    }

    private async Task<AuthResultVM> GenerateTokenAsync(ApplicationUser user, string rToken)
    {
        var authClaims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
           
        }
        
        var authSiginKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            expires: DateTime.UtcNow.AddMinutes(10),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSiginKey, SecurityAlgorithms.HmacSha256));
        
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = new RefreshToken();

        if (string.IsNullOrEmpty(rToken))
        {
            refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user.Id,
                Created = DateTime.Now,
                Expires = DateTime.Now.AddMonths(10),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
            };
            
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        var response = new AuthResultVM()
        {
            Token = jwtToken,
            RefreshToken = (string.IsNullOrEmpty(rToken)) ? refreshToken.Token : rToken,
            Expires = token.ValidTo
        };
        return response;
    }
}