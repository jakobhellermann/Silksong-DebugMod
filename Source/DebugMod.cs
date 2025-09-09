using BepInEx;
using BepInEx.Configuration;
using DebugMod.Modules;
using DebugMod.Savestates;
using DebugModPlus.Modules;
using HarmonyLib;
using UnityEngine;

namespace DebugMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class DebugMod : BaseUnityPlugin {
    internal static DebugMod Instance = null!;
    private SavestateModule? savestateModule;
    private InfotextModule? infotextModule;

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


        var configInfoTextEnabled = Config.Bind("Info Text Panel",
            "Show Info",
            false);
        var configInfoTextFilter = Config.Bind("Info Text Panel",
            "Filter",
            InfotextModule.InfotextFilter.Basic);

        infotextModule = new InfotextModule(configInfoTextEnabled, configInfoTextFilter);
    }

    private void Update() {
        savestateModule?.Update();
        infotextModule?.Update();
    }

    private void OnGUI() {
        savestateModule?.OnGui();
        infotextModule?.OnGui();
    }

    private void OnDestroy() {
        harmony?.UnpatchSelf();
    }
}