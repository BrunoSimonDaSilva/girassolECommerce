using GirassolCommerce.Api.Models;
using MongoDB.Driver;

namespace GirassolCommerce.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Produto> Produtos => _database.GetCollection<Produto>("produtos");
    public IMongoCollection<Pedido> Pedidos => _database.GetCollection<Pedido>("pedidos");
    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("usuarios");
}
