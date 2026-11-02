using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;

    public TelegramController(ITelegramBotClient botClient, IConfiguration configuration)
    {
        _botClient = botClient;
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Post(
        [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? secretToken,
        [FromBody] Update update)
    {
        // 1. Validação de segurança opcional, mas recomendada pelo Telegram
        var expectedToken = _configuration["Telegram:SecretToken"];
        if (!string.IsNullOrEmpty(expectedToken) && secretToken != expectedToken)
        {
            return Forbid();
        }

        // 2. Garante que é uma mensagem de texto simples
        if (update.Message is not { Text: { } messageText } message)
        {
            return Ok(); // Responda 200 para eventos não tratados (ex: entrou no grupo)
        }

        var chatId = message.Chat.Id;

        // 3. Aqui você chamará o seu Use Case / Service de Estoque
        if (messageText.StartsWith("/start"))
        {
            await _botClient.SendMessage(chatId, "Olá! Envie o nome de um produto faltante ou digite /faltantes para ver a lista.");
            return Ok();
        }

        // Exemplo provisório de eco / confirmação
        await _botClient.SendMessage(chatId, $"Recebido: \"{messageText}\". Processando no estoque...");

        return Ok();
    }
}