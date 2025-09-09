using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Pointers;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedContext(IModLoader modLoader, IReloadedHooks hooks, IScans scans, string modId)
{
    public IModLoader ModLoader { get; } = modLoader;

    public IReloadedHooks ReloadedHooks { get; } = hooks;

    public PyloadedScanHooks ScanHooks { get; } = new(scans, modId);

    public static unsafe Ptr<T> CreatePtr<T>(nint address) where T : unmanaged => new((T*)address);
}
