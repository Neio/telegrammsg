using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class TelegramBotService
{
    private readonly string botToken;
    private readonly string chatId;
    private readonly HttpClient httpClient;

    public TelegramBotService(string botToken, string chatId, HttpClient httpClient)
    {
        this.botToken = botToken;
        this.chatId = chatId;
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task SendMessageAsync(string message)
    {
        string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

        var parameters = new
        {
            chat_id = chatId,
            text = message
        };

        var json = JsonSerializer.Serialize(parameters);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

    }

    public async Task<string> GetUpdatesAsync()
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

        HttpResponseMessage response = await httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();

    }
}
