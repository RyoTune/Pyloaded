using System.Diagnostics.CodeAnalysis;
using Python.Runtime;

namespace Pyloaded.Reloaded.Python;

public class PyNintCodec : IPyObjectEncoder, IPyObjectDecoder
{
    public static readonly PyNintCodec Instance = new();
    
    public bool CanEncode(Type type) => type == typeof(nint);

    public PyObject TryEncode(object value)
    {
        if (Environment.Is64BitProcess)
        {
            return new PyInt((nint)value);
        }

        return new PyInt((int)(nint)value);
    }
    
    public bool CanDecode(PyType objectType, Type targetType)
        => objectType.Name == "int" && targetType == typeof(nint);

    public bool TryDecode<T>(PyObject pyObj, [UnscopedRef] out T? value)
    {
        if (Environment.Is64BitProcess)
        {
            value = (T)(object)(nint)pyObj.As<long>(); 
        }
        else
        {
            value = (T)(object)(nint)pyObj.As<int>();
        }
            
        return true;
    }
}