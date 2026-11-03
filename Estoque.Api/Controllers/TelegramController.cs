using Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

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

        if (update.Message is { Text: { } messageText } message)
        {
            await _telegramBotService.ProcessarMensagemAsync(message.Chat.Id, messageText);
        }

        return Ok();
    }
}