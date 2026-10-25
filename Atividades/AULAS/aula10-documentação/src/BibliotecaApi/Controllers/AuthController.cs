using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaApi.Controllers;

/// <summary>
/// Endpoint de autenticação para geração de token JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Autenticação")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    /// <summary>
    /// Credenciais para autenticação no sistema.
    /// </summary>
    /// <param name="Usuario">Nome de usuário (ex: bibliotecario).</param>
    /// <param name="Senha">Senha de acesso (ex: newton123).</param>
    public record CredenciaisRequest(string Usuario, string Senha);

    /// <summary>
    /// Dados do token gerado.
    /// </summary>
    /// <param name="Token">Token JWT para uso no cabeçalho Authorization.</param>
    /// <param name="ExpiraEm">Data e hora de expiração.</param>
    public record TokenResponse(string Token, DateTime ExpiraEm);

    /// <summary>
    /// Realiza a autenticação e devolve um token JWT válido.
    /// </summary>
    /// <remarks>
    /// Usuário didático fixo:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "usuario": "bibliotecario",
    ///         "senha": "newton123"
    ///     }
    ///
    /// Copie o token retornado e cole no botão "Authorize" do Swagger UI.
    /// </remarks>
    /// <param name="credenciais">Credenciais do usuário.</param>
    /// <response code="200">Token gerado com sucesso.</response>
    /// <response code="401">Credenciais inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] CredenciaisRequest credenciais)
    {
        if (credenciais.Usuario != "bibliotecario" || credenciais.Senha != "newton123")
            return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });

        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Chave"]!));
        var expiraEm = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Emissor"],
            audience: _config["Jwt:Audiencia"],
            claims: new[]
            {
                new Claim(ClaimTypes.Name, credenciais.Usuario),
                new Claim(ClaimTypes.Role, "Bibliotecario"),
            },
            expires: expiraEm,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));

        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiraEm));
    }
}