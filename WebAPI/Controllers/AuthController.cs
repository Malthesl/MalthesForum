using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserRepository users) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDTO)
    {
        User? user = await users.VerifyUserCredentials(loginDTO.Username, loginDTO.Password);
        
        if (user is null) return Unauthorized();
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Id", user.Id.ToString(), ClaimValueTypes.Integer)
        };

        var key = new SymmetricSecurityKey("SuperSecretKeyThatIsAtMinimum32CharactersLong"u8.ToArray());
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MalthesForum",
            audience: "MalthesUsers",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }
}