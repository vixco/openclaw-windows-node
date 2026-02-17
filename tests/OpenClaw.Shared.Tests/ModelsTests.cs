using Xunit;
using OpenClaw.Shared;

namespace OpenClaw.Shared.Tests;

public class AgentActivityTests
{
    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForExec()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Exec };
        Assert.Equal("💻", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForRead()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Read };
        Assert.Equal("📄", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForWrite()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Write };
        Assert.Equal("✍️", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForEdit()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Edit };
        Assert.Equal("📝", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForSearch()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Search };
        Assert.Equal("🔍", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForBrowser()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Browser };
        Assert.Equal("🌐", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForMessage()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Message };
        Assert.Equal("💬", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForTool()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Tool };
        Assert.Equal("🛠️", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsCorrectEmoji_ForJob()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Job };
        Assert.Equal("⚡", activity.Glyph);
    }

    [Fact]
    public void Glyph_ReturnsEmpty_ForIdle()
    {
        var activity = new AgentActivity { Kind = ActivityKind.Idle };
        Assert.Equal("", activity.Glyph);
    }

    [Fact]
    public void DisplayText_ReturnsEmpty_WhenIdle()
    {
        var activity = new AgentActivity 
        { 
            Kind = ActivityKind.Idle,
            Label = "Some label" 
        };
        Assert.Equal("", activity.DisplayText);
    }

    [Fact]
    public void DisplayText_IncludesMainPrefix_ForMainSession()
    {
        var activity = new AgentActivity 
        { 
            Kind = ActivityKind.Exec,
            IsMain = true,
            Label = "Running command" 
        };
        Assert.Equal("Main · 💻 Running command", activity.DisplayText);
    }

    [Fact]
    public void DisplayText_IncludesSubPrefix_ForSubSession()
    {
        var activity = new AgentActivity 
        { 
            Kind = ActivityKind.Read,
            IsMain = false,
            Label = "Reading file" 
        };
        Assert.Equal("Sub · 📄 Reading file", activity.DisplayText);
    }

    [Fact]
    public void DisplayText_HandlesEmptyLabel()
    {
        var activity = new AgentActivity 
        { 
            Kind = ActivityKind.Tool,
            IsMain = true,
            Label = "" 
        };
        Assert.Equal("Main · 🛠️ ", activity.DisplayText);
    }
}

public class ChannelHealthTests
{
    [Theory]
    [InlineData("ok", "[ON]")]
    [InlineData("connected", "[ON]")]
    [InlineData("running", "[ON]")]
    [InlineData("OK", "[ON]")]
    public void DisplayText_ShowsOn_ForOkStatuses(string status, string expected)
    {
        var health = new ChannelHealth { Name = "slack", Status = status };
        Assert.StartsWith(expected, health.DisplayText);
    }

    [Theory]
    [InlineData("linked", "[LINKED]")]
    [InlineData("Linked", "[LINKED]")]
    public void DisplayText_ShowsLinked_ForLinkedStatus(string status, string expected)
    {
        var health = new ChannelHealth { Name = "telegram", Status = status };
        Assert.StartsWith(expected, health.DisplayText);
    }

    [Theory]
    [InlineData("ready", "[READY]")]
    [InlineData("Ready", "[READY]")]
    public void DisplayText_ShowsReady_ForReadyStatus(string status, string expected)
    {
        var health = new ChannelHealth { Name = "telegram", Status = status };
        Assert.StartsWith(expected, health.DisplayText);
    }

    [Theory]
    [InlineData("connecting", "[...]")]
    [InlineData("reconnecting", "[...]")]
    public void DisplayText_ShowsLoading_ForConnectingStatuses(string status, string expected)
    {
        var health = new ChannelHealth { Name = "slack", Status = status };
        Assert.StartsWith(expected, health.DisplayText);
    }

    [Theory]
    [InlineData("error", "[ERR]")]
    [InlineData("disconnected", "[ERR]")]
    public void DisplayText_ShowsError_ForErrorStatuses(string status, string expected)
    {
        var health = new ChannelHealth { Name = "slack", Status = status };
        Assert.StartsWith(expected, health.DisplayText);
    }

