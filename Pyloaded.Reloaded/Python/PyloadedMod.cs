using System.Reactive.Concurrency;
using Python.Runtime;
using Reloaded.Mod.Interfaces;

namespace Pyloaded.Reloaded.Python;

public class PyloadedMod
{
    private static readonly EventLoopScheduler Scheduler = new();
    
    private readonly string _name;
    private readonly string _pyModFile;
    private readonly PyloadedLogger _pyLog;
    private readonly PyModule _module;
    private readonly RxFileWatcher _pyModFileWatcher;

    public PyloadedMod(PyloadedContext pyCtx, ILogger log, string name, string pyModFile)
    {
        _name = name;
        _pyModFile = pyModFile;
        _pyLog = new(log, name);

        _pyModFileWatcher = new(pyModFile, Scheduler);
        _pyModFileWatcher.Changed += _ =>
        {
            Log.Information($"Hot Reload || Mod: {name}");
            RunMod();
        };

        using (Py.GIL())
        {
            _module = Py.CreateScope();
            _module.Set("Pyloaded", pyCtx);
            _module.Set("Log", _pyLog);
            _module.Set("print", (object? obj) => _pyLog.Print(obj));
        }
        
        RunMod();
    }

    private void RunMod()
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
}
