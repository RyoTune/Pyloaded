using System.Diagnostics.CodeAnalysis;
using Python.Runtime;

namespace Pyloaded.Reloaded.Python;

public class PyNuintCodec : IPyObjectEncoder, IPyObjectDecoder
{
    public static readonly PyNuintCodec Instance = new();
    
    public bool CanEncode(Type type) => type == typeof(nuint);

    public PyObject TryEncode(object value)
    {
        if (Environment.Is64BitProcess)
        {
            return new PyInt((nuint)value);
        }

        return new PyInt((uint)(nuint)value);
    }
    
    public bool CanDecode(PyType objectType, Type targetType)
        => objectType.Name == "int" && targetType == typeof(nuint);

    public bool TryDecode<T>(PyObject pyObj, [UnscopedRef] out T? value)
    {
        if (Environment.Is64BitProcess)
        {
            value = (T)(object)(nuint)pyObj.As<ulong>(); 
        }
        else
        {
            value = (T)(object)(nuint)pyObj.As<uint>();
        }
            
        return true;
    }
}