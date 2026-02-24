using OpenClaw.Shared;
using Xunit;

namespace OpenClaw.Shared.Tests;

public class GatewayUrlHelperTests
{
    [Theory]
    [InlineData("ws://localhost:18789", "ws://localhost:18789")]
    [InlineData("wss://host.tailnet.ts.net", "wss://host.tailnet.ts.net")]
    [InlineData("http://localhost:18789", "ws://localhost:18789")]
    [InlineData("https://host.tailnet.ts.net", "wss://host.tailnet.ts.net")]
    [InlineData("HTTP://LOCALHOST:18789", "ws://LOCALHOST:18789")]
    [InlineData("HTTPS://HOST.EXAMPLE.COM", "wss://HOST.EXAMPLE.COM")]
    public void TryNormalizeWebSocketUrl_NormalizesSupportedSchemes(string inputUrl, string expected)
    {
        var result = GatewayUrlHelper.TryNormalizeWebSocketUrl(inputUrl, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("localhost:18789")]
    [InlineData("ftp://example.com")]
    [InlineData("file://localhost/c$/temp")]
    public void TryNormalizeWebSocketUrl_RejectsInvalidOrUnsupportedUrls(string inputUrl)
    {
        var result = GatewayUrlHelper.TryNormalizeWebSocketUrl(inputUrl, out var normalized);

        Assert.False(result);
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void TryNormalizeWebSocketUrl_RejectsNullInput()
    {
        var result = GatewayUrlHelper.TryNormalizeWebSocketUrl(null, out var normalized);

        Assert.False(result);
        Assert.Equal(string.Empty, normalized);
    }

    [Theory]
    [InlineData("  ws://localhost:18789  ", "ws://localhost:18789")]
    [InlineData("  http://localhost:18789  ", "ws://localhost:18789")]
    public void TryNormalizeWebSocketUrl_TrimsWhitespace(string inputUrl, string expected)
    {
        var result = GatewayUrlHelper.TryNormalizeWebSocketUrl(inputUrl, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("wss://user:pass@host.example.com", "wss://user:pass@host.example.com")]
    [InlineData("https://user:pass@host.example.com", "wss://user:pass@host.example.com")]
    public void TryNormalizeWebSocketUrl_PreservesEmbeddedCredentials(string inputUrl, string expected)
    {
        var result = GatewayUrlHelper.TryNormalizeWebSocketUrl(inputUrl, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    // --- IsValidGatewayUrl ---

    [Theory]
    [InlineData("ws://localhost:18789")]
    [InlineData("wss://host.tailnet.ts.net")]
    [InlineData("http://localhost:18789")]
    [InlineData("https://host.tailnet.ts.net")]
    public void IsValidGatewayUrl_ReturnsTrueForValidUrls(string url)
    {
        Assert.True(GatewayUrlHelper.IsValidGatewayUrl(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("localhost:18789")]
    [InlineData("ftp://example.com")]
    public void IsValidGatewayUrl_ReturnsFalseForInvalidUrls(string? url)
    {
        Assert.False(GatewayUrlHelper.IsValidGatewayUrl(url));
    }

    // --- NormalizeForWebSocket ---

    [Theory]
    [InlineData("http://localhost:18789", "ws://localhost:18789")]
    [InlineData("https://host.tailnet.ts.net", "wss://host.tailnet.ts.net")]
    [InlineData("ws://localhost:18789", "ws://localhost:18789")]
    [InlineData("wss://host.tailnet.ts.net", "wss://host.tailnet.ts.net")]
    public void NormalizeForWebSocket_NormalizesHttpToWs(string inputUrl, string expected)
    {
        var result = GatewayUrlHelper.NormalizeForWebSocket(inputUrl);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeForWebSocket_ReturnsEmptyString_ForNull()
    {
        var result = GatewayUrlHelper.NormalizeForWebSocket(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeForWebSocket_ReturnsTrimmedOriginal_ForInvalidUrl()
    {
        // Invalid URL returns trimmed original (fallback behavior)
        var result = GatewayUrlHelper.NormalizeForWebSocket("  not-a-url  ");
        Assert.Equal("not-a-url", result);
    }

    [Fact]
    public void ValidationMessage_IsNotEmpty()
    {
        Assert.NotEmpty(GatewayUrlHelper.ValidationMessage);
        Assert.Contains("ws://", GatewayUrlHelper.ValidationMessage);
    }
}
