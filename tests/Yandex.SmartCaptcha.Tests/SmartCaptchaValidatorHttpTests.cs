using FluentAssertions;
using Moq;
using Xunit;

namespace Yandex.SmartCaptcha.Tests;

public class SmartCaptchaValidatorHttpTests : SmartCaptchaTestsBase
{
    [Fact]
    public async Task ValidateAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        Mock<HttpMessageHandler> mockHandler = CreateSuccessfulHttpMock();
        var httpClient1 = new HttpClient(mockHandler.Object);
        using HttpClient httpClient = httpClient1;
        SmartCaptchaValidator validator = CreateValidatorWithHttpClient(httpClient);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken, TestConstants.DefaultIp);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Status.Should().Be("ok");
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        Mock<HttpMessageHandler> mockHandler = CreateFailureHttpMock();
        using var httpClient = new HttpClient(mockHandler.Object);
        SmartCaptchaValidator validator = CreateValidatorWithHttpClient(httpClient);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.InvalidToken, TestConstants.DefaultIp);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid token");
        result.Status.Should().Be("failed");
    }

    [Fact]
    public async Task ValidateAsync_WithHttpError_ReturnsFailure()
    {
        // Arrange
        Mock<HttpMessageHandler> mockHandler = CreateErrorHttpMock();
        using var httpClient = new HttpClient(mockHandler.Object);
        SmartCaptchaValidator validator = CreateValidatorWithHttpClient(httpClient);

        // Act
        SmartCaptchaValidationResult result = await validator.ValidateAsync(TestConstants.DefaultToken, TestConstants.DefaultIp);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Сервис валидации каптчи недоступен");
    }

    [Theory]
    [InlineData(SmartCaptchaAuthType.SecretKey, TestConstants.DefaultSecretKey)]
    [InlineData(SmartCaptchaAuthType.IamToken, TestConstants.IamTokenValue)]
    [InlineData(SmartCaptchaAuthType.ApiKey, TestConstants.ApiKeyValue)]
    public async Task ValidateAsync_WithDifferentAuthTypes_SendsCorrectRequest(SmartCaptchaAuthType authType, string keyValue)
    {
        // Arrange
        (Mock<HttpMessageHandler>? mockHandler, List<HttpRequestMessage>? capturedRequests) = CreateCapturingHttpMock();
        SmartCaptchaSettings settings = CreateAuthSettings(authType, keyValue, isEnabled: true);

        using var httpClient = new HttpClient(mockHandler.Object);
        SmartCaptchaValidator validator = CreateValidatorWithHttpClient(httpClient, settings);

        // Act
        await validator.ValidateAsync(TestConstants.DefaultToken, TestConstants.DefaultIp);

        // Assert
        capturedRequests.Should().HaveCount(1);
        HttpRequestMessage capturedRequest = capturedRequests[0];

        switch (authType)
        {
            case SmartCaptchaAuthType.SecretKey:
                capturedRequest.Content.Should().BeOfType<FormUrlEncodedContent>();
                break;
            case SmartCaptchaAuthType.IamToken:
                capturedRequest.Headers.Authorization?.Scheme.Should().Be("Bearer");
                capturedRequest.Headers.Authorization?.Parameter.Should().Be(keyValue);
                break;
            case SmartCaptchaAuthType.ApiKey:
                capturedRequest.Headers.Authorization?.Scheme.Should().Be("Api-Key");
                capturedRequest.Headers.Authorization?.Parameter.Should().Be(keyValue);
                break;
        }
    }
}
