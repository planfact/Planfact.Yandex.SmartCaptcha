using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Yandex.SmartCaptcha.Tests;

public class SmartCaptchaValidatorConfigurationTests : SmartCaptchaTestsBase
{
    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act & Assert
        FluentActions.Invoking(() => new SmartCaptchaValidator(null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        IOptions<SmartCaptchaSettings> options = Options.Create(CreateDefaultSettings());

        // Act & Assert
        FluentActions.Invoking(() => new SmartCaptchaValidator(null!, options))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act & Assert
        FluentActions.Invoking(() => new SmartCaptchaValidator(httpClient, null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithValidSettings_CreatesInstance()
    {
        // Arrange
        SmartCaptchaSettings settings = CreateDefaultSettings();

        // Act
        var validator = new SmartCaptchaValidator(settings);

        // Assert
        validator.Should().NotBeNull();
    }
}
