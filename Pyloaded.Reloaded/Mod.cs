#if DEBUG
using System.Diagnostics;
#endif
using Reloaded.Mod.Interfaces;
using Pyloaded.Reloaded.Template;
using Pyloaded.Reloaded.Configuration;
using Pyloaded.Reloaded.Python;
using Python.Runtime;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;

namespace Pyloaded.Reloaded;

public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly IReloadedHooks _hooks;
    private readonly ILogger _log;
    private readonly IMod _owner;

    private Config _config;
    private readonly IModConfig _modConfig;
    private readonly List<PyloadedMod> _pyMods = [];
    private readonly string[] _pyHelperScripts;
    private readonly PyloadedScans _scans;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks!;
        _log = context.Logger;
        _owner = context.Owner;
        _config = context.Configuration;
        _modConfig = context.ModConfig;
#if DEBUG
        Debugger.Launch();
#endif
        Project.Initialize(_modConfig, _modLoader, _log, true);
        Log.LogLevel = _config.LogLevel;
        
        _scans = new(_modLoader, _hooks!);

        var modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);

        var helpersDir = Path.Join(modDir, "Python", "PyloadedHelpers");
        _pyHelperScripts = Directory.GetFiles(helpersDir).Select(File.ReadAllText).ToArray();
        
        Runtime.PythonDLL = Path.Join(modDir, "python", "python313.dll");
        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();

        // nint conversions.
        PyObjectConversions.RegisterDecoder(PyNintCodec.Instance);
        PyObjectConversions.RegisterEncoder(PyNintCodec.Instance);
        
        // nuint conversions.
        PyObjectConversions.RegisterDecoder(PyNuintCodec.Instance);
        PyObjectConversions.RegisterEncoder(PyNuintCodec.Instance);
        
        // Encode IHook as an object so all properties are accessible.
        PyObjectConversions.RegisterEncoder(PyIHookEncoder.Instance);
        
        _modLoader.ModLoaded += ModLoaded;
    }

    private void ModLoaded(IModV1 mod, IModConfigV1 modConfig)
    {
        if (!Project.IsModDependent(modConfig)) return;

        var pyloadedDir = Path.Join(_modLoader.GetDirectoryForModId(modConfig.ModId), "pyloaded");
        if (!Directory.Exists(pyloadedDir)) return;

        var pyModFile = Path.Join(pyloadedDir, "mod.py");
        if (!File.Exists(pyModFile)) return;
        
        _pyMods.Add(new(_modLoader, _hooks, _scans, _log, modConfig.ModId, pyModFile, _pyHelperScripts));
    }

    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _config = configuration;
        _log.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        Log.LogLevel = _config.LogLevel;
    }

    #endregion

    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion
}