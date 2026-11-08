using Estoque.Application.Exceptions;
using Estoque.Application.Services;
using Estoque.Domain.Entities;
using Estoque.Domain.Interfaces;
using FluentAssertions;
using FluentAssertions.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests
{
    public class ProdutoServiceTests_FindByNameAsync
    {
        private readonly Mock<IProdutoRepository> _repositoryMock;
        private readonly ProdutoService _service;

        public ProdutoServiceTests_FindByNameAsync()
        {
            // 1. Inicializa o mock da dependência externa
            _repositoryMock = new Mock<IProdutoRepository>();
            // 2. Injeta o mock no serviço em teste
            _service = new ProdutoService(_repositoryMock.Object);
        }



        [Fact]
        public async Task FindByNameAsync_QuandoProdutoExiste_DeveRetornarProdutoGetDTO()
        {
            // Arrange
            var nomePesquisa = "Óleo de Soja 900ml";
            var dataRegistro = DateTime.UtcNow.AddDays(-5);
            var produtoMock = new Produto
            {
                Id = 1,
                Nome = nomePesquisa,
                Disponivel = true,
                DataRegistro = dataRegistro,
                DataRegistroFalta = dataRegistro
            };

            _repositoryMock
                .Setup(repo => repo.FindByNameAsync(nomePesquisa))
                .ReturnsAsync(produtoMock);

            // Act
            var resultado = await _service.FindByNameAsync(nomePesquisa);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(1);
            resultado.Nome.Should().Be(nomePesquisa);
            resultado.Disponivel.Should().BeTrue();
            resultado.DataRegistro.Should().Be(dataRegistro);
            resultado.DataRegistroFalta.Should().Be(dataRegistro);

            _repositoryMock.Verify(repo => repo.FindByNameAsync(nomePesquisa), Times.Once);
        }

        [Fact]
        public async Task FindByNameAsync_QuandoProdutoNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            var nomeInexistente = "Item Inexistente";

            _repositoryMock
                .Setup(repo => repo.FindByNameAsync(nomeInexistente))
                .ReturnsAsync((Produto?)null);

            // Act
            Func<Task> acao = async () => await _service.FindByNameAsync(nomeInexistente);

            // Assert
            await acao.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage($"Produto com nome '{nomeInexistente}' não encontrado.");

            _repositoryMock.Verify(repo => repo.FindByNameAsync(nomeInexistente), Times.Once);
        }

        [Theory]
        [InlineData("Café em Pó 500g")]
        [InlineData("Farinha de Trigo Especial 1kg")]
        public async Task FindByNameAsync_ComNomesDiferentes_DeveConsultarRepositorioComParametroExato(string nomeTeste)
        {
            // Arrange
            var produtoMock = new Produto
            {
                Id = 10,
                Nome = nomeTeste,
                Disponivel = true
            };

            _repositoryMock
                .Setup(repo => repo.FindByNameAsync(nomeTeste))
                .ReturnsAsync(produtoMock);

            // Act
            var resultado = await _service.FindByNameAsync(nomeTeste);

            // Assert
            resultado.Nome.Should().Be(nomeTeste);
            _repositoryMock.Verify(repo => repo.FindByNameAsync(nomeTeste), Times.Once);
        }
    }
}
