using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string botToken;
        string chatId = null;

        // Get the directory where the executable is located
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string configFile = Path.Combine(exeDirectory, "appsettings.json");

        // Read bot token and chat id from appsettings.json
        if (File.Exists(configFile))
        {
            string json = await File.ReadAllTextAsync(configFile);
            var settings = JsonSerializer.Deserialize<Settings>(json);

            botToken = settings.BotToken;
            if (args.Length > 0 && args[0] == "-l")
            {
                chatId = null; // When listing recent chats, chat ID is not needed
            }
            else
            {
                chatId = settings.ChatId;
            }
        }
        else
        {
            Console.WriteLine("appsettings.json not found in the application directory.");
            return;
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
                await SendMessageAsync(botToken, chatId, messageText);
                Console.WriteLine("Message sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
            }
        }
        else if (command == "-l")
        {
            await ShowRecentChatsAsync(botToken);
        }
        else
        {
            Console.WriteLine("Invalid command. Usage: -m <message> or -l");
        }
    }

    static async Task SendMessageAsync(string botToken, string chatId, string message)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

            var parameters = new
            {
                chat_id = chatId,
                text = message
            };

            var json = JsonSerializer.Serialize(parameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }

    static async Task ShowRecentChatsAsync(string botToken)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.telegram.org/bot{botToken}/getUpdates";

            var parameters = new
            {
                offset = 0, // Optional parameter to specify the starting point for the results
                limit = 10, // Optional parameter to limit the number of updates to be retrieved
                timeout = 0 // Optional parameter to set a timeout for long polling
            };

            var json = JsonSerializer.Serialize(parameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content of getUpdates response:");
            Console.WriteLine(responseBody);
        }
    }



    class Settings
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; }
    }

}
