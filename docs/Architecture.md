# Архитектура

Описание архитектуры и дизайна библиотеки Yandex.SmartCaptcha.

## Обзор

Библиотека спроектирована с учетом принципов SOLID и enterprise требований:

- **Типобезопасность**: полная поддержка nullable reference types
- **Асинхронность**: async/await паттерны везде
- **Dependency Injection**: готовая интеграция с `Microsoft.Extensions.DI`
- **Логирование**: встроенная поддержка `Microsoft.Extensions.Logging`
- **Тестируемость**: все компоненты легко мокаются

## Структура проекта

```shell
src/Yandex.SmartCaptcha/
├── ISmartCaptchaValidator.cs        # Основной интерфейс
├── SmartCaptchaValidator.cs         # Реализация валидатора
├── SmartCaptchaValidationResult.cs  # Результат валидации
├── SmartCaptchaSettings.cs          # Настройки
├── SmartCaptchaAuthType.cs          # Типы аутентификации
└── ServiceCollectionExtensions.cs   # DI расширения
```

## Основные компоненты

### ISmartCaptchaValidator

Главный интерфейс для валидации токенов:

```csharp
public interface ISmartCaptchaValidator
{
    Task<SmartCaptchaValidationResult> ValidateAsync(
        string token,
        string? clientIp = null,
        CancellationToken cancellationToken = default);
}
```

### SmartCaptchaValidationResult

Иммутабельная структура результата:

```csharp
public readonly record struct SmartCaptchaValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Status { get; init; }
}
```

### SmartCaptchaSettings

Конфигурация валидатора:

```csharp
public sealed class SmartCaptchaSettings
{
    public bool IsEnabled { get; set; } = true;
    public SmartCaptchaAuthType AuthType { get; set; } = SmartCaptchaAuthType.SecretKey;
    public string SecretKey { get; set; } = string.Empty;
    public string? IamToken { get; set; }
    public string? ApiKey { get; set; }
    public IList<string> SkipIpAddresses { get; set; } = [];
    public int TimeoutSeconds { get; set; } = 30;
}
```

## Типы аутентификации

### Secret Key

Стандартный метод с form-encoded параметрами:

```http
POST https://smartcaptcha.yandexcloud.net/validate
Content-Type: application/x-www-form-urlencoded

secret=YOUR_SECRET&token=TOKEN&ip=127.0.0.1
```

### IAM Token

Для Yandex Cloud с JSON payload:

```http
POST https://smartcaptcha.yandexcloud.net/validate
Authorization: Bearer YOUR_IAM_TOKEN
Content-Type: application/json

{"token": "TOKEN", "ip": "127.0.0.1"}
```

### API Key

Альтернативный метод с JSON:

```http
POST https://smartcaptcha.yandexcloud.net/validate
Authorization: Api-Key YOUR_API_KEY
Content-Type: application/json

{"token": "TOKEN", "ip": "127.0.0.1"}
```

## Dependency Injection

### Регистрация сервисов

```csharp
// Базовая регистрация
services.AddSmartCaptcha(configuration);

// С HttpClient (рекомендуется)
services.AddSmartCaptchaWithHttpClient(configuration);
```

### HttpClient management

При использовании `AddSmartCaptchaWithHttpClient`:

- HttpClient управляется через `IHttpClientFactory`
- Автоматическое управление соединениями
- Поддержка retry policies и circuit breakers
- Лучшая производительность в enterprise сценариях

## Обработка ошибок

### Типы ошибок

1. **ArgumentException** – некорректные параметры (eager validation)
2. **HttpRequestException** – сетевые ошибки
3. **TaskCanceledException** – таймауты
4. **JsonException** - ошибки парсинга ответа
5. **InvalidOperationException** – ошибки конфигурации

### Async validation pattern

```csharp
// Публичный метод - синхронная валидация аргументов
public Task<SmartCaptchaValidationResult> ValidateAsync(string token, ...)
{
    if (string.IsNullOrWhiteSpace(token))
        throw new ArgumentException("Токен не может быть пустым.", nameof(token));

    return ValidateAsyncCore(token, ...);
}

// Приватный метод - async реализация
private async Task<SmartCaptchaValidationResult> ValidateAsyncCore(...)
{
    // Реальная async логика
}
```

## Логирование

### Уровни логирования

- **Debug**: детали HTTP запросов и ответов
- **Information**: успешные валидации
- **Warning**: неуспешные валидации, пропущенные IP
- **Error**: сетевые ошибки, таймауты

### Structured logging

```csharp
_logger.LogDebug("Валидация каптчи пропущена для IP {ClientIp}", clientIp);
_logger.LogError(httpEx, "Ошибка HTTP при валидации каптчи");
```

## Производительность

### Рекомендации

1. **HttpClient pooling**: используйте `AddSmartCaptchaWithHttpClient`
2. **Таймауты**: настройте разумные значения (10-30 сек)
3. **IP caching**: кешируйте результаты для trusted IP
4. **Graceful degradation**: обрабатывайте недоступность сервиса

### Метрики

Рекомендуется отслеживать:

- Время отклика API
- Количество успешных/неуспешных валидаций
- Количество сетевых ошибок
- Использование пропуска IP

## Безопасность

### Защита ключей

```csharp
// ❌ Плохо - хардкод в коде
var settings = new SmartCaptchaSettings { SecretKey = "secret123" };

// ✅ Хорошо - из конфигурации
builder.Services.AddSmartCaptcha(builder.Configuration);

// ✅ Еще лучше - из Azure Key Vault / переменных среды
builder.Configuration.AddAzureKeyVault(...);
```

### Валидация IP

