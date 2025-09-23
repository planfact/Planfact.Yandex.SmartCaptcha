namespace Yandex.SmartCaptcha;

/// <summary>
/// Результат валидации SmartCaptcha.
/// </summary>
public readonly record struct SmartCaptchaValidationResult
{
    /// <summary>
    /// Успешность валидации.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Сообщение об ошибке (если валидация не прошла).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Оригинальный статус ответа от SmartCaptcha API.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Создает результат успешной валидации.
    /// </summary>
    /// <returns>Результат успешной валидации.</returns>
    public static SmartCaptchaValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Создает результат успешной валидации с оригинальным статусом.
    /// </summary>
    /// <param name="status">Оригинальный статус от API.</param>
    /// <returns>Результат успешной валидации.</returns>
    public static SmartCaptchaValidationResult Success(string status) =>
        new() { IsValid = true, Status = status };

    /// <summary>
    /// Создает результат неуспешной валидации с сообщением об ошибке.
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке.</param>
    /// <returns>Результат неуспешной валидации.</returns>
    public static SmartCaptchaValidationResult Failure(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage };

    /// <summary>
    /// Создает результат неуспешной валидации с сообщением об ошибке и статусом.
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке.</param>
    /// <param name="status">Оригинальный статус от API.</param>
    /// <returns>Результат неуспешной валидации.</returns>
    public static SmartCaptchaValidationResult Failure(string errorMessage, string status) =>
        new() { IsValid = false, ErrorMessage = errorMessage, Status = status };
}
