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

    public object CreateHook(string scanId, PyObject hookMethod, string pattern)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, pattern, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, string pattern)
    {
        var id = $"{modId}+{GetPyName(hookMethod)}";
        var pyHook = new PyloadedHook(scans, id, pattern, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string scanId, PyObject hookMethod, PyObject onFail, string pattern)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, pattern, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, PyObject onFail, string pattern)
    {
        var id = $"{modId}+{GetPyName(hookMethod)}";
        var pyHook = new PyloadedHook(scans, id, pattern, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string scanId, PyObject hookMethod, nint address)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, address, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, nint address)
    {
        var id = $"{modId}+{GetPyName(hookMethod)}";
        var pyHook = new PyloadedHook(scans, id, address, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string scanId, PyObject hookMethod, PyObject onFail, nint address)
    {
        scanId = $"{modId}+{scanId}";
        var pyHook = new PyloadedHook(scans, scanId, address, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, PyObject onFail, nint address)
    {
        var id = $"{modId}+{GetPyName(hookMethod)}";
        var pyHook = new PyloadedHook(scans, id, address, hookMethod, onFail);
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