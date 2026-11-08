using Estoque.Api.Erros;
using Estoque.Application.DTOs.Produto;
using Estoque.Domain.Entities;
using Estoque.Infra.Data.Context;
using FluentAssertions;
using IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace IntegrationTests.Controllers;

public class ProdutosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProdutosControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_ComPayloadValido_DeveRetornarOkEMensagemDeSucesso()
    {
        // Arrange
        var novoProduto = new ProdutoPostDTO { Nome = "Queijo Mussarela 1kg" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Produto", novoProduto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var corpo = await response.Content.ReadFromJsonAsync<RespostaPadraoDTO>();
        corpo.Should().NotBeNull();
        corpo!.Message.Should().Be("Produto criado com sucesso.");
    }

    public record RespostaPadraoDTO(string Message);

    [Fact]
    public async Task GetFaltantes_QuandoExistemItensFaltantes_DeveRetornarStatus200Ok()
    {
        // 1. Arrange: Insere diretamente no banco em memória um item com falta dentro de 48h
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Produtos.Add(new Produto
            {
                Nome = "Café Torrado 500g",
                Disponivel = false,
                DataRegistro = DateTime.UtcNow,
                DataRegistroFalta = DateTime.UtcNow // Dentro da janela de 48h
            });

            await dbContext.SaveChangesAsync();
        }

        // 2. Act: Consulta os faltantes
        var response = await _client.GetAsync("/api/Produto/listar-falta");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var itens = await response.Content.ReadFromJsonAsync<List<ProdutoGetDTO>>();
        itens.Should().NotBeNull();
        itens.Should().NotBeEmpty();
        itens!.Should().Contain(p => p.Nome == "Café Torrado 500g");
    }

    [Fact]
    public async Task GetById_QuandoProdutoNaoExiste_DeveRetornar404FormatadoPeloMiddleware()
    {
        // Arrange
        var idInexistente = 999999;

        // Act
        var response = await _client.GetAsync($"/api/Produto/{idInexistente}");

        // Assert - Valida o status code interceptado pelo ExceptionMiddleware
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        // Valida se o formato do JSON obedece à estrutura ApiException (RFC 7807)
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var erro = await response.Content.ReadFromJsonAsync<ApiException>(options);

        erro.Should().NotBeNull();
        erro!.StatusCode.Should().Be("404");
        erro.Message.Should().Contain("Produto não encontrado");
    }
}