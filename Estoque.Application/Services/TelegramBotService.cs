using Estoque.Application.DTOs.Produto;
using Estoque.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;

namespace Estoque.Application.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly IProdutoService _produtoService;
        private readonly ITelegramBotClient _botClient;

        public TelegramBotService(IProdutoService produtoService, ITelegramBotClient botClient)
        {
            _produtoService = produtoService;
            _botClient = botClient;
        }

        public async Task ProcessarMensagemAsync(long chatId, string mensagem)
        {
            var textoLimpo = mensagem.Trim();

            // 1. Comando: Boas-vindas
            if (textoLimpo.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                await _botClient.SendMessage(chatId,
                    "📦 *Sistema de Estoque*\n\n" +
                    "Comandos disponíveis:\n" +
                    "• `/faltantes` - Lista os itens que faltaram nos últimos 2 dias\n" +
                    "• `/falta [nome do item]` - Registra um produto como indisponível",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            // 2. Comando: Listar Faltantes dos últimos 2 dias
            if (textoLimpo.Equals("/faltantes", StringComparison.OrdinalIgnoreCase))
            {
                var faltantes = await _produtoService.GetProdutosFaltaAsync();

                if (faltantes == null)
                {
                    await _botClient.SendMessage(chatId, "✅ Nenhum item marcado como faltante nas últimas 48 horas.");
                    return;
                }

                var resposta = "⚠️ *Itens Faltantes (Últimos 2 dias):*\n\n" +
                               string.Join("\n", faltantes.Select(p => $"• {p.Nome} (desde {p.DataRegistroFalta:dd/MM HH:mm})"));

                await _botClient.SendMessage(chatId, resposta, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            // 3. Comando: Marcar falta de um item (Ex: /falta Café)
            if (textoLimpo.StartsWith("/falta ", StringComparison.OrdinalIgnoreCase))
            {
                var nomeItem = textoLimpo.Substring(7).Trim().ToLower();

                var produto = await _produtoService.FindByNameAsync(nomeItem);

                if(produto == null)
                {
                    await _botClient.SendMessage(chatId, $"❌ Não foi possível encontrar o produto *{nomeItem}* no catálogo.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    return;
                }

                var sucesso = await _produtoService.PatchAsync(produto.Id, new ProdutoPatchDTO { Disponivel = false });

                if (sucesso != null)
                {
                    await _botClient.SendMessage(chatId, $"🔴 O item *{nomeItem}* foi marcado como faltante no estoque.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }
                else
                {
                    await _botClient.SendMessage(chatId, $"❌ Não foi possível encontrar o produto *{nomeItem}* no catálogo.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }
                return;
            }


            if(textoLimpo.Equals("/listar", StringComparison.OrdinalIgnoreCase))
            {
                var produtos = await _produtoService.GetAllAsync();
                if(produtos == null || !produtos.Any())
                {
                    await _botClient.SendMessage(chatId, "📦 Nenhum produto cadastrado no estoque.");
                    return;
                }

                var resposta = "📦 *Produtos no Estoque:*\n\n" +
                               string.Join("\n", produtos.Select(p => $"• {p.Nome} - {(p.Disponivel ? "Disponível" : "Indisponível")}"));
                await _botClient.SendMessage(chatId, resposta, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }

            if(textoLimpo.StartsWith("/adicionar ", StringComparison.OrdinalIgnoreCase))
            {
                var nomeItem = textoLimpo.Substring(11).Trim().ToLower();
                var produtoPostDTO = new ProdutoPostDTO { Nome = nomeItem };
                var createdProduto = await _produtoService.AddAsync(produtoPostDTO);
                if (createdProduto != null)
                {
                    await _botClient.SendMessage(chatId, $"✅ O item *{nomeItem}* foi adicionado ao estoque.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }
                else
                {
                    await _botClient.SendMessage(chatId, $"❌ Não foi possível adicionar o produto *{nomeItem}* ao estoque.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                }
            }
           


            // 4. Mensagem não reconhecida
            await _botClient.SendMessage(chatId, "Não entendi o comando. Digite `/start` para ver as opções.");
        }
    }
}
