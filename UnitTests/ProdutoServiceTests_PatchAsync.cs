using Estoque.Application.DTOs.Produto;
using Estoque.Application.Services;
using Estoque.Domain.Entities;
using Estoque.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests
{
    public class ProdutoServiceTests_PatchAsync
    {
        private readonly Mock<IProdutoRepository> _repositoryMock;
        private readonly ProdutoService _service;

        public ProdutoServiceTests_PatchAsync()
        {
            // 1. Inicializa o mock da dependência externa
            _repositoryMock = new Mock<IProdutoRepository>();
            // 2. Injeta o mock no serviço em teste
            _service = new ProdutoService(_repositoryMock.Object);
        }

        [Fact]
        public async Task PatchAsync_AoMarcarItemComoIndisponivel_DevePreencherDataRegistroFaltaEAtualizar()
        {
            var dataRegistro = DateTime.UtcNow.AddDays(-5);
            // Arrange
            var idProduto = 1;
            var produtoExistente = new Produto
            {
                Id = idProduto,
                Nome = "Açúcar 1kg",
                Disponivel = true,
                DataRegistroFalta = dataRegistro
            };

            var patchDto = new ProdutoPatchDTO { Disponivel = false };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(idProduto))
                .ReturnsAsync(produtoExistente);

            _repositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Produto>()))
                .ReturnsAsync((Produto p) => p);

            // Act
            var resultado = await _service.PatchAsync(idProduto, patchDto);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Disponivel.Should().BeFalse();
            resultado.DataRegistroFalta.Should().NotBe(dataRegistro);

            _repositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Produto>(p =>
                p.Id == idProduto &&
                p.Disponivel == false &&
                p.DataRegistroFalta != null
            )), Times.Once);
        }
    }
}
