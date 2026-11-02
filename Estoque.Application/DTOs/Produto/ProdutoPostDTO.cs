using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Estoque.Application.DTOs.Produto
{
    public class ProdutoPostDTO
    {
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O nome do produto não pode exceder 50 caracteres.")]
        public string Nome { get; set; }
    }
}
