# Yandex.SmartCaptcha

[![NuGet Version](https://img.shields.io/nuget/v/Yandex.SmartCaptcha?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Yandex.SmartCaptcha/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Yandex.SmartCaptcha?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Yandex.SmartCaptcha/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/planfact/Yandex.SmartCaptcha/ci.yml?branch=main&style=flat-square&logo=github)](https://github.com/planfact/Yandex.SmartCaptcha/actions)
[![codecov](https://img.shields.io/codecov/c/github/planfact/Yandex.SmartCaptcha?style=flat-square&logo=codecov)](https://codecov.io/gh/planfact/Yandex.SmartCaptcha)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-6.0%7C8.0%7C9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![GitHub release](https://img.shields.io/github/v/release/planfact/Yandex.SmartCaptcha?style=flat-square&logo=github)](https://github.com/planfact/Yandex.SmartCaptcha/releases)

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
- 🛡️ **Встроенная надежность**: автоматические retry, circuit breaker, и resilience patterns
- 🚀 **Enterprise-готовность**: основан на Reliable.HttpClient для production-нагрузок
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
