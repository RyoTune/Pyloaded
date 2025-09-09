using System.Reactive.Concurrency;
using Python.Runtime;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedMod
{
    private static readonly EventLoopScheduler Scheduler = new();
    private static PyObject? _pyloadedTypes;
    
    private readonly string _name;
    private readonly string _pyModFile;
    private readonly PyloadedLogger _pyLog;
    private readonly PyModule _module;
    private readonly RxFileWatcher _pyModFileWatcher;
    private readonly PyloadedContext _pyCtx;

    public PyloadedMod(IModLoader modLoader, IScans scans, ILogger log, string name, string pyModFile)
    {
        _name = name;
        _pyModFile = pyModFile;
        _pyLog = new(log, name);

        _pyModFileWatcher = new(pyModFile, Scheduler);
        _pyModFileWatcher.Changed += _ => ReloadMod();

        _pyCtx = new(modLoader, scans);
        using (Py.GIL())
        {
            _module = Py.CreateScope();
            _module.Set("Pyloaded", _pyCtx);
            _module.Set("Log", _pyLog);
            _module.Set("print", (object? obj) => _pyLog.Print(obj));

            if (_pyloadedTypes == null)
                _pyloadedTypes = PyModule.Import("pyloadedtypes");
            
            _module.ImportAll(_pyloadedTypes.GetAttr("types_dict").As<PyDict>());
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
                Log.Debug($"Compiling: {_name}");
                using var pyModScript = PythonEngine.Compile(pyModSource, _pyModFile);

                Log.Debug($"Running: {_name}");
                _module.Execute(pyModScript)
                    .Dispose();

                using var pyMod = _module.GetAttr("Mod").Invoke();
                if (!pyMod.IsNone()) Log.Information($"Loaded: {_name}");
                else Log.Error($"Error: {_name}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to run mod '{_name}'.");
        }
    }

    private void ReloadMod()
    {
        Log.Information($"Hot Reload: Reloading mod '{_name}'...");
        
        Log.Debug("Hot Reload: Disabling existing hooks.");
        _pyCtx.ScanHooks.ClearHooks();
        
        Log.Debug("Hot Reload: Running mod.");
        RunMod();
    }
}
