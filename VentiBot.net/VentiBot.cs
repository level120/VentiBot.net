using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace VentiBot.net
{
    public sealed partial class VentiBot
    {
        private const string DebugTokenPath = "./debug.token";

        private static TelegramBotClient _client;
        public static TelegramBotClient Client => _client ??= new TelegramBotClient(GetToken());

        private static string GetToken()
        {
            if (Debugger.IsAttached && File.Exists(DebugTokenPath))
            {
                return File.ReadAllText(DebugTokenPath)
                    .Replace(Environment.NewLine, string.Empty);
            }

            // todo: token 받을 수 있도록 수정필요
            return null;
        }

        public static async Task RunAsync(CancellationTokenSource cts)
        {
            var me = await Client.GetMeAsync(cts.Token);
            Console.Title = me.Username;

            Client.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
        }
    }

    public partial class VentiBot
    {
        private static async Task HandleUpdateAsync(
                ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            if (message?.Type != MessageType.Text)
                return;

            var action = message.Text.Split(' ').First() switch
            {
                "/list" => SendListMsgAsync(message),
                _ => SendUsageMsgAsync(message)
            };

            await action;
        }

        private static async Task SendListMsgAsync(Message message)
        {
            // todo: api 작업 후 list msg 확인
            const string itemMsg = "Empty";

            await Client.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: itemMsg,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private static async Task SendUsageMsgAsync(Message message)
        {
            const string usage = "Usage:\n" +
                                 "/list   - ??? 목록 확인";

            await Client.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
        }

        private static async Task HandleErrorAsync(
            ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errMsg = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",

                _ => exception.ToString()
            };

            Console.Error.WriteLineAsync(errMsg);
        }
    }
}
