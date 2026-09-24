namespace GirassolCommerce.Api.Dtos;

public record CreateProdutoRequest(
    string Nome,
    string Descricao,
    string Categoria,
    decimal Preco,
    int Estoque,
    string? ImagemUrl = null,
    string[]? Tags = null,
    bool Ativo = true);

public record UpdateProdutoRequest(
    string? Nome,
    string? Descricao,
    string? Categoria,
    decimal? Preco,
    int? Estoque,
    string? ImagemUrl,
    string[]? Tags,
    bool? Ativo);
