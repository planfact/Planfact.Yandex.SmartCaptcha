using System.Net;
using System.Text;

using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

namespace Yandex.SmartCaptcha.Tests;

/// <summary>
/// Базовый класс для тестов с общими вспомогательными методами.
/// </summary>
public abstract class SmartCaptchaTestsBase
{
    /// <summary>
    /// Создает стандартные настройки с SecretKey для тестов.
    /// </summary>
    protected static SmartCaptchaSettings CreateDefaultSettings(string secretKey = "test_key") =>
        new() { SecretKey = secretKey };

    /// <summary>
    /// Создает настройки с отключенной валидацией для unit-тестов.
    /// </summary>
    protected static SmartCaptchaSettings CreateDisabledSettings(string secretKey = "test_key") =>
        new() { SecretKey = secretKey, IsEnabled = false };

    /// <summary>
    /// Создает настройки для конкретного типа аутентификации.
    /// </summary>
    protected static SmartCaptchaSettings CreateAuthSettings(SmartCaptchaAuthType authType, string authValue, bool isEnabled = false) =>
        authType switch
        {
            SmartCaptchaAuthType.SecretKey => new() { SecretKey = authValue, IsEnabled = isEnabled },
            SmartCaptchaAuthType.IamToken => new() { AuthType = authType, IamToken = authValue, IsEnabled = isEnabled },
            SmartCaptchaAuthType.ApiKey => new() { AuthType = authType, ApiKey = authValue, IsEnabled = isEnabled },
            _ => throw new ArgumentOutOfRangeException(nameof(authType)),
        };

    /// <summary>
    /// Создает настройки с пропуском IP-адресов.
    /// </summary>
    protected static SmartCaptchaSettings CreateSettingsWithSkipIps(params string[] skipIps) =>
        new()
        {
            SecretKey = "test_key",
            SkipIpAddresses = [.. skipIps],
        };

    /// <summary>
    /// Создает валидатор с обычными настройками.
    /// </summary>
    protected static SmartCaptchaValidator CreateValidator(SmartCaptchaSettings? settings = null) =>
        new(settings ?? CreateDefaultSettings());

    /// <summary>
    /// Создает валидатор с HttpClient и Options pattern.
    /// </summary>
    protected static SmartCaptchaValidator CreateValidatorWithHttpClient(
        HttpClient httpClient,
        SmartCaptchaSettings? settings = null) =>
        new(httpClient, Options.Create(settings ?? CreateDefaultSettings()));

    /// <summary>
    /// Создает мок HttpMessageHandler с успешным ответом.
    /// </summary>
    protected static Mock<HttpMessageHandler> CreateSuccessfulHttpMock(string status = "ok", string message = "") =>
        CreateHttpMock(HttpStatusCode.OK, $"{{\"status\":\"{status}\",\"message\":\"{message}\"}}");

    /// <summary>
    /// Создает мок HttpMessageHandler с неуспешным ответом.
    /// </summary>
    protected static Mock<HttpMessageHandler> CreateFailureHttpMock(string status = "failed", string message = "Invalid token") =>
        CreateHttpMock(HttpStatusCode.OK, $"{{\"status\":\"{status}\",\"message\":\"{message}\"}}");

    /// <summary>
    /// Создает мок HttpMessageHandler с HTTP ошибкой.
    /// </summary>
    protected static Mock<HttpMessageHandler> CreateErrorHttpMock(HttpStatusCode statusCode = HttpStatusCode.InternalServerError) =>
        CreateHttpMock(statusCode, "Internal Server Error");

    /// <summary>
    /// Создает базовый мок HttpMessageHandler.
    /// </summary>
    private static Mock<HttpMessageHandler> CreateHttpMock(HttpStatusCode statusCode, string content)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
            });
        return mockHandler;
    }

    /// <summary>
    /// Создает мок HttpMessageHandler с возможностью перехвата запроса.
    /// </summary>
    protected static (Mock<HttpMessageHandler> mock, List<HttpRequestMessage> capturedRequests) CreateCapturingHttpMock()
    {
        var capturedRequests = new List<HttpRequestMessage>();
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequests.Add(req))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"ok\",\"message\":\"\"}", Encoding.UTF8, "application/json"),
            });

        return (mockHandler, capturedRequests);
    }

    /// <summary>
    /// Стандартные тестовые константы.
    /// </summary>
    protected static class TestConstants
    {
        public const string DefaultSecretKey = "test_key";
        public const string DefaultToken = "test_token";
        public const string DefaultIp = "127.0.0.1";
        public const string InvalidToken = "invalid_token";
        public const string IamTokenValue = "test_iam_token";
        public const string ApiKeyValue = "test_api_key";
    }
}
