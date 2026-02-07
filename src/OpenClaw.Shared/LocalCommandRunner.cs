using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenClaw.Shared;

/// <summary>
/// Executes commands locally via Process.Start (pwsh.exe / cmd.exe).
/// This is the default runner. Swap with DockerCommandRunner, WslCommandRunner, etc.
/// </summary>
public class LocalCommandRunner : ICommandRunner
{
    private readonly IOpenClawLogger _logger;
    
    public string Name => "local";
    
    public LocalCommandRunner(IOpenClawLogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }
    
    public async Task<CommandResult> RunAsync(CommandRequest request, CancellationToken ct = default)
    {
        var (fileName, arguments) = BuildProcessArgs(request);
        
        _logger.Info($"[EXEC] {fileName} {arguments}");
        
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        
        if (!string.IsNullOrEmpty(request.Cwd))
        {
            psi.WorkingDirectory = request.Cwd;
        }
        
        if (request.Env != null)
        {
            foreach (var (key, value) in request.Env)
            {
                psi.Environment[key] = value;
            }
        }
        
        var sw = Stopwatch.StartNew();
        using var process = new Process { StartInfo = psi };
        
        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();
        
        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdoutBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderrBuilder.AppendLine(e.Data); };
        
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            _logger.Error($"[EXEC] Failed to start process: {ex.Message}");
            return new CommandResult
            {
                Stderr = $"Failed to start: {ex.Message}",
                ExitCode = -1,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        
        var timedOut = false;
        
        try
        {
            if (request.TimeoutMs > 0)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(request.TimeoutMs);
                
                try
                {
                    await process.WaitForExitAsync(timeoutCts.Token);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    timedOut = true;
                    _logger.Warn($"[EXEC] Process timed out after {request.TimeoutMs}ms");
                    KillProcess(process);
                }
            }
            else
            {
                await process.WaitForExitAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            KillProcess(process);
            throw;
        }
        
        sw.Stop();
        
        var result = new CommandResult
        {
            Stdout = stdoutBuilder.ToString().TrimEnd(),
            Stderr = stderrBuilder.ToString().TrimEnd(),
            ExitCode = timedOut ? -1 : process.ExitCode,
            TimedOut = timedOut,
            DurationMs = sw.ElapsedMilliseconds
        };
        
        _logger.Info($"[EXEC] Exit={result.ExitCode} Duration={result.DurationMs}ms TimedOut={timedOut} Stdout={result.Stdout.Length}chars Stderr={result.Stderr.Length}chars");
        
        return result;
    }
    
    private static (string fileName, string arguments) BuildProcessArgs(CommandRequest request)
    {
        var shell = (request.Shell ?? "powershell").ToLowerInvariant();
        var command = request.Command;
        
        if (request.Args is { Length: > 0 })
        {
            command = command + " " + string.Join(" ", request.Args);
        }
        
        return shell switch
        {
            "cmd" => ("cmd.exe", $"/C {command}"),
            "pwsh" => ("pwsh.exe", $"-NoProfile -NonInteractive -Command {command}"),
            _ => ("powershell.exe", $"-NoProfile -NonInteractive -Command {command}")
        };
    }
    
    private void KillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex)
        {
            _logger.Warn($"[EXEC] Failed to kill process: {ex.Message}");
        }
    }
}
