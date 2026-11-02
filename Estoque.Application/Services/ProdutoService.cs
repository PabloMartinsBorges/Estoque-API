using Estoque.Application.DTOs.Produto;
using Estoque.Application.Interfaces;
using Estoque.Domain.Entities;
using Estoque.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        public readonly IProdutoRepository produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            this.produtoRepository = produtoRepository;
        }

        public async Task<ProdutoGetDTO> AddAsync(ProdutoPostDTO produtoPostDTO)
        {
            var produto = new Produto
            {
                Nome = produtoPostDTO.Nome,
                DataRegistro = DateTime.UtcNow,
                Disponivel = false,
                DataRegistroFalta = DateTime.UtcNow // Initialize the date when the product goes out of stock  
            };

            var createdProduto = await produtoRepository.AddAsync(produto);
            return new ProdutoGetDTO
            {
                Id = createdProduto.Id,
                Nome = createdProduto.Nome
            };
        }

        public async Task<List<ProdutoGetDTO>> GetAllAsync()
        {
            var produtos = await produtoRepository.GetAllAsync();
            return produtos.Select(p => new ProdutoGetDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                Disponivel = p.Disponivel,
                DataRegistro = p.DataRegistro,
                DataRegistroFalta = p.DataRegistroFalta // Include the date when the product goes out of stock
            }).ToList();
        }

        public async Task<ProdutoGetDTO> GetByIdAsync(int id)
        {
            var produto = await produtoRepository.GetByIdAsync(id);
            return new ProdutoGetDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Disponivel = produto.Disponivel,
                DataRegistro = produto.DataRegistro,
                DataRegistroFalta = produto.DataRegistroFalta // Include the date when the product goes out of stock
            };
        }

        public async Task<ProdutoGetDTO> UpdateAsync(int id, ProdutoPutDTO produtoPutDTO)
        {
            var produto = await produtoRepository.GetByIdAsync(id);
            produto.Nome = produtoPutDTO.Nome;
            var updatedProduto = await produtoRepository.UpdateAsync(produto);
            return new ProdutoGetDTO
            {
                Id = updatedProduto.Id,
                Nome = updatedProduto.Nome,
                Disponivel = updatedProduto.Disponivel,
            
            };
        }

        public async Task<ProdutoGetDTO> PatchAsync(int id, ProdutoPatchDTO produtoPatchDTO)
        { 
            var produto = await produtoRepository.GetByIdAsync(id);
            produto.Disponivel = produtoPatchDTO.Disponivel;
            if(!produto.Disponivel)
            {
                produto.DataRegistroFalta = DateTime.UtcNow;
            }
            var updatedProduto = await produtoRepository.UpdateAsync(produto);
            return new ProdutoGetDTO
            {
                Id = updatedProduto.Id,
                Nome = updatedProduto.Nome,
                Disponivel = updatedProduto.Disponivel,
                DataRegistro = updatedProduto.DataRegistro,
                DataRegistroFalta = updatedProduto.DataRegistroFalta
            };
        }

        public async Task<List<ProdutoGetDTO>> GetProdutosFaltaAsync()
        {
            var produtos = await produtoRepository.GetAllAsync();

            var periodoFalta = DateTime.UtcNow.AddDays(-2); // Define o período de falta como os últimos 2 dias

            var produtosFalta = produtos.Where(p => !p.Disponivel && p.DataRegistroFalta >= periodoFalta).ToList();

            if(produtosFalta == null || produtosFalta.Count == 0)
            {
                return null;
            }

            return produtosFalta.Select(p => new ProdutoGetDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                Disponivel = p.Disponivel,
                DataRegistro = p.DataRegistro,
                DataRegistroFalta = p.DataRegistroFalta // Include the date when the product goes out of stock
            }).ToList();
        }
    }
}
