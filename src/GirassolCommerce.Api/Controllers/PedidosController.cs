using System.Security.Claims;
using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;
using GirassolCommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GirassolCommerce.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;
    private readonly PedidoBusinessService _pedidoBusinessService;

    public PedidosController(IPedidoService pedidoService, PedidoBusinessService pedidoBusinessService)
    {
        _pedidoService = pedidoService;
        _pedidoBusinessService = pedidoBusinessService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetAll()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isAdmin = User.IsInRole("Admin");
        var pedidos = await _pedidoService.ListarAsync(usuarioId, isAdmin);
        return Ok(pedidos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetById(string id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isAdmin = User.IsInRole("Admin");
        var pedido = await _pedidoService.BuscarPorIdAsync(id, usuarioId, isAdmin);

        if (pedido is null)
        {
            return NotFound(new { message = "Pedido não encontrado." });
        }

        return Ok(pedido);
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Create([FromBody] CreatePedidoRequest request)
    {
        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Unauthorized(new { message = "É necessário autenticar para criar um pedido." });
            }

            var pedido = await _pedidoService.CriarAsync(usuarioId, request, _pedidoBusinessService);
            return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Pedido>> UpdateStatus(string id, [FromBody] UpdatePedidoRequest request)
    {
        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var pedidoAtualizado = await _pedidoService.AtualizarAsync(id, request.Status, usuarioId, true);
            return Ok(pedidoAtualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _pedidoService.DeletarAsync(id, usuarioId, true);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
