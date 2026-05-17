using HarmonyLib;
using UnityEngine.UI;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches UnityEngine.UI.Button, not strictly a VSeeFace class but we'll
  see if I need to patch anything random like this.
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(Button))]
    public static class Patch_Button
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnPointerClick")]
        static void OnPointerClick_Postfix(Button __instance)
        {
            RfPlugin.LogDebug($"-- {nameof(Button)}.OnPointerClick");
            
            // TODO: can't I just add listeners to the button itself
            if (__instance != null && __instance.gameObject != null && __instance.gameObject == LaunchUI.startButton)
            {
                RfPlugin.LogDebug($"-- {nameof(Button)}.OnPointerClick");
            }
        }
    }

}
