using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sigscan;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

/// <summary>
/// Pyloaded <see cref="IScans"/> implementation which swaps to manual scanner after init,
/// allowing for real-time scans and hooks with Hot Reload.
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class PyloadedScans : IScans
{
    private static readonly Dictionary<string, nint> CachedResults = [];
    
    private readonly IReloadedHooks _hooks;
    private readonly Scanner _scanner;
    private bool _isPostInit;

    public PyloadedScans(IModLoader modLoader, IReloadedHooks hooks)
    {
        _hooks = hooks;
        
        var proc = Process.GetCurrentProcess();
        _scanner = new(proc, proc.MainModule);

        modLoader.OnModLoaderInitialized += () => _isPostInit = true;
    }

    public void AddScan(string id, string? pattern, Action<nint> onSuccess, Action? onFail = null)
    {
        if (string.IsNullOrEmpty(pattern)) return;
        
        if (!_isPostInit)
        {
            Project.Scans.AddScan(id, pattern, result =>
            {
                CachedResults[pattern] = result;
                onSuccess(result);
            }, onFail);
            return;
        }

        if (TryGetScanOrCachedResult(pattern, out var localResult))
        {
            Log.Information($"'{id}' found at: 0x{localResult:X}");
            onSuccess(localResult);
        }
        else if (onFail != null)
        {
            onFail();
        }
        else
        {
            Log.Error($"Failed to find pattern for '{id}'.\nPattern: {pattern}");
        }
    }

    public void AddScanHook(string id, string? pattern, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
    {
        if (string.IsNullOrEmpty(pattern)) return;
        
        if (!_isPostInit)
        {
            Project.Scans.AddScanHook(id, pattern, (results, hooks) =>
            {
                CachedResults[pattern] = results;
                onSuccess(results, hooks);
            }, onFail);
            return;
        }
        
        AddScan(id, pattern, result => onSuccess(result, _hooks), onFail);
    }

    public void AddScan(string id, nint defaultResult, Action<nint> onSuccess, Action? onFail = null)
    {
        if (!_isPostInit)
        {
            Project.Scans.AddScan(id, defaultResult, onSuccess, onFail);
            return;
        }
        
        onSuccess(defaultResult);
    }

    public void AddScanHook(string id, nint defaultResult, Action<nint, IReloadedHooks> onSuccess,
        Action? onFail = null)
    {
        if (!_isPostInit)
        {
            Project.Scans.AddScanHook(id, defaultResult, onSuccess, onFail);
            return;
        }
        
        onSuccess(defaultResult, _hooks);
    }

    private bool TryGetScanOrCachedResult(string pattern, out nint result)
    {
        if (CachedResults.TryGetValue(pattern, out result)) return true;
        
        var ofs = _scanner.FindPattern(pattern);
        if (!ofs.Found) return false;
        
        result = Utilities.BaseAddress + ofs.Offset;
        CachedResults[pattern] = result;
        return true;
    }

    #region Not suitable for hot reload.
    
    public void AddScan(string id, Action<nint> onSuccess, Action? onFail = null)
        => throw new NotImplementedException("Patternless and result-less scans is unsupported.");

    public void AddScanHook(string id, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => throw new NotImplementedException("Patternless and result-less scans is unsupported.");

    public void AddListener(string id, Action<nint> onSuccess, Action? onFail = null) => AddScan(id, onSuccess, onFail);

    #endregion
}