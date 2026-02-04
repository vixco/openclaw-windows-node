using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenClaw.Shared.Capabilities;

/// <summary>
/// System capability - notifications, exec (future), etc.
/// </summary>
public class SystemCapability : NodeCapabilityBase
{
    public override string Category => "system";
    
    private static readonly string[] _commands = new[]
    {
        "system.notify"
        // Future: "system.run", "system.execApprovals.get", "system.execApprovals.set"
    };
    
    public override IReadOnlyList<string> Commands => _commands;
    
    // Event to let UI handle the actual notification display
    public event EventHandler<SystemNotifyArgs>? NotifyRequested;
    
    public SystemCapability(IOpenClawLogger logger) : base(logger)
    {
    }
    
    public override async Task<NodeInvokeResponse> ExecuteAsync(NodeInvokeRequest request)
    {
        return request.Command switch
        {
            "system.notify" => await HandleNotifyAsync(request),
            _ => Error($"Unknown command: {request.Command}")
        };
    }
    
    private Task<NodeInvokeResponse> HandleNotifyAsync(NodeInvokeRequest request)
    {
        var title = GetStringArg(request.Args, "title", "OpenClaw");
        var body = GetStringArg(request.Args, "body", "");
        var subtitle = GetStringArg(request.Args, "subtitle");
        var sound = GetBoolArg(request.Args, "sound", true);
        
        Logger.Info($"system.notify: {title} - {body}");
        
        // Raise event for UI to handle
        NotifyRequested?.Invoke(this, new SystemNotifyArgs
        {
            Title = title ?? "OpenClaw",
            Body = body ?? "",
            Subtitle = subtitle,
            PlaySound = sound
        });
        
        return Task.FromResult(Success(new { sent = true }));
    }
}

public class SystemNotifyArgs : EventArgs
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string? Subtitle { get; set; }
    public bool PlaySound { get; set; } = true;
}
