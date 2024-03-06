using Moq.Protected;
using Moq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class TelegramBotServiceTests
{
    [Fact]
    public async Task SendMessageAsync_SendsMessageSuccessfully()
    {
        // Arrange
        var botToken = "your-bot-token";
        var chatId = "your-chat-id";
        var message = "Test message";
        var expectedUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";
        var expectedContent = "{\"chat_id\":\"your-chat-id\",\"text\":\"Test message\"}";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);
        var botService = new TelegramBotService(botToken, chatId, httpClient);

        // Act
        await botService.SendMessageAsync(message);

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri == new Uri(expectedUrl)
                && req.Content.ReadAsStringAsync().Result == expectedContent),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUpdatesAsync_ReturnsUpdatesSuccessfully()
    {
        // Arrange
        var botToken = "your-bot-token";
        var chatId = "your-chat-id";
        var expectedUrl = $"https://api.telegram.org/bot{botToken}/getUpdates";
        var expectedContent = "{\"offset\":0,\"limit\":10,\"timeout\":0}";

        var responseContent = "{\"result\":[{\"update_id\":123,\"message\":{\"message_id\":123,\"text\":\"Test message\"}}]}";
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);
        var botService = new TelegramBotService(botToken, chatId, httpClient);

        // Act
        var updatesResponse = await botService.GetUpdatesAsync();

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri == new Uri(expectedUrl)
                && req.Content.ReadAsStringAsync().Result == expectedContent),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal(responseContent, updatesResponse);
    }
}
