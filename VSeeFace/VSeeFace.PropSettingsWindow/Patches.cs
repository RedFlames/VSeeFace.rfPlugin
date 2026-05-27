using HarmonyLib;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the PropSettingsWindow class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    // Using this to add things to RefreshSettings because it's different from PropManager.onSelectedPropChange ...
    public delegate void OnRefreshSettings();
    public static event OnRefreshSettings onRefreshSettings;

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
        
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropSettingsWindow.RefreshSettings))]
        static void RefreshSettings_Postfix(PropSettingsWindow __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropSettingsWindow)}.{nameof(PropSettingsWindow.Update)}");
            onRefreshSettings();
        }
        
    }

}
