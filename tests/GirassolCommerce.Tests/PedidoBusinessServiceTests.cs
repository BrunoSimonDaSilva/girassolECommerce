using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;
using GirassolCommerce.Api.Services;

namespace GirassolCommerce.Tests;

public class PedidoBusinessServiceTests
{
    private readonly PedidoBusinessService _service = new();

    [Fact]
    public void CriarPedido_ComItensValidos_DeveCalcularTotal()
    {
        var produtos = new List<Produto>
        {
            new() { Id = "p1", Nome = "Caneca Girassol", Categoria = "Casa", Preco = 49.90m, Estoque = 10, Ativo = true },
            new() { Id = "p2", Nome = "Camiseta Verde", Categoria = "Moda", Preco = 79.00m, Estoque = 5, Ativo = true }
        };

        var pedido = _service.CriarPedido(
            "user-1",
            new CreatePedidoRequest(
                "Maria",
                "maria@email.com",
                "Rua das Flores, 10",
                new List<PedidoItemRequest>
                {
                    new("p1", 2),
                    new("p2", 1)
                }),
            produtos);

        Assert.Equal("user-1", pedido.UsuarioId);
        Assert.Equal(178.80m, pedido.Total);
        Assert.Equal(2, pedido.Itens.Count);
        Assert.Equal("Pendente", pedido.Status);
    }

    [Fact]
    public void ValidarProduto_ComDadosCorretos_DeveAceitar()
    {
        var produto = new Produto
        {
            Nome = "Kit Floral",
            Categoria = "Decoracao",
            Preco = 35.50m,
            Estoque = 12,
            Ativo = true
        };

        var exception = Record.Exception(() => PedidoBusinessService.ValidarProduto(produto));

        Assert.Null(exception);
    }

    [Fact]
    public void CriarPedido_QuantidadeInvalida_DeveLancarErro()
    {
        var produtos = new List<Produto>
        {
            new() { Id = "p1", Nome = "Caneca Girassol", Categoria = "Casa", Preco = 49.90m, Estoque = 10, Ativo = true }
        };

        var ex = Assert.Throws<ArgumentException>(() => _service.CriarPedido(
            "user-1",
            new CreatePedidoRequest(
                "Maria",
                "maria@email.com",
                "Rua das Flores, 10",
                new List<PedidoItemRequest> { new("p1", 0) }),
            produtos));

        Assert.Contains("quantidade", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CriarPedido_EstoqueInsuficiente_DeveLancarErro()
    {
        var produtos = new List<Produto>
        {
            new() { Id = "p1", Nome = "Caneca Girassol", Categoria = "Casa", Preco = 49.90m, Estoque = 1, Ativo = true }
        };

        var ex = Assert.Throws<InvalidOperationException>(() => _service.CriarPedido(
            "user-1",
            new CreatePedidoRequest(
                "Maria",
                "maria@email.com",
                "Rua das Flores, 10",
                new List<PedidoItemRequest> { new("p1", 2) }),
            produtos));

        Assert.Contains("Estoque insuficiente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
