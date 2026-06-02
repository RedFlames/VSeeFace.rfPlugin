using HarmonyLib;

using UnityEngine;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the SpoutExport class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(SpoutExport))]
    public static class Patch_SpoutExport
    {
        private static bool dontRecurse = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpoutExport.SetSpoutActive))]
        static bool SetSpoutActive_Prefix(SpoutExport __instance, bool spoutActive)
        {
            if (dontRecurse)
            {
                RfPlugin.LogWarn($"Preventing prefix recursion {nameof(SpoutExport)}.{nameof(SpoutExport.SetSpoutActive)}");
                return true;
            }
            
            if (!RfPlugin.AlwaysTopPropsCam)
            {
                RfPlugin.LogWarn($"Skipping override of {nameof(SpoutExport)}.{nameof(SpoutExport.SetSpoutActive)}");
                return true;
            } else
            {
                RfPlugin.LogWarn($"Overriding {nameof(SpoutExport)}.{nameof(SpoutExport.SetSpoutActive)}");
            }

            Camera currentCam = __instance.cam;
            
            dontRecurse = true;
            __instance.cam = RfPlugin.AlwaysTopPropsCam;
            __instance.SetSpoutActive(spoutActive);

            __instance.cam = currentCam;
            dontRecurse = false;

            RfPlugin.LogWarn($"Overriding {nameof(SpoutExport)}.{nameof(SpoutExport.SetSpoutActive)} done.");
            
            return true;
        }


        
    }

}
