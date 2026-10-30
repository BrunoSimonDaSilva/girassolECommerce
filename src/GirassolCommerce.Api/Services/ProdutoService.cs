using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;
using MongoDB.Driver;

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
    private readonly MongoDbContext _context;

    public ProdutoService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Produto>> ListarAsync()
    {
        return await _context.Produtos.Find(_ => true)
            .SortBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Produto?> BuscarPorIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return await _context.Produtos.Find(p => p.Id == id).FirstOrDefaultAsync();
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
        await _context.Produtos.InsertOneAsync(produto);
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
        await _context.Produtos.ReplaceOneAsync(p => p.Id == id, produto);

        return produto;
    }

    public async Task DeletarAsync(string id)
    {
        var result = await _context.Produtos.DeleteOneAsync(p => p.Id == id);
        if (result.DeletedCount == 0)
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
        await _context.Produtos.ReplaceOneAsync(p => p.Id == produtoId, produto);
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
