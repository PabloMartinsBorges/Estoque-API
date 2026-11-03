using Estoque.Domain.Entities;
using Estoque.Domain.Interfaces;
using Estoque.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Infra.Data.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly ApplicationDbContext _context;
        
        public ProdutoRepository(ApplicationDbContext context)
        {
            _context = context;
        }  

        public async Task<Produto> AddAsync(Produto produto)
        {
           _context.Produtos.Add(produto);
           await _context.SaveChangesAsync();
           return produto;
        }

        public Task<Produto> FindByNameAsync(string nome)
        {
            return _context.Produtos.FirstOrDefaultAsync(p => p.Nome == nome);
        }

        public async Task<List<Produto>> GetAllAsync()
        {
            return await _context.Produtos.ToListAsync();
        }

        public async Task<Produto> GetByIdAsync(int id)
        {
            return await _context.Produtos.FindAsync(id);
        }

        public async Task<Produto> UpdateAsync(Produto produto)
        {
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();

            return produto;
        }
    }
}
