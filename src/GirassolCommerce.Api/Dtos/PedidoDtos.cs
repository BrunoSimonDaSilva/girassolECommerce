namespace GirassolCommerce.Api.Dtos;

public record PedidoItemRequest(
    string ProdutoId,
    int Quantidade);

public record CreatePedidoRequest(
    string NomeCliente,
    string EmailCliente,
    string EnderecoEntrega,
    List<PedidoItemRequest> Itens);

public record UpdatePedidoRequest(
    string Status);
