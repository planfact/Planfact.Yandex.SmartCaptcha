using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using FluentAssertions;
using Xunit;

namespace Planfact.Yandex.SmartCaptcha.Tests;

public class IntegrationTests : SmartCaptchaTestsBase
{
    [Fact]
    public void AddSmartCaptcha_RegistersServices_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["SmartCaptcha:SecretKey"] = TestConstants.DefaultSecretKey,
            ["SmartCaptcha:IsEnabled"] = "true",
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        // Act
        services.AddSmartCaptcha(configuration);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        ISmartCaptchaValidator? validator = provider.GetService<ISmartCaptchaValidator>();
        validator.Should().NotBeNull();
        validator.Should().BeOfType<SmartCaptchaValidator>();
    }

    [Fact]
    public void SmartCaptchaSettings_Configuration_BindsCorrectly()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["SmartCaptcha:SecretKey"] = TestConstants.DefaultSecretKey,
            ["SmartCaptcha:IsEnabled"] = "false",
            ["SmartCaptcha:AuthType"] = "IamToken",
            ["SmartCaptcha:IamToken"] = TestConstants.IamTokenValue,
            ["SmartCaptcha:TimeoutSeconds"] = "60",
            ["SmartCaptcha:SkipIpAddresses:0"] = "127.0.0.1",
            ["SmartCaptcha:SkipIpAddresses:1"] = "::1",
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        // Act
        var settings = new SmartCaptchaSettings();
        configuration.GetSection("SmartCaptcha").Bind(settings);

        // Assert
        settings.SecretKey.Should().Be(TestConstants.DefaultSecretKey);
        settings.IsEnabled.Should().BeFalse();
        settings.AuthType.Should().Be(SmartCaptchaAuthType.IamToken);
        settings.IamToken.Should().Be(TestConstants.IamTokenValue);
        settings.TimeoutSeconds.Should().Be(60);
        settings.SkipIpAddresses.Should().Contain("127.0.0.1");
        settings.SkipIpAddresses.Should().Contain("::1");
    }
}
