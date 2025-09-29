namespace Planfact.Yandex.SmartCaptcha;

/// <summary>
/// Интерфейс для валидации SmartCaptcha.
/// </summary>
public interface ISmartCaptchaValidator
{
    /// <summary>
    /// Валидирует токен SmartCaptcha на стороне сервера.
    /// </summary>
    /// <param name="token">Токен каптчи, полученный от клиента.</param>
    /// <param name="clientIp">IP-адрес клиента для дополнительной проверки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат валидации каптчи.</returns>
    Task<SmartCaptchaValidationResult> ValidateAsync(
        string token,
        string? clientIp = null,
        CancellationToken cancellationToken = default);
}
