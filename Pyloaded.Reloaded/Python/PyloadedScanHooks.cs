using Python.Runtime;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedScanHooks(IScans scans, string modId)
{
    // ReSharper disable once CollectionNeverQueried.Local
    // Need to store hook references so they don't get GC'd.
    private readonly List<PyloadedHook> _pyHooks = [];

    public void AddScan(string scanId, PyObject onSuccess, string pattern)
    {
        scanId = $"{modId}+{scanId}";
        scans.AddScan(scanId, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        });
    }

    public void AddScan(string scanId, PyObject onSuccess, PyObject onFail, string pattern)
    {
        scanId = $"{modId}+{scanId}";
        scans.AddScan(scanId, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        }, () =>
        {
            using (Py.GIL())
                onFail.Invoke();
        });
    }

    public object CreateHook(string scanId, PyObject hookFunc, string pattern)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, pattern, hookFunc);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookFunc, string pattern)
    {
        var id = $"{modId}+{GetPyName(hookFunc)}";
        var pyHook = new PyloadedHook(scans, id, pattern, hookFunc);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string scanId, PyObject hookFunc, PyObject onFail, string pattern)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, pattern, hookFunc, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookFunc, PyObject onFail, string pattern)
    {
        var id = $"{modId}+{GetPyName(hookFunc)}";
        var pyHook = new PyloadedHook(scans, id, pattern, hookFunc, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string scanId, PyObject hookFunc, nint address)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, address, hookFunc);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookFunc, nint address)
    {
        var id = $"{modId}+{GetPyName(hookFunc)}";
        var pyHook = new PyloadedHook(scans, id, address, hookFunc);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string scanId, PyObject hookFunc, PyObject onFail, nint address)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, address, hookFunc, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookFunc, PyObject onFail, nint address)
    {
        var id = $"{modId}+{GetPyName(hookFunc)}";
        var pyHook = new PyloadedHook(scans, id, address, hookFunc, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    private static string GetPyName(PyObject obj)
    {
        using (Py.GIL())
            return obj.GetAttr("__name__").As<string>();
    }

    internal void ClearHooks()
    {
        foreach (var pyHook in _pyHooks.Where(x => x.Hook?.IsHookEnabled == true))
        {
            pyHook.Disable?.Invoke();
        }
    }
}