using Python.Runtime;
using Reloaded.Hooks.Definitions;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

internal class PyloadedHook(IScans scans)
{
    private static PyObject? _inspect;
    private HookBundle? _hookBundle;

    public PyloadedHook(IScans scans, string? id, string pattern, PyObject hookMethod) : this(scans)
        => SetupScanHook(id, pattern, 0, hookMethod, null);
    
    public PyloadedHook(IScans scans, string? id, string pattern, PyObject hookMethod, PyObject onFail) : this(scans)
        => SetupScanHook(id, pattern, 0, hookMethod, onFail);
    
    public PyloadedHook(IScans scans, string? id, nint defaultResult, PyObject hookMethod) : this(scans)
        => SetupScanHook(id, null, defaultResult, hookMethod, null);

    public PyloadedHook(IScans scans, string? id, nint defaultResult, PyObject hookMethod, PyObject onFail) : this(scans)
        => SetupScanHook(id, null, defaultResult, hookMethod, onFail);

    public IHook? Hook => _hookBundle?.Hook;

    internal Action? Enable => _hookBundle?.Enable;

    internal Action? Disable => _hookBundle?.Disable;
    
    /// <summary>
    /// Handles setting up a scan hook for all possible configuration of input parameters.
    /// </summary>
    /// <param name="id">Scan ID. If null, uses <paramref name="hookMethod"/>'s name for ID.</param>
    /// <param name="pattern">Scan pattern. If null, scan is given <paramref name="defaultResult"/>.</param>
    /// <param name="defaultResult">Default result of scan, if pattern not provided.</param>
    /// <param name="hookMethod">Python hook method.</param>
    /// <param name="onFail">Python method to call on scan failure, if any.</param>
    private void SetupScanHook(string? id, string? pattern, nint defaultResult, PyObject hookMethod, PyObject? onFail)
    {
        using (Py.GIL())
        {
            var methodInfo = GetMethodInfo(hookMethod);
            id ??= methodInfo.Name;

            Action? onFailCb = onFail != null
                ? () =>
                {
                    using (Py.GIL())
                        onFail.Invoke();
                }
                : null;

            if (pattern != null)
            {
                scans.AddScanHook(id, pattern, (result, hooks) => _hookBundle = CreateHook(hooks, hookMethod, methodInfo, result), onFailCb);
            }
            else
            {
                scans.AddScanHook(id, defaultResult, (result, hooks) => _hookBundle = CreateHook(hooks, hookMethod, methodInfo, result), onFailCb);
            }
        }
    }
    
    private static HookBundle CreateHook(IReloadedHooks hooks, PyObject hookMethod, MethodInfo methodInfo, nint address)
    {
        switch (methodInfo.Params.Length)
        {
            case 0:
                var hook0 = hooks.CreateHook<Func0>(() => CallPyMethod(hookMethod, methodInfo), address).Activate();
                return new(hook0, hook0.Enable, hook0.Disable);
            case 1:
                var hook1 = hooks.CreateHook<Func1>((a) => CallPyMethod(hookMethod, methodInfo, a), address).Activate();
                return new(hook1, hook1.Enable, hook1.Disable);
            case 2:
                var hook2 = hooks.CreateHook<Func2>((a, b) => CallPyMethod(hookMethod, methodInfo, a, b), address).Activate();
                return new(hook2, hook2.Enable, hook2.Disable);
            case 3:
                var hook3 = hooks.CreateHook<Func3>((a, b, c) => CallPyMethod(hookMethod, methodInfo, a, b, c), address).Activate();
                return new(hook3, hook3.Enable, hook3.Disable);
            case 4:
                var hook4 = hooks.CreateHook<Func4>((a, b, c, d) => CallPyMethod(hookMethod, methodInfo, a, b, c, d), address)
                    .Activate();
                return new(hook4, hook4.Enable, hook4.Disable);
            case 5:
                var hook5 = hooks.CreateHook<Func5>((a, b, c, d, e) => CallPyMethod(hookMethod, methodInfo, a, b, c, d, e), address)
                    .Activate();
                return new(hook5, hook5.Enable, hook5.Disable);
            case 6:
                var hook6 = hooks.CreateHook<Func6>((a, b, c, d, e, f) => CallPyMethod(hookMethod, methodInfo, a, b, c, d, e, f),
                        address)
                    .Activate();
                return new(hook6, hook6.Enable, hook6.Disable);
            case 7:
                var hook7 = hooks.CreateHook<Func7>((a, b, c, d, e, f, g) => CallPyMethod(hookMethod, methodInfo, a, b, c, d, e, f, g),
                        address)
                    .Activate();
                return new(hook7, hook7.Enable, hook7.Disable);
            case 8:
                var hook8 = hooks
                    .CreateHook<Func8>((a, b, c, d, e, f, g, h) => CallPyMethod(hookMethod, methodInfo, a, b, c, d, e, f, g, h),
                        address)
                    .Activate();
                return new(hook8, hook8.Enable, hook8.Disable);
            default:
                throw new NotSupportedException("Function hooks can only have a maximum of 8 parameters.");
        }
    }

