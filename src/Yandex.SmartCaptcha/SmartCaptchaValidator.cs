using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Yandex.SmartCaptcha;

/// <summary>
/// Валидатор SmartCaptcha с поддержкой различных типов аутентификации.
/// </summary>
public sealed class SmartCaptchaValidator : ISmartCaptchaValidator, IDisposable
{
    private const string ValidationEndpoint = "validate";

    private readonly SmartCaptchaSettings _settings;
    private readonly ILogger<SmartCaptchaValidator>? _logger;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// Создает экземпляр валидатора SmartCaptcha с внешним HttpClient (предпочтительный способ).
    /// </summary>
    /// <param name="httpClient">HTTP клиент.</param>
    /// <param name="options">Настройки SmartCaptcha.</param>
    /// <param name="logger">Логгер (опционально).</param>
    public SmartCaptchaValidator(HttpClient httpClient, IOptions<SmartCaptchaSettings> options, ILogger<SmartCaptchaValidator>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _ownsHttpClient = false;

        ValidateSettings();
    }

    /// <summary>
    /// Создает экземпляр валидатора SmartCaptcha с собственным HttpClient.
    /// </summary>
    /// <param name="settings">Настройки SmartCaptcha.</param>
    /// <param name="logger">Логгер (опционально).</param>
    public SmartCaptchaValidator(SmartCaptchaSettings settings, ILogger<SmartCaptchaValidator>? logger = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://smartcaptcha.yandexcloud.net/"),
            Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds),
        };
        _ownsHttpClient = true;

        ValidateSettings();
    }

    /// <summary>
    /// Валидирует корректность настроек.
    /// </summary>
    private void ValidateSettings()
    {
        switch (_settings.AuthType)
        {
            case SmartCaptchaAuthType.SecretKey:
                if (string.IsNullOrEmpty(_settings.SecretKey))
                    throw new InvalidOperationException("SecretKey не может быть пустым.");
                break;
            case SmartCaptchaAuthType.IamToken:
                if (string.IsNullOrEmpty(_settings.IamToken))
                    throw new InvalidOperationException("IamToken не может быть пустым.");
                break;
            case SmartCaptchaAuthType.ApiKey:
                if (string.IsNullOrEmpty(_settings.ApiKey))
                    throw new InvalidOperationException("ApiKey не может быть пустым.");
                break;
            default:
                throw new InvalidOperationException($"Неподдерживаемый тип аутентификации: {_settings.AuthType}");
        }
    }

    /// <inheritdoc />
    public Task<SmartCaptchaValidationResult> ValidateAsync(
        string token,
        string? clientIp = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Токен не может быть пустым.", nameof(token));

        return ValidateAsyncCore(token, clientIp, cancellationToken);
    }

    /// <summary>
    /// Внутренняя реализация валидации токена.
    /// </summary>
    private async Task<SmartCaptchaValidationResult> ValidateAsyncCore(
        string token,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        if (!_settings.IsEnabled)
        {
            _logger?.LogDebug("Валидация каптчи отключена в настройках");
            return SmartCaptchaValidationResult.Success();
        }

        if (!string.IsNullOrEmpty(clientIp) && _settings.SkipIpAddresses.Contains(clientIp, StringComparer.OrdinalIgnoreCase))
        {
            _logger?.LogDebug("Валидация каптчи пропущена для IP {ClientIp}", clientIp);
            return SmartCaptchaValidationResult.Success();
        }

        try
        {
            return await PerformValidationAsync(token, clientIp, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException httpEx)
        {
            _logger?.LogError(httpEx, "Ошибка HTTP при валидации каптчи");
            return SmartCaptchaValidationResult.Failure("Сервис валидации каптчи недоступен");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger?.LogWarning("Валидация каптчи отменена по запросу");
            return SmartCaptchaValidationResult.Failure("Валидация каптчи отменена");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Неожиданная ошибка при валидации каптчи");
            return SmartCaptchaValidationResult.Failure("Внутренняя ошибка валидации каптчи");
        }
    }

    /// <summary>
    /// Выполняет фактическую валидацию токена через API SmartCaptcha.
    /// </summary>
    private async Task<SmartCaptchaValidationResult> PerformValidationAsync(
        string token,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        _logger?.LogDebug("Отправка запроса валидации каптчи к Yandex SmartCaptcha");

        HttpResponseMessage response = _settings.AuthType switch
        {
            SmartCaptchaAuthType.SecretKey => await SendSecretKeyRequestAsync(token, clientIp, cancellationToken).ConfigureAwait(false),
            SmartCaptchaAuthType.IamToken => await SendIamTokenRequestAsync(token, clientIp, cancellationToken).ConfigureAwait(false),
            SmartCaptchaAuthType.ApiKey => await SendApiKeyRequestAsync(token, clientIp, cancellationToken).ConfigureAwait(false),
            _ => throw new InvalidOperationException($"Неподдерживаемый тип аутентификации: {_settings.AuthType}"),
        };

        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogWarning(
                "Сервис валидации каптчи вернул ошибку: {StatusCode} {ReasonPhrase}",
                response.StatusCode,
                response.ReasonPhrase);
            return SmartCaptchaValidationResult.Failure("Сервис валидации каптчи недоступен");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Получен ответ от Yandex SmartCaptcha: {Response}", jsonResponse);

        return ParseValidationResponse(jsonResponse);
    }

    /// <summary>
    /// Отправляет запрос с аутентификацией через Secret Key (форма).
    /// </summary>
    private async Task<HttpResponseMessage> SendSecretKeyRequestAsync(
        string token,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_settings.SecretKey))
            throw new InvalidOperationException("SecretKey не может быть пустым.");

        List<KeyValuePair<string, string>> formData = [
            new("secret", _settings.SecretKey),
            new("token", token),
        ];

        if (!string.IsNullOrEmpty(clientIp))
        {
            formData.Add(new("ip", clientIp));
        }

        using var content = new FormUrlEncodedContent(formData);
        return await _httpClient.PostAsync(ValidationEndpoint, content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Отправляет запрос с аутентификацией через IAM Token (JSON).
    /// </summary>
    private async Task<HttpResponseMessage> SendIamTokenRequestAsync(
        string token,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_settings.IamToken))
            throw new InvalidOperationException("IamToken не может быть пустым.");

        using var request = new HttpRequestMessage(HttpMethod.Post, ValidationEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.IamToken);

        var requestBody = new { token, ip = clientIp };
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            System.Text.Encoding.UTF8,
            "application/json");

        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Отправляет запрос с аутентификацией через API Key (заголовок).
    /// </summary>
    private async Task<HttpResponseMessage> SendApiKeyRequestAsync(
        string token,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
            throw new InvalidOperationException("ApiKey не может быть пустым.");

        using var request = new HttpRequestMessage(HttpMethod.Post, ValidationEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Api-Key", _settings.ApiKey);

        var requestBody = new { token, ip = clientIp };
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            System.Text.Encoding.UTF8,
            "application/json");

        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Парсит ответ от SmartCaptcha и возвращает результат валидации.
    /// </summary>
    private SmartCaptchaValidationResult ParseValidationResponse(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("status", out var statusElement))
            {
                return SmartCaptchaValidationResult.Failure("Некорректный ответ от сервиса: отсутствует поле status");
            }

            var status = statusElement.GetString();
            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;

            if (string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogDebug("Валидация каптчи успешна");
                return SmartCaptchaValidationResult.Success(status ?? "ok");
            }

            _logger?.LogDebug("Валидация каптчи не пройдена. Статус: {Status}, Сообщение: {Message}", status, message);
            return SmartCaptchaValidationResult.Failure(
                message ?? "Валидация каптчи не пройдена",
                status ?? "unknown");
        }
        catch (JsonException jsonEx)
        {
            _logger?.LogError(jsonEx, "Ошибка парсинга ответа от сервиса валидации каптчи");
            return SmartCaptchaValidationResult.Failure("Некорректный ответ от сервиса валидации каптчи");
        }
    }

    /// <summary>
    /// Освобождает ресурсы.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