```csharp
var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
var result = await validator.ValidateAsync(token, clientIp);
```

IP адрес повышает точность валидации и помогает предотвратить атаки.

## Тестирование

### Unit тесты

```csharp
// Мокирование интерфейса
var mockValidator = new Mock<ISmartCaptchaValidator>();
mockValidator.Setup(x => x.ValidateAsync(It.IsAny<string>(), null, default))
            .ReturnsAsync(SmartCaptchaValidationResult.Success());
```

### Integration тесты

```csharp
// Замена в TestServer
services.Replace(ServiceDescriptor.Scoped<ISmartCaptchaValidator, MockValidator>());
```

### Test doubles

Для разных сценариев тестирования библиотека предоставляет легкое мокирование через DI.

## Паттерны использования

### 1. Простое использование

```csharp
var settings = new SmartCaptchaSettings { SecretKey = "key" };
var validator = new SmartCaptchaValidator(settings);
var result = await validator.ValidateAsync(token, clientIp);
```

### 2. Dependency Injection

```csharp
// Регистрация
services.AddSmartCaptcha(configuration);

// Использование
public class AuthController(ISmartCaptchaValidator validator)
{
    public async Task<bool> ValidateLogin(string token, string ip)
    {
        var result = await validator.ValidateAsync(token, ip);
        return result.IsValid;
    }
}
```

## Расширенные примеры

### Добавление логирования

```csharp
services.AddSmartCaptcha(configuration);
services.Decorate<ISmartCaptchaValidator>((validator, provider) =>
{
    var logger = provider.GetService<ILogger<SmartCaptchaValidator>>();
    return new LoggingSmartCaptchaValidator(validator, logger);
});
```

### Добавление метрик

```csharp
public class MetricsSmartCaptchaValidator : ISmartCaptchaValidator
{
    private readonly ISmartCaptchaValidator _inner;
    private readonly IMetrics _metrics;

    public async Task<SmartCaptchaValidationResult> ValidateAsync(
        string token, string? clientIp, CancellationToken ct)
    {
        using var timer = _metrics.Measure.Timer.Time("captcha.validation");
        var result = await _inner.ValidateAsync(token, clientIp, ct);

        _metrics.Measure.Counter.Increment("captcha.validations",
            new MetricTags("success", result.IsValid.ToString()));

        return result;
    }
}
```

## Расширенные примеры тестирования

### Unit тестирование с FluentAssertions

```csharp
[Fact]
public async Task ValidateAsync_WithDisabledCaptcha_ReturnsSuccess()
{
    var settings = new SmartCaptchaSettings { IsEnabled = false };
    var validator = new SmartCaptchaValidator(settings);

    var result = await validator.ValidateAsync("any_token");

    result.IsValid.Should().BeTrue();
}
```

### Интеграционное тестирование

```csharp
[Fact]
public async Task ValidateAsync_WithRealApi_WorksCorrectly()
{
    var settings = new SmartCaptchaSettings
    {
        SecretKey = TestConfiguration.SmartCaptchaSecretKey
    };
    var validator = new SmartCaptchaValidator(settings);

    // Тест с реальным API...
}
```

### Моки для тестирования

```csharp
public class MockSmartCaptchaValidator : ISmartCaptchaValidator
{
    private readonly bool _shouldSucceed;

    public MockSmartCaptchaValidator(bool shouldSucceed = true)
    {
        _shouldSucceed = shouldSucceed;
    }

    public Task<SmartCaptchaValidationResult> ValidateAsync(
        string token, string? clientIp, CancellationToken ct)
    {
        return Task.FromResult(_shouldSucceed
            ? SmartCaptchaValidationResult.Success()
            : SmartCaptchaValidationResult.Failure("Mock failure"));
    }
}
```

## Конфигурация

### Базовая конфигурация

```json
{
  "SmartCaptcha": {
    "IsEnabled": true,
    "AuthType": "SecretKey",
    "SecretKey": "your_key",
    "TimeoutSeconds": 30,
    "SkipIpAddresses": ["127.0.0.1"]
  }
}
```

### Продвинутая конфигурация

```csharp
var settings = new SmartCaptchaSettings
{
    IsEnabled = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development",
    AuthType = SmartCaptchaAuthType.SecretKey,
    SecretKey = configuration["SmartCaptcha:SecretKey"],
    SkipIpAddresses = ["127.0.0.1", "::1", "10.0.0.0/8"],
    TimeoutSeconds = 30
};
```

## Performance соображения

### HTTP клиент

Библиотека управляет HttpClient автоматически:

```csharp
// Автоматическое управление HttpClient
var validator = new SmartCaptchaValidator(settings);

// Или с внешним HttpClient для продвинутых сценариев
services.AddHttpClient<SmartCaptchaValidator>();
```

### Таймауты и retry

```csharp
var settings = new SmartCaptchaSettings
{
    TimeoutSeconds = 10 // Короткий таймаут для лучшего UX
};

// Для retry логики используйте Polly
services.AddHttpClient<SmartCaptchaValidator>()
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));
```

## Продвинутая безопасность

### Защита секретных ключей

```csharp
// Используйте Azure Key Vault, AWS Secrets Manager и т.д.
services.Configure<SmartCaptchaSettings>(options =>
{
    options.SecretKey = keyVaultService.GetSecret("smartcaptcha-secret");
});
```

### Расширенная валидация IP

```csharp
var settings = new SmartCaptchaSettings
{
    // Пропускать валидацию только для известных IP
    SkipIpAddresses = ["10.0.0.0/8"] // Только внутренние IP
};
```

## Миграция с других решений

См. [MIGRATION.md](MIGRATION.md) для подробного руководства по миграции с Google reCAPTCHA.