    private static nint CallPyMethod(PyObject method, MethodInfo methodInfo, params nint[] args)
    {
        using (Py.GIL())
        {
            return method.Invoke(CreatePyArgs(methodInfo, args)).As<nint>();
        }
    }

    private static PyObject[] CreatePyArgs(MethodInfo methodInfo, params nint[] args)
    {
        var pyArgs = new List<PyObject>();
        for (var i = 0; i < args.Length; i++)
        {
            var currArg = args[i];
            var targetType = methodInfo.Params[i].Type;
            
            // Encode arg to Python, marshalling first to target type if parameter has type info.
            pyArgs.Add(targetType != null ? PyloadedUtils.ConvertUnchecked(currArg, targetType).ToPython() : currArg.ToPython());
        }

        return pyArgs.ToArray();
    }

    private static MethodInfo GetMethodInfo(PyObject method)
    {
        const string clrTypeName = "CLRMetatype";
        const string intTypeName = "int";
        
        // Import once and save for all hooks.
        if (_inspect == null) _inspect = Py.Import("inspect");
        
        // Get method name.
        var methodName = method.GetAttr("__name__").As<string>();
        
        // Get method parameters, including type info if available.
        var methodParams = new List<MethodParamInfo>();
        
        using var sig = _inspect.InvokeMethod("signature", method);
        using var sigParams = sig.GetAttr("parameters");
        using var sigParamsIter = sigParams.GetIterator();
        
        while (sigParamsIter.MoveNext())
        {
            var currParam = sigParams[sigParamsIter.Current];
            var currParamName = sigParamsIter.Current.As<string>();

            var anno = currParam.GetAttr("annotation");
            var currParamTypeName = anno.GetAttr("__name__").As<string>();

            if (currParamTypeName == intTypeName || anno.GetPythonType().Name == clrTypeName)
            {
                var superType = currParamName == intTypeName ? typeof(int) : anno.As<Type>();
                methodParams.Add(new(currParamName, superType));
                
                Log.Verbose($"{nameof(GetMethodInfo)} || Method: {methodName} || Registered param '{currParamName}' as CLR type '{superType.Name}'.");
            }
            else
            {
                methodParams.Add(new(currParamName, null));
                Log.Verbose($"{nameof(GetMethodInfo)} || Method: {methodName} || Registered param '{currParamName}'.");
            }
        }
        
        return new(methodName, methodParams.ToArray());
    }

    private delegate nint Func0();
    private delegate nint Func1(nint a);
    private delegate nint Func2(nint a, nint b);
    private delegate nint Func3(nint a, nint b, nint c);
    private delegate nint Func4(nint a, nint b, nint c, nint d);
    private delegate nint Func5(nint a, nint b, nint c, nint d, nint e);
    private delegate nint Func6(nint a, nint b, nint c, nint d, nint e, nint f);
    private delegate nint Func7(nint a, nint b, nint c, nint d, nint e, nint f, nint g);
    private delegate nint Func8(nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h);

    private record MethodInfo(string Name, MethodParamInfo[] Params);

    private record MethodParamInfo(string Name, Type? Type);

    private record HookBundle(IHook Hook, Action Enable, Action Disable);
}