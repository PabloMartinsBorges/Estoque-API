using System.Net;
using System.Text;
using FluentAssertions;
using IntegrationTests.Fixtures;
using Xunit;

namespace IntegrationTests.Controllers;

public class TelegramControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TelegramControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Webhook_SemHeaderSecretToken_DeveRetornarForbidden403()
    {
        // Arrange: payload de atualização vazio
        var payload = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act: requisição sem o cabeçalho 'X-Telegram-Bot-Api-Secret-Token'
        var response = await _client.PostAsync("/api/telegram/webhook", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}