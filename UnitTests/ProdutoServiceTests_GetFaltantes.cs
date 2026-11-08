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
    public class ProdutoServiceTests_GetFaltantes
    {
        private readonly Mock<IProdutoRepository> _repositoryMock;
        private readonly ProdutoService _service;

        public ProdutoServiceTests_GetFaltantes()
        {
            // 1. Inicializa o mock da dependência externa
            _repositoryMock = new Mock<IProdutoRepository>();
            // 2. Injeta o mock no serviço em teste
            _service = new ProdutoService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetProdutosFaltaAsync_QuandoExistemItensFaltantesDentroDe48Horas_DeveRetornarApenasItensValidos()
        {
            // Arrange
            var agora = DateTime.UtcNow;
            var listaMista = new List<Produto>
        {
            // 1. Deve vir: indisponível e falta registrada há 1 dia (dentro da janela de 48h)
            new Produto
            {
                Id = 1,
                Nome = "Óleo de Soja 900ml",
                Disponivel = false,
                DataRegistro = agora.AddDays(-10),
                DataRegistroFalta = agora.AddDays(-1)
            },
            // 2. Não deve vir: indisponível, mas a falta foi há 4 dias (fora da janela)
            new Produto
            {
                Id = 2,
                Nome = "Açúcar Cristal 1kg",
                Disponivel = false,
                DataRegistro = agora.AddDays(-10),
                DataRegistroFalta = agora.AddDays(-4)
            },
            // 3. Não deve vir: está disponível (em estoque)
            new Produto
            {
                Id = 3,
                Nome = "Sal Refinado 1kg",
                Disponivel = true,
                DataRegistro = agora.AddDays(-5),
                DataRegistroFalta = agora.AddDays(-1)
            }
        };

            _repositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(listaMista);

            // Act
            var resultado = await _service.GetProdutosFaltaAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);

            var itemFaltante = resultado!.First();
            itemFaltante.Id.Should().Be(1);
            itemFaltante.Nome.Should().Be("Óleo de Soja 900ml");
            itemFaltante.Disponivel.Should().BeFalse();

            _repositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProdutosFaltaAsync_QuandoNenhumProdutoEstiverFaltando_DeveRetornarNull()
        {
            // Arrange: repositório contendo apenas itens disponíveis em estoque
            var produtosDisponiveis = new List<Produto>
        {
            new Produto { Id = 1, Nome = "Farinha de Trigo", Disponivel = true },
            new Produto { Id = 2, Nome = "Manteiga", Disponivel = true }
        };

            _repositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(produtosDisponiveis);

            // Act
            var resultado = await _service.GetProdutosFaltaAsync();

            // Assert
            resultado.Should().BeNull();
            _repositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProdutosFaltaAsync_QuandoBaseEstiverVazia_DeveRetornarNull()
        {
            // Arrange: repositório retorna uma lista sem nenhum registro
            _repositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Produto>());

            // Act
            var resultado = await _service.GetProdutosFaltaAsync();

            // Assert
            resultado.Should().BeNull();
            _repositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Theory]
        [InlineData(-1, true)]   // Faltou há 1 dia (dentro da janela de 48h) -> deve ser incluído
        [InlineData(-3, false)]  // Faltou há 3 dias (fora da janela de 48h) -> deve ser descartado
        public async Task GetProdutosFaltaAsync_ParametrizadoPorDiasDeFalta_DeveFiltrarJanelaCorretamente(int diasRelativos, bool deveRetornar)
        {
            // Arrange
            var produtoTeste = new List<Produto>
        {
            new Produto
            {
                Id = 10,
                Nome = "Item Teste Temporal",
                Disponivel = false,
                DataRegistroFalta = DateTime.UtcNow.AddDays(diasRelativos)
            }
        };

            _repositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(produtoTeste);

            // Act
            var resultado = await _service.GetProdutosFaltaAsync();

            // Assert
            if (deveRetornar)
            {
                resultado.Should().NotBeNull();
                resultado.Should().ContainSingle(p => p.Id == 10);
            }
            else
            {
                resultado.Should().BeNull();
            }
        }
    }
}
