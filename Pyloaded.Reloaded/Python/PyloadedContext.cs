using Reloaded.Memory.Pointers;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedContext(IModLoader modLoader, IScans scans)
{
    public IModLoader ModLoader { get; } = modLoader;

    //public IReloadedHooks Hooks { get; } = hooks;

    public PyloadedScanHooks ScanHooks { get; } = new(scans);

    public static unsafe Ptr<T> CreatePtr<T>(nint address) where T : unmanaged => new((T*)address);
}
