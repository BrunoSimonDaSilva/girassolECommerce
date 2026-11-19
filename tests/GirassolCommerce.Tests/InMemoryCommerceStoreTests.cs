using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Services;

namespace GirassolCommerce.Tests;

public class InMemoryCommerceStoreTests
{
    [Fact]
    public async Task ProdutoService_ComArmazenamentoEmMemoria_DevePermitirCrud()
    {
        var store = new InMemoryCommerceStore();
        var service = new ProdutoService(store);

        var criado = await service.CriarAsync(new CreateProdutoRequest(
            "Caneca Girassol",
            "Caneca de cerâmica",
            "Casa",
            59.90m,
            10));

        Assert.NotNull(criado.Id);
        Assert.Equal(criado.Id, (await service.BuscarPorIdAsync(criado.Id))?.Id);

        var atualizado = await service.AtualizarAsync(criado.Id, new UpdateProdutoRequest(
            null, null, null, 64.90m, null, null, null, null));
        Assert.Equal(64.90m, atualizado.Preco);

        await service.DeletarAsync(criado.Id);
        Assert.Null(await service.BuscarPorIdAsync(criado.Id));
    }
}
