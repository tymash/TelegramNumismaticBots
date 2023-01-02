using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot;

static class Program
{
    static async Task Main(string[] args)
    {
        var depositBot = new TelegramBotClient("5920158597:AAF7QXDlXSUfKhH9F-_GymGKOttbjo6CgAA");
        var creditBot = new TelegramBotClient("5817236422:AAH1TPuHvHwFLt8W6WYM-LCQ3-EmVYQjMz4");

        using CancellationTokenSource cts = new();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        depositBot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
        
        creditBot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var depositBotUser = await depositBot.GetMeAsync();
        var creditBotUser = await creditBot.GetMeAsync();

        Console.WriteLine($"Start listening for @{depositBotUser.Username}");
        Console.WriteLine($"Start listening for @{creditBotUser.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;
            if (messageText == "/start")
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            await botClient.ForwardMessageAsync(
                chatId: 573625848,
                fromChatId: chatId,
                messageId: message.MessageId,
                cancellationToken: cancellationToken
            );
            
            // Echo received message text
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Your message will be reviewed",
                cancellationToken: cancellationToken);
            
            Console.WriteLine("Message review sent");
        }
        
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}