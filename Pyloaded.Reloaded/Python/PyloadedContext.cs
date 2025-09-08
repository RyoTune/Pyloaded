using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Scans;

namespace Pyloaded.Reloaded.Python;

public class PyloadedContext(IModLoader modLoader, IScans scans)
{
    public IModLoader ModLoader { get; } = modLoader;

    //public IReloadedHooks Hooks { get; } = hooks;

    public PyloadedScanHooks ScanHooks { get; } = new(scans);
}