using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reliable.HttpClient;

namespace Yandex.SmartCaptcha;

/// <summary>
/// Методы расширения для регистрации SmartCaptcha в DI контейнере.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет SmartCaptcha валидатор в коллекцию сервисов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="sectionName">Имя секции конфигурации (по умолчанию "SmartCaptcha").</param>
    /// <returns>Коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddSmartCaptcha(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SmartCaptcha")
    {
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Имя секции не может быть пустым.", nameof(sectionName));

        services.Configure<SmartCaptchaSettings>(configuration.GetSection(sectionName));
        
        // Добавляем HttpClient с resilience patterns для надежности
        services.AddHttpClient<ISmartCaptchaValidator, SmartCaptchaValidator>(client =>
        {
            client.BaseAddress = new Uri("https://smartcaptcha.yandexcloud.net/");
            client.Timeout = TimeSpan.FromSeconds(30); // Базовый timeout
        })
        .AddResilience(); // Автоматические retry + circuit breaker

        return services;
    }

    /// <summary>
    /// Добавляет SmartCaptcha валидатор с настройками в коллекцию сервисов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="settings">Настройки SmartCaptcha.</param>
    /// <returns>Коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddSmartCaptcha(
        this IServiceCollection services,
        SmartCaptchaSettings settings)
    {
        services.TryAddSingleton(settings);
        
        // Добавляем HttpClient с resilience patterns для надежности
        services.AddHttpClient<ISmartCaptchaValidator, SmartCaptchaValidator>(client =>
        {
            client.BaseAddress = new Uri("https://smartcaptcha.yandexcloud.net/");
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        })
        .AddResilience(); // Автоматические retry + circuit breaker

        return services;
    }

    /// <summary>
    /// Добавляет SmartCaptcha валидатор с HttpClient для улучшенного управления соединениями.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="sectionName">Имя секции конфигурации (по умолчанию "SmartCaptcha").</param>
    /// <returns>Коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddSmartCaptchaWithHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SmartCaptcha")
    {
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Имя секции не может быть пустым.", nameof(sectionName));

        services.Configure<SmartCaptchaSettings>(configuration.GetSection(sectionName));

        services.AddHttpClient<ISmartCaptchaValidator, SmartCaptchaValidator>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://smartcaptcha.yandexcloud.net");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddResilience(); // Автоматические retry + circuit breaker

        return services;
    }
}
