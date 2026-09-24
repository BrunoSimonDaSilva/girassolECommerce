using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GirassolCommerce.Api.Models;

public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Usuario";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
