using Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Estoque.Application.DTOs.Produto;

namespace Estoque.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : Controller
    {
        private readonly IProdutoService _produtoService;
        public ProdutoController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpPost]

        public async Task<ActionResult> CreateProduto(ProdutoPostDTO produtoPostDTO)
        {
            var createdProduto = await _produtoService.AddAsync(produtoPostDTO);
            if (createdProduto == null)
            {
                return BadRequest("Erro ao criar o produto.");
            }
            return Ok(new { message = "Produto criado com sucesso." });
        }

        [HttpGet]
        public async Task<ActionResult> GetAllProdutos()
        {
            var produtos = await _produtoService.GetAllAsync();

            if (produtos == null) return BadRequest("Erro ao buscar produtos.");
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProdutoById(int id)
        {
            var produto = await _produtoService.GetByIdAsync(id);
            if (produto == null) return NotFound("Produto não encontrado.");
            return Ok(produto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduto(int id, ProdutoPutDTO produtoPutDTO)
        {
            var updatedProduto = await _produtoService.UpdateAsync(id, produtoPutDTO);
            if (updatedProduto == null)
            {
                return BadRequest("Erro ao atualizar o produto.");
            }
            return Ok(new { message = "Produto atualizado com sucesso." });
        }

        [HttpPatch("{id}/disponivel")]
        public async Task<ActionResult> UpdateProdutoDisponibilidade(int id, ProdutoPatchDTO produtoPatchDTO)
        {

            var updatedProduto = await _produtoService.PatchAsync(id, produtoPatchDTO);
            if(updatedProduto == null) return BadRequest("Erro ao atualizar a disponibilidade do produto.");

            return Ok(new { message = "Disponibilidade do produto atualizada com sucesso." });
        }

        [HttpGet("/listar-falta")]
        public async Task<ActionResult> GetProdutosFalta()
        {
            var produtos = await _produtoService.GetProdutosFaltaAsync();

            return Ok(produtos);
        }       
    }
}
