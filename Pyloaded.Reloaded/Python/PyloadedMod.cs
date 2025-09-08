using Python.Runtime;
using Reloaded.Mod.Interfaces;

namespace Pyloaded.Reloaded.Python;

public class PyloadedMod : IDisposable
{
    private readonly string _name;
    private readonly string _pyModFile;
    private readonly List<object> _pyHooks = [];
    private readonly PyloadedLogger _pyLog;

    private readonly PyModule _module;
    
    public PyloadedMod(PyloadedContext pyCtx, ILogger log, string name, string pyModFile)
    {
        _name = name;
        _pyModFile = pyModFile;
        _pyLog = new(log, name);

        var pyModSource = File.ReadAllText(pyModFile);
        using (Py.GIL())
        {
            _module = Py.CreateScope();
            _module.Set("Pyloaded", pyCtx);
            _module.Set("Log", _pyLog);
            _module.Set("print", (object? obj) => _pyLog.Print(obj));
            
            Log.Debug($"Compiling: {name}");
            using var pyModCompiled = PythonEngine.Compile(pyModSource, pyModFile);
        
            Log.Debug($"Running: {name}");
            _module.Execute(pyModCompiled);
        
            using var result = _module.InvokeMethod("mod");
            if (result.IsNone() || result.As<int>() == 0) Log.Information($"Loaded: {name}");
            else Log.Error($"Error: {name}");
        }
    }

    public void Dispose()
    {
        _module.Dispose();
    }
}
