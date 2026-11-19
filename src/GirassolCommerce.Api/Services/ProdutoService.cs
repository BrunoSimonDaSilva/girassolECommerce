using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;

namespace GirassolCommerce.Api.Services;

public interface IProdutoService
{
    Task<List<Produto>> ListarAsync();
    Task<Produto?> BuscarPorIdAsync(string id);
    Task<Produto> CriarAsync(CreateProdutoRequest request);
    Task<Produto> AtualizarAsync(string id, UpdateProdutoRequest request);
    Task DeletarAsync(string id);
    Task DebitarEstoqueAsync(string produtoId, int quantidade);
}

public class ProdutoService : IProdutoService
{
    private readonly ICommerceStore _store;

    public ProdutoService(ICommerceStore store)
    {
        _store = store;
    }

    public async Task<List<Produto>> ListarAsync()
    {
        return (await _store.ListarProdutosAsync())
            .OrderBy(p => p.Nome)
            .ToList();
    }

    public async Task<Produto?> BuscarPorIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return await _store.BuscarProdutoAsync(id);
    }

    public async Task<Produto> CriarAsync(CreateProdutoRequest request)
    {
        var produto = new Produto
        {
            Nome = request.Nome.Trim(),
            Slug = GerarSlug(request.Nome),
            Descricao = request.Descricao.Trim(),
            Categoria = request.Categoria.Trim(),
            Preco = request.Preco,
            Estoque = request.Estoque,
            Ativo = request.Ativo,
            ImagemUrl = request.ImagemUrl ?? string.Empty,
            Tags = request.Tags ?? Array.Empty<string>(),
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };

        PedidoBusinessService.ValidarProduto(produto);
        await _store.InserirProdutoAsync(produto);
        return produto;
    }

    public async Task<Produto> AtualizarAsync(string id, UpdateProdutoRequest request)
    {
        var produto = await BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException("Produto não encontrado.");

        produto.Nome = request.Nome ?? produto.Nome;
        produto.Descricao = request.Descricao ?? produto.Descricao;
        produto.Categoria = request.Categoria ?? produto.Categoria;
        produto.Preco = request.Preco ?? produto.Preco;
        produto.Estoque = request.Estoque ?? produto.Estoque;
        produto.ImagemUrl = request.ImagemUrl ?? produto.ImagemUrl;
        produto.Tags = request.Tags ?? produto.Tags;
        produto.Ativo = request.Ativo ?? produto.Ativo;
        produto.Slug = GerarSlug(produto.Nome);
        produto.AtualizadoEm = DateTime.UtcNow;

        PedidoBusinessService.ValidarProduto(produto);
        await _store.AtualizarProdutoAsync(produto);

        return produto;
    }

    public async Task DeletarAsync(string id)
    {
        if (!await _store.DeletarProdutoAsync(id))
        {
            throw new KeyNotFoundException("Produto não encontrado.");
        }
    }

    public async Task DebitarEstoqueAsync(string produtoId, int quantidade)
    {
        var produto = await BuscarPorIdAsync(produtoId)
            ?? throw new KeyNotFoundException("Produto não encontrado.");

        if (produto.Estoque < quantidade)
        {
            throw new InvalidOperationException($"Estoque insuficiente para {produto.Nome}.");
        }

        produto.Estoque -= quantidade;
        produto.AtualizadoEm = DateTime.UtcNow;
        await _store.AtualizarProdutoAsync(produto);
    }

    private static string GerarSlug(string value)
    {
        var slug = value.Trim();
        slug = slug.ToLowerInvariant();
        slug = slug.Replace(" ", "-");
        slug = slug.Replace("_", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9-]", string.Empty);
        slug = slug.Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "produto" : slug;
    }
}
