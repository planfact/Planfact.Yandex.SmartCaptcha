namespace Yandex.SmartCaptcha;

/// <summary>
/// Типы аутентификации для Yandex SmartCaptcha API.
/// </summary>
public enum SmartCaptchaAuthType
{
    /// <summary>
    /// Аутентификация через серверный ключ (Secret Key).
    /// Стандартный метод для большинства сценариев.
    /// </summary>
    SecretKey = 1,

    /// <summary>
    /// Аутентификация через IAM токен.
    /// Используется для продвинутых сценариев с Yandex Cloud.
    /// </summary>
    IamToken = 2,

    /// <summary>
    /// Аутентификация через API ключ.
    /// Альтернативный упрощенный метод.
    /// </summary>
    ApiKey = 3,
}
