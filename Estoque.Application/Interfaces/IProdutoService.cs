using Estoque.Application.DTOs.Produto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Application.Interfaces
{
    public interface IProdutoService
    {
        public Task<ProdutoGetDTO> AddAsync(ProdutoPostDTO produtoPostDTO);
        public Task<ProdutoGetDTO> UpdateAsync(int id,ProdutoPutDTO produtoPutDTO);
        public Task<List<ProdutoGetDTO>> GetAllAsync();

        public Task<ProdutoGetDTO> GetByIdAsync(int id);

        public Task<ProdutoGetDTO> PatchAsync(int id, ProdutoPatchDTO produtoPatchDTO);

        public Task<List<ProdutoGetDTO>> GetProdutosFaltaAsync();

    }
}
