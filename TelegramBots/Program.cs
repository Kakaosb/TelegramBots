using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
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
            //StartBot().GetAwaiter().GetResult();
            some().GetAwaiter().GetResult();
            Console.ReadLine();
        }

        private async static Task some() 
        {
            try
            {
                using var client = new WTelegram.Client();
                var myself = await client.LoginUserIfNeeded();
                Console.WriteLine($"We are logged-in as {myself} (id {myself.id})");


                //var chats = await client.Messages_GetAllChats();

                //Console.WriteLine("This user has joined the following:");
                //foreach (var (id, chat) in chats.chats)
                //    if (chat.IsActive)
                //        Console.WriteLine($"{id,10}: {chat}");
                //Console.Write("Type a chat ID to send a message: ");
                //long chatId = long.Parse(Console.ReadLine());
                //var target = chats.chats[chatId];
                //Console.WriteLine($"Sending a message in chat {chatId}: {target.Title}");
                //await client.SendMessageAsync(target, "Hello, World");

                var dialogs = await client.Messages_GetAllDialogs();
            
                var chats = dialogs.users;
                chats.ToList().ForEach(el=> {
                   // Console.WriteLine($"{el.Key} {el.Value} ");
                
                });
                Console.Write("Type a chat ID to send a message: ");
                long chatId = long.Parse(Console.ReadLine());
                var target = dialogs.users[chatId];
                Console.WriteLine($"Sending a message in chat {chatId}: {target.username}");
                await client.SendMessageAsync(target, "здарова, уёбки");

            }
            catch (Exception ex) {
                var a = ex.Message;
            }
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
