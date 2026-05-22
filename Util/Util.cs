
using System;
using HarmonyLib;

namespace rfPlugin;

public static class StringExt
{
    // because uhh yea why can't I easily to string.Contains but case-insensitive
    public static bool IContains(this string str, string search) => str.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
}


/*
   The following two patches just kill some spammy things that show up in the BepInEx console
   and are annoying me with how much the console jumps around at startup :3
*/

[HarmonyPatch(typeof(OpenSee.OpenSeeWebcamInfo), nameof(OpenSee.OpenSeeWebcamInfo.ListCameraDetails))]
public static class Patch_OpenSeeWebcamInfo
{
    static void Prefix(OpenSee.OpenSeeWebcamInfo __instance, bool includeBlackMagic)
    {
        if (!RfPlugin.SuppressOtherLogSpam)
            return;
        
        if (OpenSee.OpenSeeWebcamInfo.dumpJsonStatic == true)
            RfPlugin.LogWarn("Suppressing OpenSee.OpenSeeWebcamInfo.ListCameraDetails JSON Dumps...");
        OpenSee.OpenSeeWebcamInfo.dumpJsonStatic = false;
    }
}
/*
[HarmonyPatch(typeof(Leap.Unity.LeapServiceProvider), "Update")]
public static class Patch_LeapServiceProvider
{
    private static bool logged = false;
    static bool Prefix(Leap.Unity.LeapServiceProvider __instance)
    {
        if (!RfPlugin.SuppressOtherLogSpam)
            return true;
        if(!logged)
            RfPlugin.LogWarn("Leap.Unity.LeapServiceProvider.Update suppressed through Prefix.");
        logged = true;
        return false;
    }
}*/
