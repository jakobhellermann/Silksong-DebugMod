using HarmonyLib;
using UnityEngine;

namespace DebugMod;

[HarmonyPatch]
public class Patches {
    [HarmonyPatch(typeof(Debug), nameof(Debug.Log), typeof(object))]
    [HarmonyPrefix]
    private static bool Nope() => false;
}