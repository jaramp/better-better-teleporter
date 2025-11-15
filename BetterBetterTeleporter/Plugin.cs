using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BetterBetterTeleporter.Integrations;
using BepInEx.Bootstrap;

namespace BetterBetterTeleporter;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("LethalNetworkAPI")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; private set; }
    public static Plugin Instance { get; private set; }

    internal void Awake()
    {
        if (Instance != null) return;

        Instance = this;
        Logger = base.Logger;
        ModConfig.Init(Config);

        if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
        {
            LethalConfigIntegration.Initialize();
        }

        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        Logger.LogInfo("BetterBetterTeleporter loaded successfully.");
    }
}
