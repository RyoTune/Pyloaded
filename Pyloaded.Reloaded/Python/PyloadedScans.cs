using Python.Runtime;

namespace Pyloaded.Reloaded.Python;

public class PyloadedScans
{
    // ReSharper disable once CollectionNeverQueried.Local
    // Need to store hook references so they don't get GC'd.
    private static readonly List<object> PyHooks = [];

    public static void AddScan(string id, PyObject onSuccess, string pattern)
    {
        Project.Scans.AddScan(id, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        });
    }

    public static void AddScan(string id, PyObject onSuccess, PyObject onFail, string pattern)
    {
        Project.Scans.AddScan(id, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        }, () =>
        {
            using (Py.GIL())
                onFail.Invoke();
        });
    }

    public static object CreateHook(string id, PyObject hookMethod, string pattern)
    {
        var pyHook = new PyloadedHook(id, pattern, hookMethod);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(PyObject hookMethod, string pattern)
    {
        var pyHook = new PyloadedHook(null, pattern, hookMethod);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(string id, PyObject hookMethod, PyObject onFail, string pattern)
    {
        var pyHook = new PyloadedHook(id, pattern, hookMethod, onFail);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(PyObject hookMethod, PyObject onFail, string pattern)
    {
        var pyHook = new PyloadedHook(null, pattern, hookMethod, onFail);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(string id, PyObject hookMethod, nint address)
    {
        var pyHook = new PyloadedHook(id, address, hookMethod);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(PyObject hookMethod, nint address)
    {
        var pyHook = new PyloadedHook(null, address, hookMethod);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(string id, PyObject hookMethod, PyObject onFail, nint address)
    {
        var pyHook = new PyloadedHook(id, address, hookMethod, onFail);
        PyHooks.Add(pyHook);
        return pyHook;
    }

    public static object CreateHook(PyObject hookMethod, PyObject onFail, nint address)
    {
        var pyHook = new PyloadedHook(null, address, hookMethod, onFail);
        PyHooks.Add(pyHook);
        return pyHook;
    }
}