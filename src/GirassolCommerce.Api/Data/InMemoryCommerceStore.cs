using GirassolCommerce.Api.Models;
using MongoDB.Bson;

namespace GirassolCommerce.Api.Data;

public class InMemoryCommerceStore : ICommerceStore
{
    private readonly object _lock = new();
    private readonly List<Produto> _produtos = new();
    private readonly List<Pedido> _pedidos = new();
    private readonly List<Usuario> _usuarios = new();

    public Task<List<Produto>> ListarProdutosAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_produtos.ToList());
        }
    }

    public Task<Produto?> BuscarProdutoAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_produtos.FirstOrDefault(p => p.Id == id));
        }
    }

    public Task InserirProdutoAsync(Produto produto)
    {
        lock (_lock)
        {
            produto.Id ??= ObjectId.GenerateNewId().ToString();
            _produtos.Add(produto);
        }

        return Task.CompletedTask;
    }

    public Task AtualizarProdutoAsync(Produto produto)
    {
        lock (_lock)
        {
            var index = _produtos.FindIndex(p => p.Id == produto.Id);
            if (index >= 0)
            {
                _produtos[index] = produto;
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeletarProdutoAsync(string id)
    {
        int removed;
        lock (_lock)
        {
            removed = _produtos.RemoveAll(p => p.Id == id);
        }

        return Task.FromResult(removed > 0);
    }

    public Task<List<Pedido>> ListarPedidosAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_pedidos.ToList());
        }
    }

    public Task<Pedido?> BuscarPedidoAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_pedidos.FirstOrDefault(p => p.Id == id));
        }
    }

    public Task InserirPedidoAsync(Pedido pedido)
    {
        lock (_lock)
        {
            pedido.Id ??= ObjectId.GenerateNewId().ToString();
            _pedidos.Add(pedido);
        }

        return Task.CompletedTask;
    }

    public Task AtualizarPedidoAsync(Pedido pedido)
    {
        lock (_lock)
        {
            var index = _pedidos.FindIndex(p => p.Id == pedido.Id);
            if (index >= 0)
            {
                _pedidos[index] = pedido;
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeletarPedidoAsync(string id)
    {
        int removed;
        lock (_lock)
        {
            removed = _pedidos.RemoveAll(p => p.Id == id);
        }

        return Task.FromResult(removed > 0);
    }

    public Task<List<Usuario>> ListarUsuariosAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_usuarios.ToList());
        }
    }

    public Task<Usuario?> BuscarUsuarioPorEmailAsync(string email)
    {
        lock (_lock)
        {
            return Task.FromResult(_usuarios.FirstOrDefault(u => u.Email == email));
        }
    }

    public Task InserirUsuarioAsync(Usuario usuario)
    {
        lock (_lock)
        {
            usuario.Id ??= ObjectId.GenerateNewId().ToString();
            _usuarios.Add(usuario);
        }

        return Task.CompletedTask;
    }
}
