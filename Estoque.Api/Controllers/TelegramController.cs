using Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramBotService _telegramBotService;
    private readonly IConfiguration _configuration;

    public TelegramController(ITelegramBotService telegramBotService, IConfiguration configuration)
    {
        _telegramBotService = telegramBotService;
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Post(
    [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? secretToken,
    [FromBody] Update update)
    {
        var expectedToken = _configuration["Telegram:SecretToken"];
        if (!string.IsNullOrEmpty(expectedToken) && secretToken != expectedToken)
        {
            return Forbid();
        }

        // 1. Trata clique nos botões (Inline Keyboards)
        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await _telegramBotService.ProcessarCallbackAsync(update.CallbackQuery);
            return Ok();
        }

        // 2. Trata mensagens de texto enviadas pelo usuário
        if (update.Message is { Text: { } messageText } message)
        {
            await _telegramBotService.ProcessarMensagemAsync(message.Chat.Id, messageText);
            return Ok();
        }

        return Ok();
    }
}