namespace GirassolCommerce.Api.Dtos;

public record RegisterRequest(
    string Nome,
    string Email,
    string Senha,
    string? Role = null);

public record LoginRequest(
    string Email,
    string Senha);

public record AuthResponse(
    string Id,
    string Nome,
    string Email,
    string Role,
    string Token);
