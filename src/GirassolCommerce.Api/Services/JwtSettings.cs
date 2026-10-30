namespace GirassolCommerce.Api.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = "dev-secret-key-muda-antes-de-produzir";
    public string Issuer { get; set; } = "GirassolCommerce";
    public string Audience { get; set; } = "GirassolCommerceUsers";
    public int ExpirationMinutes { get; set; } = 120;
}
