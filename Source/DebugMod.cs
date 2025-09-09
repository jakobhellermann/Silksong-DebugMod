using BepInEx;
using HarmonyLib;

namespace DebugMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class DebugMod : BaseUnityPlugin {
    private Harmony? harmony;

    private void Awake() {
        Log.Init(Logger);
        harmony = Harmony.CreateAndPatchAll(typeof(DebugMod).Assembly);
    }

    private void OnDestroy() {
        harmony?.UnpatchSelf();
    }
}