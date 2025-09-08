using Python.Runtime;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedContext(IModLoader modLoader, IReloadedHooks hooks)
{
    private static readonly List<object> PyHooks = [];
    
    public IModLoader ModLoader { get; } = modLoader;

    public IReloadedHooks Hooks { get; } = hooks;

    public IScans Scans { get; } = Project.Scans;

    public void AddScan(string id, string pattern, PyObject onSuccess)
    {
        Scans.AddScan(id, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        });
    }

    public void AddScan(string id, string pattern, PyObject onSuccess, PyObject onFail)
    {
        Scans.AddScan(id, pattern, result =>
        {
            using (Py.GIL())
                onSuccess.Invoke(result.ToPython());
        }, () =>
        {
            using (Py.GIL())
                onFail.Invoke();
        });
    }

    public object CreateHook(PyObject method, PyObject address)
    {
        object? hook;
        
        using (Py.GIL())
        {
            using var inspect = Py.Import("inspect");
            using var sig = inspect.InvokeMethod("signature", method);
            var numParams = sig.GetAttr("parameters").Length();

            hook = numParams switch
            {
                0 => Hooks.CreateHook<Func0>(() => CallPyMethod(method), address.As<nint>()).Activate(),
                1 => Hooks.CreateHook<Func1>((a) => CallPyMethod(method, a), address.As<nint>()).Activate(),
                2 => Hooks.CreateHook<Func2>((a, b) => CallPyMethod(method, a, b), address.As<nint>()).Activate(),
                3 => Hooks.CreateHook<Func3>((a, b, c) => CallPyMethod(method, a, b, c), address.As<nint>()).Activate(),
                4 => Hooks.CreateHook<Func4>((a, b, c, d) => CallPyMethod(method, a, b, c, d), address.As<nint>())
                    .Activate(),
                5 => Hooks.CreateHook<Func5>((a, b, c, d, e) => CallPyMethod(method, a, b, c, d, e), address.As<nint>())
                    .Activate(),
                6 => Hooks.CreateHook<Func6>((a, b, c, d, e, f) => CallPyMethod(method, a, b, c, d, e, f),
                        address.As<nint>())
                    .Activate(),
                7 => Hooks.CreateHook<Func7>((a, b, c, d, e, f, g) => CallPyMethod(method, a, b, c, d, e, f, g),
                        address.As<nint>())
                    .Activate(),
                8 => Hooks.CreateHook<Func8>((a, b, c, d, e, f, g, h) => CallPyMethod(method, a, b, c, d, e, f, g, h),
                        address.As<nint>())
                    .Activate(),
                _ => throw new NotSupportedException("Function hooks can only have a maximum of 8 parameters.")
            };
        }

        PyHooks.Add(hook);
        return hook;
    }

    private static nint CallPyMethod(PyObject method, params nint[] args)
    {
        using (Py.GIL())
        {
            return method.Invoke(CreatePyArgs(args)).As<nint>();
        }
    }
    
    private static PyObject[] CreatePyArgs(params nint[] args) => args.Select(x => x.ToPython()).ToArray();

    private delegate nint Func0();
    private delegate nint Func1(nint a);
    private delegate nint Func2(nint a, nint b);
    private delegate nint Func3(nint a, nint b, nint c);
    private delegate nint Func4(nint a, nint b, nint c, nint d);
    private delegate nint Func5(nint a, nint b, nint c, nint d, nint e);
    private delegate nint Func6(nint a, nint b, nint c, nint d, nint e, nint f);
    private delegate nint Func7(nint a, nint b, nint c, nint d, nint e, nint f, nint g);
    private delegate nint Func8(nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h);
}