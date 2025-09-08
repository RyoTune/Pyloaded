using Python.Runtime;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedScanHooks(IScans scans)
{
    // ReSharper disable once CollectionNeverQueried.Local
    // Need to store hook references so they don't get GC'd.
    private readonly List<PyloadedHook> _pyHooks = [];

    public void AddScan(string id, PyObject onSuccess, string pattern)
    {
        scans.AddScan(id, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        });
    }

    public void AddScan(string id, PyObject onSuccess, PyObject onFail, string pattern)
    {
        scans.AddScan(id, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        }, () =>
        {
            using (Py.GIL())
                onFail.Invoke();
        });
    }

    public object CreateHook(string id, PyObject hookMethod, string pattern)
    {
        var pyHook = new PyloadedHook(scans, id, pattern, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, string pattern)
    {
        var pyHook = new PyloadedHook(scans, null, pattern, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string id, PyObject hookMethod, PyObject onFail, string pattern)
    {
        var pyHook = new PyloadedHook(scans, id, pattern, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, PyObject onFail, string pattern)
    {
        var pyHook = new PyloadedHook(scans, null, pattern, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string id, PyObject hookMethod, nint address)
    {
        var pyHook = new PyloadedHook(scans, id, address, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, nint address)
    {
        var pyHook = new PyloadedHook(scans, null, address, hookMethod);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(string id, PyObject hookMethod, PyObject onFail, nint address)
    {
        var pyHook = new PyloadedHook(scans, id, address, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    public object CreateHook(PyObject hookMethod, PyObject onFail, nint address)
    {
        var pyHook = new PyloadedHook(scans, null, address, hookMethod, onFail);
        _pyHooks.Add(pyHook);
        return pyHook;
    }

    internal void ClearHooks()
    {
        foreach (var pyHook in _pyHooks.Where(x => x.Hook?.IsHookEnabled == true))
        {
            pyHook.Disable?.Invoke();
        }
    }
}