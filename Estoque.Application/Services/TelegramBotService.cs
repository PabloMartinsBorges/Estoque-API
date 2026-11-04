using Estoque.Application.DTOs.Produto;
using Estoque.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Estoque.Application.Services;

public class TelegramBotService : ITelegramBotService
{
    private readonly IProdutoService _produtoService;
    private readonly ITelegramBotClient _botClient;
    private readonly IMemoryCache _cache;

    // Prefixo para as chaves no cache
    // Isso ajuda a evitar colisões de chaves e facilita a identificação de dados relacionados ao estado do chat
    private const string EstadoCachePrefix = "estado_chat_";

    public TelegramBotService(IProdutoService produtoService, ITelegramBotClient botClient, IMemoryCache cache)
    {
        _produtoService = produtoService;
        _botClient = botClient;
        _cache = cache;
    }

    public async Task ProcessarMensagemAsync(long chatId, string mensagem)
    {
        var textoLimpo = mensagem.Trim();
        var cacheKey = $"{EstadoCachePrefix}{chatId}";

        // 1. Verifica se o usuário estava aguardando preencher alguma pergunta anterior
        if (_cache.TryGetValue(cacheKey, out string? estadoAtual))
        {
            if (estadoAtual == "AGUARDANDO_NOME_FALTA")
            {
                // Limpa o estado para não ficar preso no fluxo
                _cache.Remove(cacheKey);

                await TratarRegistroFaltaPorNomeAsync(chatId, textoLimpo);
                return;
            }
        }

        // 2. Comandos diretos de texto
        if (textoLimpo.Equals("/start", StringComparison.OrdinalIgnoreCase) ||
            textoLimpo.Equals("/menu", StringComparison.OrdinalIgnoreCase))
        {
            await EnviarMenuPrincipalAsync(chatId);
            return;
        }

        if (textoLimpo.Equals("/faltantes", StringComparison.OrdinalIgnoreCase))
        {
            await EnviarListaFaltantesAsync(chatId);
            return;
        }

        // Mensagem padrão caso não esteja em nenhum estado e não seja comando
        await _botClient.SendMessage(chatId,
            "Comando não reconhecido. Use o menu abaixo:",
            replyMarkup: ObterTecladoMenu());
    }

    public async Task ProcessarCallbackAsync(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message == null || string.IsNullOrEmpty(callbackQuery.Data))
            return;

        var chatId = callbackQuery.Message.Chat.Id;

        // Responde a requisição do Telegram para tirar o ícone de 'carregando' do botão
        await _botClient.AnswerCallbackQuery(callbackQuery.Id);

        switch (callbackQuery.Data)
        {
            case "menu_faltantes":
                await EnviarListaFaltantesAsync(chatId);
                break;

            case "menu_registrar_falta":
                // Define que a próxima mensagem desse chat será o nome do item (expira em 5 min)
                _cache.Set($"{EstadoCachePrefix}{chatId}", "AGUARDANDO_NOME_FALTA", TimeSpan.FromMinutes(5));

                await _botClient.SendMessage(chatId,
                    "✍️ Digite o *nome do produto* que está em falta:",
                    parseMode: ParseMode.Markdown);
                break;

            case "menu_ajuda":
                await _botClient.SendMessage(chatId,
                    "💡 *Dicas do Sistema:*\n• Use os botões para navegar.\n• Se preferir texto, digite `/faltantes` a qualquer momento.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: ObterTecladoMenu());
                break;
        }
    }

    private async Task EnviarMenuPrincipalAsync(long chatId)
    {
        await _botClient.SendMessage(
            chatId: chatId,
            text: "📦 *Painel de Controle de Estoque*\nEscolha uma das ações abaixo:",
            parseMode: ParseMode.Markdown,
            replyMarkup: ObterTecladoMenu());
    }

    private static InlineKeyboardMarkup ObterTecladoMenu()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📋 Ver Faltantes (48h)", "menu_faltantes"),
                InlineKeyboardButton.WithCallbackData("🔴 Registrar Falta", "menu_registrar_falta")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("ℹ️ Ajuda", "menu_ajuda")
            }
        });
    }

    private async Task EnviarListaFaltantesAsync(long chatId)
    {
        var faltantes = await _produtoService.GetProdutosFaltaAsync();

        if (faltantes == null || !faltantes.Any())
        {
            await _botClient.SendMessage(chatId,
                "✅ Nenhum item marcado como faltante nas últimas 48 horas.",
                replyMarkup: ObterTecladoMenu());
            return;
        }

        var resposta = "⚠️ *Itens Faltantes (Últimos 2 dias):*\n\n" +
                       string.Join("\n", faltantes.Select(p => $"• {p.Nome} (desde {p.DataRegistroFalta:dd/MM HH:mm})"));

        await _botClient.SendMessage(chatId, resposta, parseMode: ParseMode.Markdown, replyMarkup: ObterTecladoMenu());
    }

    private async Task TratarRegistroFaltaPorNomeAsync(long chatId, string nomeItem)
    {
        var produto = await _produtoService.FindByNameAsync(nomeItem);

        if (produto == null)
        {
            await _botClient.SendMessage(chatId,
                $"❌ Não foi possível encontrar o produto *{nomeItem}* no catálogo.",
                parseMode: ParseMode.Markdown,
                replyMarkup: ObterTecladoMenu());
            return;
        }

        var patchDto = new ProdutoPatchDTO { Disponivel = false };
        var atualizado = await _produtoService.PatchAsync(produto.Id, patchDto);

        if (atualizado != null)
        {
            await _botClient.SendMessage(chatId,
                $"🔴 O item *{produto.Nome}* foi marcado como faltante no estoque!",
                parseMode: ParseMode.Markdown,
                replyMarkup: ObterTecladoMenu());
        }
        else
        {
            await _botClient.SendMessage(chatId,
                $"⚠️ Ocorreu um erro ao atualizar o item *{produto.Nome}*.",
                replyMarkup: ObterTecladoMenu());
        }
    }
}