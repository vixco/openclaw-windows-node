using System.Text.Json;
using Xunit;
using OpenClaw.Shared;
using OpenClaw.Shared.Capabilities;

namespace OpenClaw.Shared.Tests;

/// <summary>
/// Unit tests for each capability: SystemCapability, CanvasCapability,
/// ScreenCapability, CameraCapability.
/// Tests execute logic, arg parsing, event raising, and error paths.
/// No hardware or UI dependencies.
/// </summary>
public class SystemCapabilityTests
{
    private static JsonElement Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    [Fact]
    public void CanHandle_SystemNotify()
    {
        var cap = new SystemCapability(NullLogger.Instance);
        Assert.True(cap.CanHandle("system.notify"));
        Assert.False(cap.CanHandle("system.run"));
        Assert.Equal("system", cap.Category);
    }

    [Fact]
    public async Task Notify_RaisesEvent_WithArgs()
    {
        var cap = new SystemCapability(NullLogger.Instance);
        SystemNotifyArgs? received = null;
        cap.NotifyRequested += (s, a) => received = a;

        var req = new NodeInvokeRequest
        {
            Id = "n1",
            Command = "system.notify",
            Args = Parse("""{"title":"Hello","body":"World","sound":false}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.NotNull(received);
        Assert.Equal("Hello", received!.Title);
        Assert.Equal("World", received.Body);
        Assert.False(received.PlaySound);
    }

    [Fact]
    public async Task Notify_DefaultsTitle_WhenMissing()
    {
        var cap = new SystemCapability(NullLogger.Instance);
        SystemNotifyArgs? received = null;
        cap.NotifyRequested += (s, a) => received = a;

        var req = new NodeInvokeRequest
        {
            Id = "n2",
            Command = "system.notify",
            Args = Parse("""{"body":"Just body"}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.Equal("OpenClaw", received!.Title);
    }

    [Fact]
    public async Task UnknownCommand_ReturnsError()
    {
        var cap = new SystemCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest
        {
            Id = "n3",
            Command = "system.unknown",
            Args = Parse("""{}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("Unknown command", res.Error);
    }
}

public class CanvasCapabilityTests
{
    private static JsonElement Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    [Fact]
    public void CanHandle_AllCanvasCommands()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        Assert.True(cap.CanHandle("canvas.present"));
        Assert.True(cap.CanHandle("canvas.hide"));
        Assert.True(cap.CanHandle("canvas.navigate"));
        Assert.True(cap.CanHandle("canvas.eval"));
        Assert.True(cap.CanHandle("canvas.snapshot"));
        Assert.True(cap.CanHandle("canvas.a2ui.push"));
        Assert.True(cap.CanHandle("canvas.a2ui.reset"));
        Assert.False(cap.CanHandle("canvas.unknown"));
        Assert.Equal("canvas", cap.Category);
    }

    [Fact]
    public async Task Present_RaisesEvent_WithArgs()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        CanvasPresentArgs? received = null;
        cap.PresentRequested += (s, a) => received = a;

        var req = new NodeInvokeRequest
        {
            Id = "c1",
            Command = "canvas.present",
            Args = Parse("""{"url":"https://example.com","width":1024,"height":768,"title":"Test","alwaysOnTop":true}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.NotNull(received);
        Assert.Equal("https://example.com", received!.Url);
        Assert.Equal(1024, received.Width);
        Assert.Equal(768, received.Height);
        Assert.Equal("Test", received.Title);
        Assert.True(received.AlwaysOnTop);
    }

    [Fact]
    public async Task Present_UsesDefaults_WhenArgsMissing()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        CanvasPresentArgs? received = null;
        cap.PresentRequested += (s, a) => received = a;

        var req = new NodeInvokeRequest
        {
            Id = "c2",
            Command = "canvas.present",
            Args = Parse("""{}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.Equal(800, received!.Width);
        Assert.Equal(600, received.Height);
        Assert.Equal("Canvas", received.Title);
        Assert.False(received.AlwaysOnTop);
    }

    [Fact]
    public async Task Hide_RaisesEvent()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        bool hideRaised = false;
        cap.HideRequested += (s, e) => hideRaised = true;

        var req = new NodeInvokeRequest { Id = "c3", Command = "canvas.hide", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.True(hideRaised);
    }

    [Fact]
    public async Task Navigate_ReturnsError_WhenUrlMissing()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "c4", Command = "canvas.navigate", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("url", res.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Eval_AcceptsJavaScriptParam()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        string? evaledScript = null;
        cap.EvalRequested += (script) =>
        {
            evaledScript = script;
            return Task.FromResult("42");
        };

        var req = new NodeInvokeRequest
        {
            Id = "c5",
            Command = "canvas.eval",
            Args = Parse("""{"javaScript":"document.title"}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.Equal("document.title", evaledScript);
    }

    [Fact]
    public async Task Eval_ReturnsError_WhenNoScript()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "c6", Command = "canvas.eval", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("script", res.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Eval_ReturnsError_WhenNoHandler()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest
        {
            Id = "c7",
            Command = "canvas.eval",
            Args = Parse("""{"script":"test"}""")
        };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("not available", res.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Snapshot_ReturnsError_WhenNoHandler()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "c8", Command = "canvas.snapshot", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("not available", res.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A2UIPush_ReturnsError_WhenNoJsonl()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "c9", Command = "canvas.a2ui.push", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("jsonl", res.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A2UIPush_RaisesEvent_WithJsonl()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        CanvasA2UIArgs? received = null;
        cap.A2UIPushRequested += (s, a) => received = a;

        var req = new NodeInvokeRequest
        {
            Id = "c10",
            Command = "canvas.a2ui.push",
            Args = Parse("""{"jsonl":"{\"type\":\"text\"}"}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.NotNull(received);
        Assert.Contains("text", received!.Jsonl);
    }

    [Fact]
    public async Task A2UIReset_RaisesEvent()
    {
        var cap = new CanvasCapability(NullLogger.Instance);
        bool resetRaised = false;
        cap.A2UIResetRequested += (s, e) => resetRaised = true;

        var req = new NodeInvokeRequest { Id = "c11", Command = "canvas.a2ui.reset", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.True(resetRaised);
    }
}

public class ScreenCapabilityTests
{
    private static JsonElement Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    [Fact]
    public void CanHandle_ScreenCommands()
    {
        var cap = new ScreenCapability(NullLogger.Instance);
        Assert.True(cap.CanHandle("screen.capture"));
        Assert.True(cap.CanHandle("screen.list"));
        Assert.False(cap.CanHandle("screen.record"));
        Assert.Equal("screen", cap.Category);
    }

    [Fact]
    public async Task Capture_ReturnsError_WhenNoHandler()
    {
        var cap = new ScreenCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "s1", Command = "screen.capture", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("not available", res.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Capture_CallsHandler_WithArgs()
    {
        var cap = new ScreenCapability(NullLogger.Instance);
        ScreenCaptureArgs? receivedArgs = null;
        cap.CaptureRequested += (args) =>
        {
            receivedArgs = args;
            return Task.FromResult(new ScreenCaptureResult { Format = "png", Width = 1920, Height = 1080, Base64 = "abc" });
        };

        var req = new NodeInvokeRequest
        {
            Id = "s2",
            Command = "screen.capture",
            Args = Parse("""{"format":"jpeg","maxWidth":800,"quality":50,"screenIndex":1}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.NotNull(receivedArgs);
        Assert.Equal("jpeg", receivedArgs!.Format);
        Assert.Equal(800, receivedArgs.MaxWidth);
        Assert.Equal(50, receivedArgs.Quality);
        Assert.Equal(1, receivedArgs.MonitorIndex);
    }

    [Fact]
    public async Task List_ReturnsError_WhenNoHandler()
    {
        var cap = new ScreenCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "s3", Command = "screen.list", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
    }

    [Fact]
    public async Task List_ReturnsScreens_WhenHandler()
    {
        var cap = new ScreenCapability(NullLogger.Instance);
        cap.ListRequested += () => Task.FromResult(new[] 
        { 
            new ScreenInfo { Index = 0, Name = "Main", IsPrimary = true, Width = 2560, Height = 1440 } 
        });

        var req = new NodeInvokeRequest { Id = "s4", Command = "screen.list", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.NotNull(res.Payload);
    }
}

public class CameraCapabilityTests
{
    private static JsonElement Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    [Fact]
    public void CanHandle_CameraCommands()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        Assert.True(cap.CanHandle("camera.list"));
        Assert.True(cap.CanHandle("camera.snap"));
        Assert.False(cap.CanHandle("camera.clip"));
        Assert.Equal("camera", cap.Category);
    }

    [Fact]
    public async Task List_ReturnsError_WhenNoHandler()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "cam1", Command = "camera.list", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
    }

    [Fact]
    public async Task List_ReturnsCameras_WhenHandler()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        cap.ListRequested += () => Task.FromResult(new[]
        {
            new CameraInfo { DeviceId = "cam-1", Name = "Front", IsDefault = true },
            new CameraInfo { DeviceId = "cam-2", Name = "Back", IsDefault = false }
        });

        var req = new NodeInvokeRequest { Id = "cam2", Command = "camera.list", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
    }

    [Fact]
    public async Task Snap_ReturnsError_WhenNoHandler()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        var req = new NodeInvokeRequest { Id = "cam3", Command = "camera.snap", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
    }

    [Fact]
    public async Task Snap_CallsHandler_WithArgs()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        CameraSnapArgs? receivedArgs = null;
        cap.SnapRequested += (args) =>
        {
            receivedArgs = args;
            return Task.FromResult(new CameraSnapResult { Format = "jpeg", Width = 640, Height = 480, Base64 = "img" });
        };

        var req = new NodeInvokeRequest
        {
            Id = "cam4",
            Command = "camera.snap",
            Args = Parse("""{"deviceId":"cam-1","format":"png","maxWidth":320,"quality":50}""")
        };

        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.NotNull(receivedArgs);
        Assert.Equal("cam-1", receivedArgs!.DeviceId);
        Assert.Equal("png", receivedArgs.Format);
        Assert.Equal(320, receivedArgs.MaxWidth);
        Assert.Equal(50, receivedArgs.Quality);
    }

    [Fact]
    public async Task Snap_UsesDefaults_WhenArgsMissing()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        CameraSnapArgs? receivedArgs = null;
        cap.SnapRequested += (args) =>
        {
            receivedArgs = args;
            return Task.FromResult(new CameraSnapResult { Format = "jpeg", Width = 640, Height = 480, Base64 = "img" });
        };

        var req = new NodeInvokeRequest { Id = "cam5", Command = "camera.snap", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.True(res.Ok);
        Assert.Null(receivedArgs!.DeviceId);
        Assert.Equal("jpeg", receivedArgs.Format);
        Assert.Equal(1280, receivedArgs.MaxWidth);
        Assert.Equal(80, receivedArgs.Quality);
    }

    [Fact]
    public async Task Snap_ReturnsError_WhenHandlerThrows()
    {
        var cap = new CameraCapability(NullLogger.Instance);
        cap.SnapRequested += (args) => throw new InvalidOperationException("Camera access blocked");

        var req = new NodeInvokeRequest { Id = "cam6", Command = "camera.snap", Args = Parse("""{}""") };
        var res = await cap.ExecuteAsync(req);
        Assert.False(res.Ok);
        Assert.Contains("Camera access blocked", res.Error);
    }
}
