using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FleetLog.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FleetLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
     public IActionResult Login([FromBody] LoginDto login)
    {
        string role;
        string nome;

        if(login.Email == "admin@fleetlog.com" && login.Senha == "Admin@1234")
        {
            role="Admin";
            nome="Administrador Geral";
        }

        else if (login.Email == "operador@fleetlog.com" && login.Senha =="Operador@123")
        {
            role = "Usuario";
            nome = "Operador de Logística";
        }

        else  return Unauthorized(new {mensagem = "Email ou senha invalidos."});
                 
    
        var secret = _configuration["JwtSettings:Secret"]?? "ChavePadraoSeguraSuperSecretaParaTokensJWT2026!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "FleetLogApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "FleetLogApp";
        var expiracaoHoras = int.Parse(_configuration["JwtSettings:ExpiracaoHoras"] ?? "2");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key , SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
          new Claim(JwtRegisteredClaimNames.Sub, login.Email),
          new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
          new Claim(ClaimTypes.Name, nome),
          new Claim(ClaimTypes.Role,role)  
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiracaoHoras),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            tipo = "Bearer",
            expiracao = DateTime.UtcNow.AddHours(expiracaoHoras),
            usuario = new {nome , email = login.Email,role}
        });
     }
    }