using Estoque.Application.DTOs.Produto;
using Estoque.Application.Exceptions;
using Estoque.Application.Services;
using Estoque.Domain.Entities;
using Estoque.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Timers;
using Xunit;

namespace UnitTests;

public class ProdutoServiceTests_AddAsync
{
    private readonly Mock<IProdutoRepository> _repositoryMock;
    private readonly ProdutoService _service;

    public ProdutoServiceTests_AddAsync()
    {
        // 1. Inicializa o mock da dependência externa
        _repositoryMock = new Mock<IProdutoRepository>();

        // 2. Injeta o mock no serviço em teste
        _service = new ProdutoService(_repositoryMock.Object);
    }

    // Teste de unidade para o método AddAsync do ProdutoService
    [Fact]
    public async Task AddAsync_ComDadosValidos_DeveSalvarERetornarProdutoGetDTO()
    {
        // Arrange (Preparação)
        var postDto = new ProdutoPostDTO { Nome = "Farinha de Trigo Especial" };
        var produtoPersistido = new Produto
        {
            Id = 1,
            Nome = postDto.Nome,
            Disponivel = false,
            DataRegistro = DateTime.UtcNow,
            DataRegistroFalta = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Produto>()))
            .ReturnsAsync(produtoPersistido);

        // Act (Execução)
        var resultado = await _service.AddAsync(postDto);

        // Assert (Verificação com FluentAssertions)
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(1);
        resultado.Nome.Should().Be(postDto.Nome);

        // Garante que o repositório foi invocado com os valores default de negócio
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Produto>(p =>
            p.Nome == postDto.Nome &&
            p.Disponivel == false &&
            p.DataRegistro != default &&
            p.DataRegistroFalta != default
        )), Times.Once);
    }

    [Fact]
    public async Task AddAsync_QuandoRepositorioRetornaNull_DeveLancarBadRequestException()
    {
        // Arrange
        var postDto = new ProdutoPostDTO { Nome = "Item Falha" };

        // Simula falha na camada de persistência retornando nulo
        _repositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Produto>()))
            .ReturnsAsync((Produto?)null);

        // Act
        Func<Task> acao = async () => await _service.AddAsync(postDto);

        // Assert
        await acao.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Erro ao criar o produto.");

        // Confirma que o método chegou a tentar persistir antes de falhar
        _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Produto>()), Times.Once);
    }

    [Theory]
    [InlineData("Café Tradicional 500g")]
    [InlineData("Óleo de Soja 900ml")]
    [InlineData("Fermento Biológico 50g")]
    public async Task AddAsync_ComDiferentesNomesValidos_DeveMapearNomeCorretamente(string nomeEsperado)
    {
        // Arrange
        var postDto = new ProdutoPostDTO { Nome = nomeEsperado };
        var produtoMock = new Produto
        {
            Id = 42,
            Nome = nomeEsperado,
            Disponivel = false
        };

        _repositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Produto>()))
            .ReturnsAsync(produtoMock);

        // Act
        var resultado = await _service.AddAsync(postDto);

        // Assert
        resultado.Nome.Should().Be(nomeEsperado);
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Produto>(p => p.Nome == nomeEsperado)), Times.Once);
    }




}