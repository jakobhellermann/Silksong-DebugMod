using BepInEx;
using BepInEx.Configuration;
using DebugMod.Modules;
using DebugMod.Savestates;
using HarmonyLib;
using UnityEngine;

namespace DebugMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class DebugMod : BaseUnityPlugin {
    internal static DebugMod Instance = null!;
    private SavestateModule? savestateModule;

    private Harmony? harmony;

    private void Awake() {
        Instance = this;

        Log.Init(Logger);
        harmony = Harmony.CreateAndPatchAll(typeof(DebugMod).Assembly);

        var configSavestateFilter = Config.Bind("Savestates",
            "Savestate filter",
            SavestateFilter.Player);
        var configSavestateLoadMode = Config.Bind("Savestates",
            "Savestate load mode",
            SavestateLoadMode.None);
        savestateModule = new SavestateModule(
            configSavestateFilter,
            configSavestateLoadMode,
            Config.Bind("Savestates",
                "Save",
                new KeyboardShortcut(KeyCode.KeypadPlus)
            ),
            Config.Bind("Savestates",
                "Load",
                new KeyboardShortcut(KeyCode.KeypadEnter)
            ),
            Config.Bind("Savestates",
                "Delete",
                new KeyboardShortcut(KeyCode.KeypadMinus)
            ),
            Config.Bind("Savestates",
                "Page next",
                new KeyboardShortcut(KeyCode.RightArrow)
            ),
            Config.Bind("Savestates",
                "Page prev",
                new KeyboardShortcut(KeyCode.LeftArrow)
            )
        );
    }

    private void Update() {
        savestateModule?.Update();
    }

    private void OnGUI() {
        savestateModule?.OnGui();
    }

    private void OnDestroy() {
        harmony?.UnpatchSelf();
    }
}