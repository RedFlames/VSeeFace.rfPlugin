using HarmonyLib;

using UnityEngine.UI;

namespace rfPlugin;

/*
  See RfPlugin.cs for the main class. These patches are part of it.
  Patches UnityEngine.UI.Button
*/
public partial class RfPlugin
{
    [HarmonyPatch(typeof(Button))]
    public static class Patch_Button
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnPointerClick")]
        static void OnPointerClick_Postfix(Button __instance)
        {
            // moved to VSeeFace.VSeeFaceHelper.Patches
        }
    }

}
