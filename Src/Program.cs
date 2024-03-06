using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        string botToken;
        string chatId;
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string configFile = Path.Combine(exeDirectory, "appsettings.json");
        // Read bot token and chat id from appsettings.json
        using (StreamReader reader = new StreamReader(configFile))
        {
            string json = await reader.ReadToEndAsync();
            var settings = JsonSerializer.Deserialize<Settings>(json);

            botToken = settings.BotToken;
            chatId = settings.ChatId;
        }

        if (string.IsNullOrEmpty(botToken))
        {
            Console.WriteLine("Bot token not found in app settings.");
            return;
        }

        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a valid command. Usage: -m <message> or -l");
            return;
        }

        string command = args[0];
        string parameter = args.Length > 1 ? args[1] : null;
        using (var client = new HttpClient())
        {
            var botService = new TelegramBotService(botToken, chatId, client);



            if (command == "-m")
            {
                if (string.IsNullOrEmpty(parameter))
                {
                    Console.WriteLine("Please provide a message to send.");
                    return;
                }

                string messageText = string.Join(" ", args[1..]);

                try
                {
                    await botService.SendMessageAsync(messageText);
                    Console.WriteLine("Message sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send message: {ex.Message}");
                }
            }
            else if (command == "-l")
            {
                await ShowRecentChatsAsync(botService);
            }
            else
            {
                Console.WriteLine("Invalid command. Usage: -m <message> or -l");
            }
        }
    }

    static async Task ShowRecentChatsAsync(TelegramBotService botService)
    {
        try
        {
            string updatesResponse = await botService.GetUpdatesAsync();
            Console.WriteLine("Content of getUpdates response:");
            Console.WriteLine(updatesResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get updates: {ex.Message}");
        }
    }

    class Settings
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; }
    }
}
