using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the PropWindow class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(PropWindow))]
    public static class Patch_PropWindow
    {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static bool Start_Prefix(PropWindow __instance)
            {
                // RfPlugin.Log($"is this thing on -- {nameof(PropImage)}.{nameof(PropImage.StartDragging)}");
                return __instance == MainPropWindow;
            }
    
            [HarmonyPrefix]
            [HarmonyPatch(nameof(PropWindow.SaveList))]
            static bool SaveList_Prefix(PropWindow __instance)
            {
                // RfPlugin.Log($"is this thing on -- {nameof(PropImage)}.{nameof(PropImage.StartDragging)}");
                return __instance == MainPropWindow;
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(PropWindow.SaveList))]
            static void SaveList_Postfix(PropWindow __instance)
            {
                IEnumerable<string> contents = from 
                    b in __instance.GetComponentsInChildren<PropButton>()
                    where b.gameObject.activeSelf
                    select b.imagePath;
                
                RfPlugin.Log($"[PropWindow.SaveList] -- saved props to file: {__instance.propListFile}");
                foreach (var path in contents)
                    RfPlugin.Log($"                       - {path}");
            }
    }

}
