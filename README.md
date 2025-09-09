<div align="center">
  <h1>Pyloaded</h1>
  <img src="/Pyloaded.Reloaded/Preview.png" height="192"/>
  <p><b>Create Reloaded code mods with Python.</b></p>
</div>

## Getting Started
1. Create or edit a mod and add a **Mod Dependency** on **Pyloaded**.
2. Create the file: `MOD_FOLDER/Pyloaded/mod.py`
3. Open the file and create a `Mod` class like below.
```
class Mod:
  def __init__(self):
	print("Hello, World!")
    pass
```
4. Now, test if your mod is working correctly by starting the game and looking for the `Hello, World!` message in the console.
5. If you found it, congrats 🎉 and continue to the **Using Pyloaded** section! If not, double-check that you followed each step correctly.

## Using Pyloaded
**Pyloaded** is made with **Python.NET**, which lets your mod both import and interact with .NET/C# code inside your script.

**Example**
```python
from System.Drawing import Point

p = Point(5, 5)
```

Using this, **Pyloaded** adds some tools of its own to make common modding tasks simple to do.

### Scans (SigScans, Pattern Scanning, Etc.)
The basic, but versatile, pattern scan. 

**`Pyloaded.ScanHooks.AddScan(scanId, onSuccess[, onFail], pattern)`**
- *scanId* - String - Scan ID, used for replacing patterns externally.
- onSuccess - Function - Callback function which is given the scan result.
- *onFail* - Function - Optional callback function to run if the scan failed. Mainly used to hide the default error message that shows for a failed scan.
- *pattern* - String - The hex pattern to scan for.

**Example**
```
Pyloaded.ScanHooks.AddScan("MyCoolScan", scan_success, "FF FF FF FF")

def scan_success(result):
  print(result)
```

### Function Hooks
Now you're modding ✨! If you don't already understand function hooks, I'd recommend reading up on them, but the tl;dr is to make the game run your code instead of its own.

From there, you can do whatever you want: replace the code completely, edit it a bit, disable it, or just log some data before running the original code.

**`Pyloaded.ScanHooks.CreateHook([scanId,] hookFunc[, onFail], pattern)`**
- *scanId* - String - Optional Scan ID, used for replacing patterns externally. If no ID is given, then the name of `hookFunc` will be used as the ID.
- *hookFunc* - Function - The function the hook will run instead of the original function.
- *onFail* - Function - Optional callback function to run if the scan failed. Mainly used to hide the default error message that shows for a failed scan.
- *pattern* - String - The hex pattern of the function to hook.

**`Pyloaded.ScanHooks.CreateHook([scanId, ]hookFunc[, onFail], address)`**
- *scanId* - String - Optional Scan ID, used for replacing patterns externally. If no ID is given, then the name of `hookFunc` will be used as the ID.
- *hookFunc* - Function - The function the hook will run instead of the original function.
- *onFail* - Function - Optional callback function to run if the scan failed. Mainly used to hide the default error message that shows for a failed scan.
- *address* - Integer - The direct address of the function to hook.

**Example (Persona 4 Golden)**
```
class Mod:
  def __init__(self):

    # Function hook with a pattern.
    self.set_cue_id = Pyloaded.ScanHooks.CreateHook(self.cri_set_cue_id, "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 54 41 56 41 57 48 81 EC 80 00 00 00 4D 63 F0")

    # Function hook with an address.
    self.set_cue_id = Pyloaded.ScanHooks.CreateHook(self.cri_set_cue_id, 0x140611E4C)
    pass

  def cri_set_cue_id(self, a: long, b: long, cueId: int):
    print(f"cri_set_cue_id: 0x{a:X} || 0x{b:X} || Cue ID: {cueId}")
    return self.set_cue_id.Hook.OriginalFunction(a, b, cueId)
```

#### Important Notes
___
**Creating Your Function**

When making your function, it's **strongly recommended** to add the correct type to each parameter like in the example `a: long, b: long, cueId: int`.

By default, all parameters are treated as `nint`, which in technical terms means it will pull the value of the full register. This puts you at risk of getting junk data when the original function normally wouldn't.

**Calling the Original Function**

Notice how the hooks are being saved to the variable `self.set_cue_id` on the class? This is required if you plan to call the original function found at `YOUR_HOOK.Hook.OriginalFunction`.

In normal Reloaded mods, this is always required for technical reasons, but **Pyloaded** saves all hooks created. If you don't plan to call the original function, you don't have to save the hook yourself.
### Logging
While `print` can get you far, sometimes you want to mark a message as a bit more special than the rest.

For that, your mod has access to it's own logger in `Logger` which can log messages with different "levels".

**Example**
```
Log.Verbose("No one will ever read this.")
Log.Debug("Something broke, didn't it?")
Log.Information("Hello, World!") # Equivalent to print()
Log.Warning("Something *might've* broke, but we ain't stopping 'till we crash!")
Log.Error("Something broke and we're about to crash, SEND HELP!!!")
```

The level can be changed in **Pyloaded's** Reloaded config.

### Pointers
For working with pointers, **Pyloaded** gives access `Reloaded.Memory`'s `Ptr<T>` type.

**`Pyloaded.CreatePtr[T](address)`**

- *T* - type - The type of pointer.
- *address* - number - The address of the pointer.

**Example**
```
ptr = Pyloaded.CreatePtr[int](0x14000000)
ptr.Set(42)
```

### Reloaded Libraries
Within `Pyloaded` you also have access to the mod loader and normal Reloaded hooks.

**Example**
```
Pyloaded.ModLoader

Pyloaded.ReloadedHooks
```

### Hot Reload
**Pyloaded** mods support real-time editing! Build or edit your mod without needing to restart the game, allowing you to quickly test ideas or fix bugs.

## Special Thanks
- **Sewer56** - Reloaded and the countless accompanying libraries!
- **.NET Foundation** - Python.NET!
- **Python Software Foundation** - Python! Duh!
