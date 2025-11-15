using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using BetterBetterTeleporter.Integrations;
using HarmonyLib;

namespace BetterBetterTeleporter;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sigurd.csync", "5.0.1")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; private set; }
    public static Plugin Instance { get; private set; }
    public static ModConfig ModConfig { get; private set; }

    internal void Awake()
    {
        if (Instance != null) return;

        Instance = this;
        Logger = base.Logger;
        ModConfig = new ModConfig(Config);

        if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
        {
            LethalConfigIntegration.Initialize();
        }

        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        Logger.LogInfo("BetterBetterTeleporter loaded successfully.");
    }
}
