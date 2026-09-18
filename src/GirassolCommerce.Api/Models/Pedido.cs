using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GirassolCommerce.Api.Models;

public class PedidoItem
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProdutoId { get; set; } = string.Empty;

    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}

public class Pedido
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    public string NomeCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string EnderecoEntrega { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente";
    public decimal Total { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public List<PedidoItem> Itens { get; set; } = new();
}
