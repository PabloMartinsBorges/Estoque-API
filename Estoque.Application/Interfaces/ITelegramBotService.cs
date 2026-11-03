using System;
using System.Collections.Generic;
using System.Text;

namespace Estoque.Application.Interfaces
{
    public interface ITelegramBotService
    {
        Task ProcessarMensagemAsync(long chatId,string mensagem);
    }
}
