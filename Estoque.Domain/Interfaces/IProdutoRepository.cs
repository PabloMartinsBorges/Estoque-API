using Estoque.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Domain.Interfaces
{
    public interface IProdutoRepository
    {
        Task<Produto> AddAsync(Produto produto);

        Task<List<Produto>> GetAllAsync();

        Task<Produto> GetByIdAsync(int id);

        Task<Produto> UpdateAsync(Produto produto);

       
    }
}
