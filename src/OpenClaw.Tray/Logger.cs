using System;
using System.Diagnostics;
using System.IO;
using OpenClaw.Shared;

namespace OpenClawTray;

/// <summary>
/// Simple file + debug logger for troubleshooting.
/// Writes to %LOCALAPPDATA%\OpenClawTray\openclaw-tray.log
/// </summary>
public static class Logger
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenClawTray");
    private static readonly string LogPath = Path.Combine(LogDir, "openclaw-tray.log");
    private static readonly object Lock = new();
    private static bool _initialized;
    private static StreamWriter? _writer;
    
    /// <summary>Get a logger instance that implements IOpenClawLogger for the shared library.</summary>
    public static IOpenClawLogger Instance { get; } = new LoggerAdapter();

    public static void Info(string message) => Write("INFO", message);
    public static void Debug(string message) => Write("DEBUG", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);
    public static void Error(string message, Exception ex) => Write("ERROR", $"{message}: {ex.Message}\n  Stack: {ex.StackTrace}");

    /// <summary>Flush and close the log file (call on app exit).</summary>
    public static void Shutdown()
    {
        lock (Lock)
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
            _initialized = false;
        }
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        try
        {
            Directory.CreateDirectory(LogDir);
            RotateIfNeeded();
            _writer = new StreamWriter(LogPath, append: true) { AutoFlush = true };
            _initialized = true;
        }
        catch
        {
            // Can't init — fall back to Debug.WriteLine only
        }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            var info = new FileInfo(LogPath);
            if (info.Exists && info.Length > 1_048_576)
            {
                var backup = LogPath + ".1";
                if (File.Exists(backup)) File.Delete(backup);
                File.Move(LogPath, backup);
            }
        }
        catch { }
    }

    private static void Write(string level, string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        System.Diagnostics.Debug.WriteLine(line);

        try
        {
            lock (Lock)
            {
                EnsureInitialized();
                _writer?.WriteLine(line);
            }
        }
        catch
        {
            // Don't crash if we can't write logs
        }
    }
    
    /// <summary>Adapter to make the static Logger work with IOpenClawLogger interface.</summary>
    private class LoggerAdapter : IOpenClawLogger
    {
        public void Info(string message) => Logger.Info(message);
        public void Debug(string message) => Logger.Debug(message);
        public void Warn(string message) => Logger.Warn(message);
        public void Error(string message, Exception? ex = null)
        {
            if (ex != null)
                Logger.Error(message, ex);
            else
                Logger.Error(message);
        }
    }
}


