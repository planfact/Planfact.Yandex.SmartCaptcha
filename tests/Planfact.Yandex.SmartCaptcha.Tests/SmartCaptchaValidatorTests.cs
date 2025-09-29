using FluentAssertions;
using Xunit;

namespace Planfact.Yandex.SmartCaptcha.Tests;

public class SmartCaptchaValidatorTests : SmartCaptchaTestsBase
{
    [Fact]
    public async Task ValidateAsync_WithEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        SmartCaptchaValidator validator = CreateValidator();

        // Act & Assert
        await FluentActions.Invoking(() => validator.ValidateAsync(""))
            .Should().ThrowAsync<ArgumentException>()
            .WithParameterName("token");
    }

    [Fact]
    public async Task ValidateAsync_WithDisabledValidation_ReturnsSuccess()
    {
        // Arrange
        SmartCaptchaValidator validator = CreateValidator(CreateDisabledSettings());

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithSkippedIp_ReturnsSuccess()
    {
        // Arrange
        SmartCaptchaValidator validator = CreateValidator(CreateSettingsWithSkipIps(TestConstants.DefaultIp));

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken, TestConstants.DefaultIp);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
