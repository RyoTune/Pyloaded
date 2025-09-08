using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Reloaded.Mod.Interfaces;

namespace Pyloaded.Reloaded.Python;

public class PyloadedLogger(ILogger log, string name)
{
    private static readonly Dictionary<LogLevel, string> LevelPrefixes = new()
    {
        [LogLevel.Verbose] = "[VRB]",
        [LogLevel.Debug] = "[DBG]",
        [LogLevel.Information] = "[INF]",
        [LogLevel.Warning] = "[WRN]",
        [LogLevel.Error] = "[ERR]"
    };
    
    private readonly Dictionary<LogLevel, Color> _levelColors = new()
    {
        [LogLevel.Verbose] = Color.White,
        [LogLevel.Debug] = Color.LightGreen,
        [LogLevel.Information] = GetColor(name),
        [LogLevel.Warning] = Color.LightGoldenrodYellow,
        [LogLevel.Error] = Color.Red
    };

    public void SetColor(string hexColor) => _levelColors[LogLevel.Information] = ColorTranslator.FromHtml(hexColor);

    public void Print(object? obj) => Information($"{obj}");
    
    public void Verbose(string message)
    {
        if (Log.LogLevel >= LogLevel.Debug) return;
        LogMessage(LogLevel.Verbose, message);
    }
    
    public void Debug(string message)
    {
        if (Log.LogLevel >= LogLevel.Information) return;
        LogMessage(LogLevel.Debug, message);
    }
    
    public void Information(string message)
    {
        if (Log.LogLevel >= LogLevel.Warning) return;
        LogMessage(LogLevel.Information, message);
    }
    
    public void Warning(string message)
    {
        if (Log.LogLevel >= LogLevel.Error) return;
        LogMessage(LogLevel.Warning, message);
    }
    
    public void Error(string message)
    {
        if (Log.LogLevel > LogLevel.Error) return;
        LogMessage(LogLevel.Error, message);
    }

    private void LogMessage(LogLevel level, string message)
    {
        log.WriteLine(FormatMessage(level, message), _levelColors[level]);
    }
    
    private string FormatMessage(LogLevel level, string message) => $"[{name}] {LevelPrefixes[level]} {message}";
    
    private static Color GetColor(string str)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(str));
        var bytes = BitConverter.GetBytes(BitConverter.ToUInt32(hash));
        var color = WithMinBrightness(Color.FromArgb(0xFF, bytes[0], bytes[1], bytes[2]), 0.85);
        return color;
    }

    private static Color WithMinBrightness(Color color, double minLum)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var lum = r * 0.2126 + g * 0.7125 + b * 0.0722;

        if (lum >= minLum)
        {
            return color;
        }

        var maxLum = 0.95;
        var diff = minLum / lum;
        var newR = Math.Min(r * diff * 255, 255 * maxLum);
        var newG = Math.Min(g * diff * 255, 255 * maxLum);
        var newB = Math.Min(b * diff * 255, 255 * maxLum);
        return Color.FromArgb(0xFF, (byte)newR, (byte)newG, (byte)newB);
    }
}