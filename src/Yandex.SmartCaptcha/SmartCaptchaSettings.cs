namespace Yandex.SmartCaptcha;

/// <summary>
/// Настройки для Yandex SmartCaptcha.
/// </summary>
public sealed class SmartCaptchaSettings
{
    /// <summary>
    /// Нужно ли выполнять валидацию каптчи.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Тип аутентификации для SmartCaptcha API.
    /// </summary>
    public SmartCaptchaAuthType AuthType { get; set; } = SmartCaptchaAuthType.SecretKey;

    /// <summary>
    /// Серверный ключ для валидации каптчи.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// IAM токен для аутентификации (альтернатива SecretKey).
    /// Используется для продвинутых сценариев с Yandex Cloud.
    /// </summary>
    public string? IamToken { get; set; }

    /// <summary>
    /// API ключ для аутентификации (альтернатива SecretKey).
    /// Используется для упрощенной аутентификации.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Список IP-адресов, для которых валидация каптчи пропускается.
    /// Используется для разработки и внутренних сервисов.
    /// </summary>
    public IList<string> SkipIpAddresses { get; set; } = [];

    /// <summary>
    /// Таймаут HTTP-запроса к сервису валидации каптчи (в секундах).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
