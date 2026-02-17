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
}
