namespace UserAuthAndAuthenticationJWT.Services;
using UserAuthAndAuthenticationJWT.Repository.UserRepository;
using UserAuthAndAuthenticationJWT.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;

public class JWTService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public JWTService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    private async Task<User> UserAuthenticate(Login user)
    {
        if(string.IsNullOrEmpty(user.Email)) return null;

        try
        {
            return await _userRepository.GetBy(user.Email);
        }
        catch(FileNotFoundException ex)
        {
            throw new FileNotFoundException(ex.Message);
        }
    }

    public async Task<string> GenerateJWT(Login user)
    {
        try
        {
            var userAuthenticated = await UserAuthenticate(user);
            if(userAuthenticated == null) return null;

            string issuer = _configuration["JWTConfiguration:Issuer"];
            string audience = _configuration["JWTConfiguration:Audience"];
            string key = _configuration["JWTConfiguration:Key"];
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JWTConfiguration:TokenValidityMins"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, userAuthenticated.Email)
                }),
                Expires = expirationTime,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            return token;
        }
        catch(FileNotFoundException ex)
        {
            throw new FileNotFoundException(ex.Message);
        }

    }
}
