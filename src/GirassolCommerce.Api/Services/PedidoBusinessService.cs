using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;

namespace GirassolCommerce.Api.Services;

public class PedidoBusinessService
{
    public Pedido CriarPedido(string usuarioId, CreatePedidoRequest request, IReadOnlyCollection<Produto> catalogo)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            throw new ArgumentException("Usuário é obrigatório para criar um pedido.");
        }

        if (request.Itens is null || request.Itens.Count == 0)
        {
            throw new ArgumentException("Pedido deve conter pelo menos um item.");
        }

        if (string.IsNullOrWhiteSpace(request.EnderecoEntrega))
        {
            throw new ArgumentException("Endereço de entrega é obrigatório.");
        }

        var itensPedido = new List<PedidoItem>();
        decimal total = 0m;

        foreach (var item in request.Itens)
        {
            if (item.Quantidade <= 0)
            {
                throw new ArgumentException($"A quantidade do produto {item.ProdutoId} deve ser maior que zero.");
            }

            var produto = catalogo.FirstOrDefault(p => p.Id == item.ProdutoId)
                ?? throw new KeyNotFoundException($"Produto {item.ProdutoId} não encontrado.");

            if (!produto.Ativo)
            {
                throw new InvalidOperationException($"O produto {produto.Nome} está indisponível no momento.");
            }

            if (produto.Estoque < item.Quantidade)
            {
                throw new InvalidOperationException($"Estoque insuficiente para {produto.Nome}. Disponível: {produto.Estoque}.");
            }

            itensPedido.Add(new PedidoItem
            {
                ProdutoId = produto.Id!,
                NomeProduto = produto.Nome,
                Quantidade = item.Quantidade,
                PrecoUnitario = produto.Preco
            });

            total += produto.Preco * item.Quantidade;
        }

        return new Pedido
        {
            UsuarioId = usuarioId,
            NomeCliente = request.NomeCliente,
            EmailCliente = request.EmailCliente,
            EnderecoEntrega = request.EnderecoEntrega,
            Status = "Pendente",
            Total = total,
            Itens = itensPedido,
            DataPedido = DateTime.UtcNow
        };
    }

    public static void ValidarProduto(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Nome))
        {
            throw new ArgumentException("Nome do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(produto.Categoria))
        {
            throw new ArgumentException("Categoria do produto é obrigatória.");
        }

        if (produto.Preco <= 0)
        {
            throw new ArgumentException("Preço do produto deve ser maior que zero.");
        }

        if (produto.Estoque < 0)
        {
            throw new ArgumentException("Estoque do produto não pode ser negativo.");
        }
    }
}
