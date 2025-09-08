using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

namespace Pyloaded.Reloaded.Python;

public class PyloadedContext(IModLoader modLoader, IReloadedHooks hooks)
{
    public IModLoader ModLoader { get; } = modLoader;

    public IReloadedHooks Hooks { get; } = hooks;

    public PyloadedScans Scans { get; } = new();
}