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

    [Theory]
    [InlineData(TestConstants.DefaultToken)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ValidateAsync_WithDisabledValidation_ReturnsSuccess(string token)
    {
        // Arrange
        SmartCaptchaValidator validator = CreateValidator(CreateDisabledSettings());

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(token);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(TestConstants.DefaultToken)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ValidateAsync_WithSkippedIp_ReturnsSuccess(string token)
    {
        // Arrange
        SmartCaptchaValidator validator = CreateValidator(CreateSettingsWithSkipIps(TestConstants.DefaultIp));

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(token, TestConstants.DefaultIp);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
