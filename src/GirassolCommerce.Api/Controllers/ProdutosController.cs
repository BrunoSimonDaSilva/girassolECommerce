using GirassolCommerce.Api.Dtos;
using GirassolCommerce.Api.Models;
using GirassolCommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GirassolCommerce.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
    {
        var produtos = await _produtoService.ListarAsync();
        return Ok(produtos);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Produto>> GetById(string id)
    {
        var produto = await _produtoService.BuscarPorIdAsync(id);
        if (produto is null)
        {
            return NotFound(new { message = "Produto não encontrado." });
        }

        return Ok(produto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Produto>> Create([FromBody] CreateProdutoRequest request)
    {
        try
        {
            var produto = await _produtoService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Produto>> Update(string id, [FromBody] UpdateProdutoRequest request)
    {
        try
        {
            var produto = await _produtoService.AtualizarAsync(id, request);
            return Ok(produto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
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
            await _produtoService.DeletarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
