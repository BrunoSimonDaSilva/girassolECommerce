using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace GirassolCommerce.Api.Services;

public interface IAuthService
{
    Task<Usuario> RegistrarAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly ICommerceStore _store;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ICommerceStore store, JwtSettings jwtSettings)
    {
        _store = store;
        _jwtSettings = jwtSettings;
    }

    public async Task<Usuario> RegistrarAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ArgumentException("Nome é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            throw new ArgumentException("Email inválido.");
        }

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
        {
            throw new ArgumentException("Senha deve ter pelo menos 6 caracteres.");
        }

        var emailNormalizado = request.Email.Trim();
        var jaExiste = await _store.BuscarUsuarioPorEmailAsync(emailNormalizado) is not null;
        if (jaExiste)
        {
            throw new InvalidOperationException("Usuário já cadastrado.");
        }

        var papel = string.IsNullOrWhiteSpace(request.Role) ? "Usuario" : request.Role.Trim();
        if (papel is not "Usuario" and not "Admin")
        {
            throw new ArgumentException("Perfil inválido. Use Usuario ou Admin.");
        }

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Role = papel,
            CriadoEm = DateTime.UtcNow
        };

        await _store.InserirUsuarioAsync(usuario);
        return usuario;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _store.BuscarUsuarioPorEmailAsync(request.Email.Trim());
        if (usuario is null)
        {
            throw new InvalidOperationException("Credenciais inválidas.");
        }

        var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
        {
            throw new InvalidOperationException("Credenciais inválidas.");
        }

        var token = GerarJwt(usuario);
        return new AuthResponse(
            usuario.Id ?? string.Empty,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            token);
    }

    private string GerarJwt(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id ?? usuario.Email),
            new(ClaimTypes.NameIdentifier, usuario.Id ?? usuario.Email),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
