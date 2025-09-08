using Python.Runtime;
using Reloaded.Hooks.Definitions;

namespace Pyloaded.Reloaded.Python;

public class PyIHookEncoder : IPyObjectEncoder
{
    public static readonly PyIHookEncoder Instance = new();
    
    public bool CanEncode(Type type) => type == typeof(IHook);

    public PyObject? TryEncode(object value) => value.ToPython();
}