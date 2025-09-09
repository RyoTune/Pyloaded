namespace Pyloaded.Reloaded.Python;

public static class PyloadedUtils
{
    public static object ConvertUnchecked(nint value, Type targetType)
    {
        if (targetType == typeof(sbyte)) return unchecked((sbyte)value);
        if (targetType == typeof(sbyte)) return unchecked((sbyte)value);
        if (targetType == typeof(byte)) return unchecked((byte)value);
        if (targetType == typeof(short)) return unchecked((short)value);
        if (targetType == typeof(ushort)) return unchecked((ushort)value);
        if (targetType == typeof(int)) return unchecked((int)value);
        if (targetType == typeof(uint)) return unchecked((uint)value);
        if (targetType == typeof(long)) return (long)value;
        if (targetType == typeof(ulong)) return unchecked((ulong)value);
        if (targetType == typeof(nint)) return value;
        if (targetType == typeof(nuint)) return unchecked((nuint)value);

        throw new InvalidCastException($"Unsupported target type '{targetType}'.");
    }
}