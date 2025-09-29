# Конфигурирование

## Типы аутентификации

Planfact.Yandex.SmartCaptcha поддерживает три типа аутентификации:

### 1. Secret Key (рекомендуется)

Самый простой способ для базового использования:

```csharp
var settings = new SmartCaptchaSettings
{
    SecretKey = "YOUR_SECRET_KEY"
};
```

### 2. IAM Token

Для использования с IAM-токенами Yandex Cloud:

```csharp
var settings = new SmartCaptchaSettings
{
    AuthType = SmartCaptchaAuthType.IamToken,
    IamToken = "YOUR_IAM_TOKEN"
};
```

### 3. API Key

Для использования с API-ключами:

```csharp
var settings = new SmartCaptchaSettings
{
    AuthType = SmartCaptchaAuthType.ApiKey,
    ApiKey = "YOUR_API_KEY"
};
```

## Настройки конфигурации

### SmartCaptchaSettings

Основной класс для настройки библиотеки. Поддерживает следующие параметры:

- **IsEnabled** - нужно ли выполнять валидацию каптчи (по умолчанию: true)
- **AuthType** - тип аутентификации (по умолчанию: SecretKey)
- **SecretKey** - серверный ключ для валидации каптчи
- **IamToken** - IAM токен для аутентификации (альтернатива SecretKey)
- **ApiKey** - API ключ для аутентификации (альтернатива SecretKey)
- **SkipIpAddresses** - список IP-адресов, для которых валидация пропускается
- **TimeoutSeconds** - таймаут HTTP-запроса в секундах (по умолчанию: 30)

> 📖 Полное описание класса см. в исходном коде: `src/Planfact.Yandex.SmartCaptcha/SmartCaptchaSettings.cs`

## Интеграция с ASP.NET Core

### Базовая настройка

```csharp
// Program.cs
builder.Services.AddSmartCaptcha(builder.Configuration);

// appsettings.json
{
  "SmartCaptcha": {
    "SecretKey": "YOUR_SECRET_KEY"
  }
}
```

### Расширенная настройка

```csharp
// Program.cs - настройка через объект
var settings = new SmartCaptchaSettings
{
    SecretKey = builder.Configuration["SmartCaptcha:SecretKey"],
    TimeoutSeconds = 15,
    SkipIpAddresses = ["127.0.0.1", "::1"]
};
builder.Services.AddSmartCaptcha(settings);

// Или через конфигурацию
builder.Services.Configure<SmartCaptchaSettings>(settings =>
{
    builder.Configuration.GetSection("SmartCaptcha").Bind(settings);
});
```

### Настройка с управляемым HttpClient

```csharp
// Используйте AddSmartCaptchaWithHttpClient для автоматической настройки HttpClient
builder.Services.AddSmartCaptchaWithHttpClient(builder.Configuration);

// HttpClient автоматически настраивается с:
// - BaseAddress: https://smartcaptcha.yandexcloud.net
// - Timeout: 30 секунд
```

## Конфигурация для разных сред

### Development

```json
{
  "SmartCaptcha": {
    "IsEnabled": false,
    "SecretKey": "dev-secret-key"
  }
}
```

### Production

```json
{
  "SmartCaptcha": {
    "IsEnabled": true,
    "SecretKey": "prod-secret-key",
    "TimeoutSeconds": 15,
    "SkipIpAddresses": ["10.0.0.0/8", "192.168.0.0/16"]
  }
}
```

### Переменные окружения

```bash
# Docker или Kubernetes
SMARTCAPTCHA__ISENABLED=true
SMARTCAPTCHA__SECRETKEY=your_secret_key
SMARTCAPTCHA__TIMEOUTSECONDS=30
SMARTCAPTCHA__SKIPIPADDRESSES__0=127.0.0.1
SMARTCAPTCHA__SKIPIPADDRESSES__1=::1
```

## Валидация конфигурации

Библиотека автоматически валидирует настройки при инициализации:

- Проверяет наличие ключа аутентификации для выбранного типа
- Валидирует корректность URL
- Проверяет допустимые значения таймаута

```csharp
// Пример обработки ошибок конфигурации
try
{
    var settings = new SmartCaptchaSettings { /* настройки */ };
    var validator = new SmartCaptchaValidator(settings);
}
catch (ArgumentException ex)
{
    // Ошибка конфигурации
    Console.WriteLine($"Ошибка настройки: {ex.Message}");
}
```

## Лучшие практики

1. **Безопасность**: Никогда не храните ключи в коде - используйте конфигурацию или переменные окружения
2. **Надежность**: Используйте стандартную регистрацию `AddSmartCaptcha()` для автоматических resilience patterns
3. **Мониторинг**: Включите логирование для отслеживания ошибок валидации и retry событий
4. **Тестирование**: Используйте `IsEnabled = false` в тестовой среде
5. **Развертывание**: Настройте `SkipIpAddresses` для внутренних сервисов
6. **Production**: Доверьтесь встроенным retry и circuit breaker patterns для высоконагруженных систем

### Resilience Configuration

Библиотека автоматически настраивает resilience patterns через Reliable.HttpClient:

```csharp
// Автоматическая настройка (рекомендуется)
builder.Services.AddSmartCaptcha(builder.Configuration);
// Включает: retry policies, circuit breaker, timeouts

// Кастомная настройка resilience (при необходимости)
builder.Services.AddHttpClient<ISmartCaptchaValidator, SmartCaptchaValidator>()
    .ConfigureHttpClient(client => /* настройки */)
    .AddResilience(); // Добавляет resilience patterns
```

Встроенные resilience patterns обеспечивают:

- **Автоматические повторы** при временных сбоях
- **Circuit breaker** для защиты от каскадных отказов
- **Exponential backoff** с jitter для оптимального retry
- **Timeout policies** на разных уровнях
