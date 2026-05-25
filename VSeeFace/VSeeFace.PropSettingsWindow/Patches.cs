using HarmonyLib;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the PropSettingsWindow class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(PropSettingsWindow))]
    public static class Patch_PropSettingsWindow
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PropSettingsWindow.Update))]
        static void Update_Prefix(PropSettingsWindow __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropSettingsWindow)}.{nameof(PropSettingsWindow.Update)}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropSettingsWindow.Update))]
        static void Update_Postfix(PropSettingsWindow __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropSettingsWindow)}.{nameof(PropSettingsWindow.Update)}");
        }
    }

}
