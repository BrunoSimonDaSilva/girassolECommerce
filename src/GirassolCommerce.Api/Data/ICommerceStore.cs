using GirassolCommerce.Api.Models;

namespace GirassolCommerce.Api.Data;

public interface ICommerceStore
{
    Task<List<Produto>> ListarProdutosAsync();
    Task<Produto?> BuscarProdutoAsync(string id);
    Task InserirProdutoAsync(Produto produto);
    Task AtualizarProdutoAsync(Produto produto);
    Task<bool> DeletarProdutoAsync(string id);

    Task<List<Pedido>> ListarPedidosAsync();
    Task<Pedido?> BuscarPedidoAsync(string id);
    Task InserirPedidoAsync(Pedido pedido);
    Task AtualizarPedidoAsync(Pedido pedido);
    Task<bool> DeletarPedidoAsync(string id);

    Task<List<Usuario>> ListarUsuariosAsync();
    Task<Usuario?> BuscarUsuarioPorEmailAsync(string email);
    Task InserirUsuarioAsync(Usuario usuario);
}