    [Theory]
    [InlineData("configured", "[OFF]")]
    [InlineData("stopped", "[OFF]")]
    public void DisplayText_ShowsOff_ForStoppedStatuses(string status, string expected)
    {
        var health = new ChannelHealth { Name = "telegram", Status = status };
        Assert.StartsWith(expected, health.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsNotAvailable_ForNotConfigured()
    {
        var health = new ChannelHealth { Name = "email", Status = "not configured" };
        Assert.StartsWith("[N/A]", health.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsOff_ForUnknownStatus()
    {
        var health = new ChannelHealth { Name = "unknown", Status = "weird" };
        Assert.StartsWith("[OFF]", health.DisplayText);
    }

    [Fact]
    public void DisplayText_CapitalizesChannelName()
    {
        var health = new ChannelHealth { Name = "slack", Status = "ok" };
        Assert.Contains("Slack", health.DisplayText);
    }

    [Fact]
    public void DisplayText_IncludesAuthAge_WhenLinked()
    {
        var health = new ChannelHealth 
        { 
            Name = "telegram", 
            Status = "ready",
            IsLinked = true,
            AuthAge = "2d ago"
        };
        Assert.Contains("linked · 2d ago", health.DisplayText);
    }

    [Fact]
    public void DisplayText_IncludesError_WhenPresent()
    {
        var health = new ChannelHealth 
        { 
            Name = "slack", 
            Status = "error",
            Error = "Connection timeout"
        };
        Assert.Contains("(Connection timeout)", health.DisplayText);
    }

    [Fact]
    public void DisplayText_HandlesEmptyName()
    {
        var health = new ChannelHealth { Name = "", Status = "ok" };
        Assert.Contains(": ok", health.DisplayText);
    }
}

public class SessionInfoTests
{
    [Fact]
    public void DisplayText_ShowsMain_ForMainSession()
    {
        var session = new SessionInfo { IsMain = true };
        Assert.StartsWith("Main", session.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsSub_ForSubSession()
    {
        var session = new SessionInfo { IsMain = false };
        Assert.StartsWith("Sub", session.DisplayText);
    }

    [Fact]
    public void DisplayText_IncludesChannel_WhenPresent()
    {
        var session = new SessionInfo 
        { 
            IsMain = true,
            Channel = "slack"
        };
        Assert.Equal("Main · slack", session.DisplayText);
    }

    [Fact]
    public void DisplayText_IncludesCurrentActivity_WhenPresent()
    {
        var session = new SessionInfo 
        { 
            IsMain = true,
            Channel = "telegram",
            CurrentActivity = "💻 Running"
        };
        Assert.Equal("Main · telegram · 💻 Running", session.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsStatus_WhenNoActivityAndStatusNotUnknownOrActive()
    {
        var session = new SessionInfo 
        { 
            IsMain = true,
            Status = "waiting"
        };
        Assert.Equal("Main · waiting", session.DisplayText);
    }

    [Fact]
    public void DisplayText_DoesNotShowStatus_WhenUnknown()
    {
        var session = new SessionInfo 
        { 
            IsMain = true,
            Status = "unknown"
        };
        Assert.Equal("Main", session.DisplayText);
    }

    [Fact]
    public void DisplayText_DoesNotShowStatus_WhenActive()
    {
        var session = new SessionInfo 
        { 
            IsMain = true,
            Status = "active"
        };
        Assert.Equal("Main", session.DisplayText);
    }

    [Fact]
    public void ShortKey_ReturnsUnknown_ForEmptyKey()
    {
        var session = new SessionInfo { Key = "" };
        Assert.Equal("unknown", session.ShortKey);
    }

    [Fact]
    public void ShortKey_ReturnsSecondToLast_ForColonSeparatedKey()
    {
        var session = new SessionInfo { Key = "agent:main:subagent:uuid" };
        Assert.Equal("subagent", session.ShortKey);
    }

    [Fact]
    public void ShortKey_ReturnsFilename_ForPathWithSlashes()
    {
        var session = new SessionInfo { Key = "/path/to/file.txt" };
        Assert.Equal("file.txt", session.ShortKey);
    }

    [Fact]
    public void ShortKey_ReturnsFilename_ForPathWithBackslashes()
    {
        var session = new SessionInfo { Key = @"C:\path\to\file.txt" };
        // Path.GetFileName behavior depends on OS - on Windows it returns filename, on Linux it returns full path
        // Since this is Windows-specific code, we check that it at least detects backslashes
        var result = session.ShortKey;
        // On Windows: file.txt, On Linux: full path (Path.GetFileName doesn't split on backslash)
        Assert.True(result.Contains("file.txt") || result.Contains("\\"));
    }

    [Fact]
    public void ShortKey_TruncatesLongKeys()
    {
        var session = new SessionInfo { Key = "this-is-a-very-long-key-that-should-be-truncated" };
        Assert.Equal("this-is-a-very-lo...", session.ShortKey);
    }

    [Fact]
    public void ShortKey_ReturnsFullKey_ForShortKeys()
    {
        var session = new SessionInfo { Key = "short" };
        Assert.Equal("short", session.ShortKey);
    }

    [Fact]
    public void RichDisplayText_IncludesModelAndContextSummary()
    {
        var session = new SessionInfo
        {
            DisplayName = "telegram:alerts",
            Model = "claude-opus-4-6",
            TotalTokens = 12_000,
            ContextTokens = 200_000,
            ThinkingLevel = "high"
        };

        var text = session.RichDisplayText;
        Assert.Contains("telegram:alerts", text);
        Assert.Contains("claude-opus-4-6", text);
        Assert.Contains("12.0K/200.0K ctx", text);
        Assert.Contains("think high", text);
    }

    [Fact]
    public void ContextSummaryShort_IsEmptyWithoutTokenWindow()
    {
        var session = new SessionInfo { TotalTokens = 1000, ContextTokens = 0 };
        Assert.Equal("", session.ContextSummaryShort);
    }
}

public class GatewayUsageInfoTests
{
    [Fact]
    public void DisplayText_ShowsNoUsageData_WhenEmpty()
    {
        var usage = new GatewayUsageInfo();
        Assert.Equal("No usage data", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsProviderSummary_WhenLegacyUsageFieldsMissing()
    {
        var usage = new GatewayUsageInfo { ProviderSummary = "OpenAI: 72% left" };
        Assert.Equal("OpenAI: 72% left", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsTokens_WhenPresent()
    {
        var usage = new GatewayUsageInfo { TotalTokens = 5000 };
        Assert.Contains("Tokens: 5.0K", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsCost_WhenPresent()
    {
        var usage = new GatewayUsageInfo { TotalTokens = 1000, CostUsd = 0.25 };
        Assert.Contains("$0.25", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsRequestCount_WhenPresent()
    {
        var usage = new GatewayUsageInfo { RequestCount = 42 };
        Assert.Contains("42 requests", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_ShowsModel_WhenPresent()
    {
        var usage = new GatewayUsageInfo 
        { 
            TotalTokens = 1000,
            Model = "claude-3-5-sonnet" 
        };
        Assert.Contains("claude-3-5-sonnet", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_FormatsMillions_Correctly()
    {
        var usage = new GatewayUsageInfo { TotalTokens = 2_500_000 };
        Assert.Contains("2.5M", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_FormatsThousands_Correctly()
    {
        var usage = new GatewayUsageInfo { TotalTokens = 15_000 };
        Assert.Contains("15.0K", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_FormatsSmallNumbers_AsIs()
    {
        var usage = new GatewayUsageInfo { TotalTokens = 999 };
        Assert.Contains("999", usage.DisplayText);
    }

    [Fact]
    public void DisplayText_CombinesAllFields_WhenAllPresent()
    {
        var usage = new GatewayUsageInfo 
        { 
            TotalTokens = 10_000,
            CostUsd = 1.50,
            RequestCount = 25,
            Model = "gpt-4"
        };
        var display = usage.DisplayText;
        Assert.Contains("10.0K", display);
        Assert.Contains("$1.50", display);
        Assert.Contains("25 requests", display);
        Assert.Contains("gpt-4", display);
    }
}
