using System.Reactive.Concurrency;
using Python.Runtime;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedMod
{
    private static readonly EventLoopScheduler Scheduler = new();
    
    private readonly string _modId;
    private readonly string _pyModFile;
    private readonly PyloadedLogger _pyLog;
    private readonly PyModule _module;
    private readonly RxFileWatcher _pyModFileWatcher;
    private readonly PyloadedContext _pyCtx;

    public PyloadedMod(IModLoader modLoader, IReloadedHooks hooks, IScans scans, ILogger log, string modId, string pyModFile, string[] helperScripts)
    {
        _modId = modId;
        _pyModFile = pyModFile;
        _pyLog = new(log, modId);

        _pyModFileWatcher = new(pyModFile, Scheduler);
        _pyModFileWatcher.Changed += _ => ReloadMod();

        _pyCtx = new(modLoader, hooks, scans, modId);
        using (Py.GIL())
        {
            _module = Py.CreateScope();
            _module.Set("Pyloaded", _pyCtx);
            _module.Set("Log", _pyLog);
            _module.Set("print", (object? obj) => _pyLog.Print(obj));

            foreach (var script in helperScripts) _module.Exec(script);
        }
        
        RunMod();
    }

    private void RunMod()
    {
        try
        {
            var pyModSource = File.ReadAllText(_pyModFile);
            using (Py.GIL())
            {
                Log.Debug($"Compiling: {_modId}");
                using var pyModScript = PythonEngine.Compile(pyModSource, _pyModFile);

                Log.Debug($"Running: {_modId}");
                _module.Execute(pyModScript)
                    .Dispose();

                using var pyMod = _module.GetAttr("Mod").Invoke();
                if (!pyMod.IsNone()) Log.Information($"Loaded: {_modId}");
                else Log.Error($"Error: {_modId}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to run mod '{_modId}'.");
        }
    }

    private void ReloadMod()
    {
        Log.Information($"Hot Reload: Reloading mod '{_modId}'...");
        
        Log.Debug("Hot Reload: Disabling existing hooks.");
        _pyCtx.ScanHooks.ClearHooks();
        
        Log.Debug("Hot Reload: Running mod.");
        RunMod();
    }
}
