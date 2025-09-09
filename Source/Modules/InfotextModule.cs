using System;
using UnityEngine;
using BepInEx.Configuration;
using DebugMod;
using DebugMod.DebugInfo;

namespace DebugModPlus.Modules;

public class InfotextModule(
    ConfigEntry<bool> infotextActive,
    ConfigEntry<InfotextModule.InfotextFilter> infotextFilter
) {
    [Flags]
    public enum InfotextFilter {
        Basic = 1 << 1,

        // ReSharper disable once InconsistentNaming
        TAS = 1 << 2,
    }

    private string debugCanvasInfoText = "";

    public void Update() {
        if (infotextActive.Value) {
            debugCanvasInfoText = UpdateInfoText();
        }
    }


    private string UpdateInfoText() {
        var text = "";
        try {
            if (infotextFilter.Value.HasFlag(InfotextFilter.Basic)) {
                text += DebugInfo.GetBasicInfoText();
            }

            if (infotextFilter.Value.HasFlag(InfotextFilter.TAS)) {
                if (!(text.EndsWith("\n") || text == "")) text += "\n";
                text += DebugInfo.GetInfoText();
            }
        } catch (Exception e) {
            Log.Error(e);
        }

        return text;
    }


    public void OnGui() {
        if (!infotextActive.Value) return;

        style ??= GuiStyle();
        // style.font = Resources.GetBuiltinResource<Font>("Courier New.ttf");
        // Log.Info(style.font);

        var textSize = style.CalcSize(new GUIContent(debugCanvasInfoText));
        const float margin = 10;
        var bottomLeft = new Rect(
            margin,
            Screen.height - textSize.y - margin,
            textSize.x,
            textSize.y
        );
        var topLeft = new Rect(margin, margin, textSize.x, textSize.y);
        GUI.Box(bottomLeft, debugCanvasInfoText, style);
    }

    private static GUIStyle GuiStyle() => new(GUI.skin.box) {
        fontSize = 20,
        wordWrap = false,
        alignment = TextAnchor.UpperLeft,
        fontStyle = FontStyle.Bold,
        // normal = { background = TextureUtils.GetColorTexture(new Color(0, 0, 0, 0)) },
    };

    private GUIStyle? style;

    public void Destroy() {
        // UnityEngine.Object.Destroy(debugCanvasInfoText.gameObject);
    }
}