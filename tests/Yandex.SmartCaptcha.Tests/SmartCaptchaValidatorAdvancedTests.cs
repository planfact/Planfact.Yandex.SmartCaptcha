using FluentAssertions;
using Xunit;

namespace Yandex.SmartCaptcha.Tests;

public class SmartCaptchaValidatorAdvancedTests : SmartCaptchaTestsBase
{
    [Fact]
    public async Task ValidateAsync_WithIamToken_WorksCorrectly()
    {
        // Arrange
        SmartCaptchaSettings settings = CreateAuthSettings(SmartCaptchaAuthType.IamToken, TestConstants.IamTokenValue);
        SmartCaptchaValidator validator = CreateValidator(settings);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithApiKey_WorksCorrectly()
    {
        // Arrange
        SmartCaptchaSettings settings = CreateAuthSettings(SmartCaptchaAuthType.ApiKey, TestConstants.ApiKeyValue);
        SmartCaptchaValidator validator = CreateValidator(settings);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithOptionsPattern_WorksCorrectly()
    {
        // Arrange
        SmartCaptchaSettings settings = CreateDisabledSettings();
        using var httpClient = new HttpClient();
        SmartCaptchaValidator validator = CreateValidatorWithHttpClient(httpClient, settings);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("192.168.1.100")]
    public async Task ValidateAsync_WithSkippedIps_ReturnsSuccess(string skipIp)
    {
        // Arrange
        SmartCaptchaSettings settings = CreateSettingsWithSkipIps("127.0.0.1", "::1", "192.168.1.100");
        SmartCaptchaValidator validator = CreateValidator(settings);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken, skipIp);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
