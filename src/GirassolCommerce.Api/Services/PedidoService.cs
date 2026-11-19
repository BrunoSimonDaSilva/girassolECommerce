using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;

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

    private readonly ICommerceStore _store;

    public PedidoService(ICommerceStore store)
    {
        _store = store;
    }

    public async Task<List<Pedido>> ListarAsync(string usuarioId, bool isAdmin)
    {
        var pedidos = await _store.ListarPedidosAsync();
        return pedidos
            .Where(p => isAdmin || p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.DataPedido)
            .ToList();
    }

    public async Task<Pedido?> BuscarPorIdAsync(string id, string usuarioId, bool isAdmin)
    {
        var pedido = await _store.BuscarPedidoAsync(id);
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
        var produtos = await _store.ListarProdutosAsync();
        var pedido = businessService.CriarPedido(usuarioId, request, produtos);

        foreach (var item in pedido.Itens)
        {
            var produto = produtos.First(p => p.Id == item.ProdutoId);
            produto.Estoque -= item.Quantidade;
            produto.AtualizadoEm = DateTime.UtcNow;
            await _store.AtualizarProdutoAsync(produto);
        }

        await _store.InserirPedidoAsync(pedido);
        return pedido;
    }

    public async Task<Pedido> AtualizarAsync(string id, string status, string usuarioId, bool isAdmin)
    {
        var pedido = await _store.BuscarPedidoAsync(id)
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
        await _store.AtualizarPedidoAsync(pedido);
        return pedido;
    }

    public async Task DeletarAsync(string id, string usuarioId, bool isAdmin)
    {
        var pedido = await _store.BuscarPedidoAsync(id)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (!isAdmin && pedido.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Você não pode excluir este pedido.");
        }

        if (!await _store.DeletarPedidoAsync(id))
        {
            throw new KeyNotFoundException("Pedido não encontrado.");
        }
    }
}
