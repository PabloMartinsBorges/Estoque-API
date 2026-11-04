using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace Estoque.Application.Interfaces
{
    public interface ITelegramBotService
    {
        Task ProcessarMensagemAsync(long chatId,string mensagem);
        Task ProcessarCallbackAsync(CallbackQuery callbackQuery);
    }
}
