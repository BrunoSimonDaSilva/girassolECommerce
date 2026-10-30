using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;
using MongoDB.Driver;

namespace GirassolCommerce.Api.Services;

public interface IPedidoService
{
    Task<List<Pedido>> ListarAsync(string usuarioId, bool isAdmin);
    Task<Pedido?> BuscarPorIdAsync(string id, string usuarioId, bool isAdmin);
    Task<Pedido> CriarAsync(string usuarioId, CreatePedidoRequest request, PedidoBusinessService businessService);
    Task<Pedido> AtualizarAsync(string id, string status, string usuarioId, bool isAdmin);
    Task DeletarAsync(string id, string usuarioId, bool isAdmin);
}

public class PedidoService : IPedidoService
{
    private static readonly HashSet<string> StatusPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pendente",
        "Pago",
        "Enviado",
        "Entregue",
        "Cancelado"
    };

    private readonly MongoDbContext _context;

    public PedidoService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pedido>> ListarAsync(string usuarioId, bool isAdmin)
    {
        var filter = isAdmin
            ? Builders<Pedido>.Filter.Empty
            : Builders<Pedido>.Filter.Eq(p => p.UsuarioId, usuarioId);

        return await _context.Pedidos.Find(filter)
            .SortByDescending(p => p.DataPedido)
            .ToListAsync();
    }

    public async Task<Pedido?> BuscarPorIdAsync(string id, string usuarioId, bool isAdmin)
    {
        var pedido = await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (pedido is null)
        {
            return null;
        }

        if (isAdmin || pedido.UsuarioId == usuarioId)
        {
            return pedido;
        }

        return null;
    }

    public async Task<Pedido> CriarAsync(string usuarioId, CreatePedidoRequest request, PedidoBusinessService businessService)
    {
        var produtos = await _context.Produtos.Find(_ => true).ToListAsync();
        var pedido = businessService.CriarPedido(usuarioId, request, produtos);

        foreach (var item in pedido.Itens)
        {
            var produto = produtos.First(p => p.Id == item.ProdutoId);
            produto.Estoque -= item.Quantidade;
            produto.AtualizadoEm = DateTime.UtcNow;
            await _context.Produtos.ReplaceOneAsync(p => p.Id == produto.Id, produto);
        }

        await _context.Pedidos.InsertOneAsync(pedido);
        return pedido;
    }

    public async Task<Pedido> AtualizarAsync(string id, string status, string usuarioId, bool isAdmin)
    {
        var pedido = await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (!isAdmin && pedido.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Você não pode alterar este pedido.");
        }

        if (!StatusPermitidos.Contains(status))
        {
            throw new ArgumentException("Status do pedido inválido.");
        }

        pedido.Status = status;
        await _context.Pedidos.ReplaceOneAsync(p => p.Id == id, pedido);
        return pedido;
    }

    public async Task DeletarAsync(string id, string usuarioId, bool isAdmin)
    {
        var pedido = await _context.Pedidos.Find(p => p.Id == id).FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (!isAdmin && pedido.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Você não pode excluir este pedido.");
        }

        var result = await _context.Pedidos.DeleteOneAsync(p => p.Id == id);
        if (result.DeletedCount == 0)
        {
            throw new KeyNotFoundException("Pedido não encontrado.");
        }
    }
}
