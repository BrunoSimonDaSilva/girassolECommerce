using GirassolCommerce.Api.Models;
using MongoDB.Driver;

namespace GirassolCommerce.Api.Data;

public class MongoCommerceStore : ICommerceStore
{
    private readonly MongoDbContext _context;

    public MongoCommerceStore(MongoDbContext context)
    {
        _context = context;
    }

    public Task<List<Produto>> ListarProdutosAsync() =>
        _context.Produtos.Find(_ => true).ToListAsync();

    public async Task<Produto?> BuscarProdutoAsync(string id) =>
        await _context.Produtos.Find(p => p.Id == id).FirstOrDefaultAsync();

    public Task InserirProdutoAsync(Produto produto) =>
        _context.Produtos.InsertOneAsync(produto);

    public Task AtualizarProdutoAsync(Produto produto) =>
        _context.Produtos.ReplaceOneAsync(p => p.Id == produto.Id, produto);

    public async Task<bool> DeletarProdutoAsync(string id) =>
        (await _context.Produtos.DeleteOneAsync(p => p.Id == id)).DeletedCount > 0;

    public Task<List<Pedido>> ListarPedidosAsync() =>
        _context.Pedidos.Find(_ => true).ToListAsync();

    public async Task<Pedido?> BuscarPedidoAsync(string id) =>
        await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync();

    public Task InserirPedidoAsync(Pedido pedido) =>
        _context.Pedidos.InsertOneAsync(pedido);

    public Task AtualizarPedidoAsync(Pedido pedido) =>
        _context.Pedidos.ReplaceOneAsync(p => p.Id == pedido.Id, pedido);

    public async Task<bool> DeletarPedidoAsync(string id) =>
        (await _context.Pedidos.DeleteOneAsync(p => p.Id == id)).DeletedCount > 0;

    public Task<List<Usuario>> ListarUsuariosAsync() =>
        _context.Usuarios.Find(_ => true).ToListAsync();

    public async Task<Usuario?> BuscarUsuarioPorEmailAsync(string email) =>
        await _context.Usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();

    public Task InserirUsuarioAsync(Usuario usuario) =>
        _context.Usuarios.InsertOneAsync(usuario);
}
