using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBots
{
    class Program
    {
        private static TelegramBotClient _bot;

        static void Main(string[] args)
        {
            StartBot().GetAwaiter().GetResult();
            Console.ReadLine();
        }

        private static async Task StartBot()
        {
            IConfigurationRoot _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

            var token = _configuration.GetValue("telegram:token", string.Empty);

            _bot = new TelegramBotClient(token);

            var me = await _bot.GetMeAsync();
            Console.Title = me.Username;

            var cts = new CancellationTokenSource();
            _bot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, cancellationToken);
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => "",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
        }


        private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            throw new NotImplementedException();
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return;

            var answer = Reverse(message.Text);

            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: answer
                );
        }

        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
