# Yandex.SmartCaptcha

Идиоматичный и типобезопасный .NET клиент для сервиса [Yandex SmartCaptcha](https://yandex.cloud/ru/docs/smartcaptcha).

## Установка

```bash
dotnet add package Yandex.SmartCaptcha
```

## Быстрый старт

```csharp
using Yandex.SmartCaptcha;

var settings = new SmartCaptchaSettings { SecretKey = "YOUR_SECRET_KEY" };
var validator = new SmartCaptchaValidator(settings);

var result = await validator.ValidateAsync("captcha_token", "127.0.0.1");
if (result.IsValid)
{
    Console.WriteLine("Валидация прошла успешно!");
}
```

## Интеграция с ASP.NET Core

```csharp
// Program.cs
builder.Services.AddSmartCaptcha(builder.Configuration);

// Внедрение зависимости
public class CaptchaController : Controller
{
    private readonly ISmartCaptchaValidator _validator;

    public CaptchaController(ISmartCaptchaValidator validator)
    {
        _validator = validator;
    }

    public async Task<IActionResult> Verify(string token)
    {
        var clientIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _validator.ValidateAsync(token, clientIp);
        return Json(new { isValid = result.IsValid });
    }
}
```

> 💡 **Конфигурирование**: Для настройки IAM Token, API Key и других параметров см. [документацию по конфигурированию](docs/Configuration.md)

## Особенности

- 🔐 **Три типа аутентификации**: Secret Key, IAM Token, API Key
- 🌐 **IP валидация** для повышения точности
- ⚙️ **Гибкие настройки**: пропуск IP-адресов, таймауты, отключение валидации
- 📝 **Интеграция с логированием** Microsoft.Extensions.Logging
- 🧪 **Готовность к тестированию** с поддержкой DI

## Поддержка платформ

- .NET 6.0+
- .NET 8.0+
- .NET 9.0+

## Документация

- [Конфигурирование](docs/Configuration.md)
- [Архитектура](docs/Architecture.md)

## Лицензия

MIT License. См. [LICENSE](LICENSE) для деталей.
