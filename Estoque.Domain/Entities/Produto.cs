using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Domain.Entities
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public bool Disponivel { get; set; }
        public DateTime DataRegistro { get; set; }

        public DateTime DataRegistroFalta { get; set; } // New property to track the date when the product went out of stock}

    }
}
