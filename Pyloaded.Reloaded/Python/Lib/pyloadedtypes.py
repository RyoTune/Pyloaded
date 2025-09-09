from typing import NewType

sbyte = NewType('sbyte', int)
byte = NewType('byte', int)
short = NewType('short', int)
ushort = NewType('ushort', int)
#int = NewType('int', int)
uint = NewType('uint', int)
long = NewType('long', int)
ulong = NewType('ulong', int)
nint = NewType('nint', int)
nuint = NewType('nuint', int)

types_dict = {
    'byte': byte,
    'sbyte': sbyte,
    'ushort': ushort,
    'short': short,
    'uint': uint,
    'long': long,
    'ulong': ulong,
    'nuint': nuint,
    'nint': nint,
}