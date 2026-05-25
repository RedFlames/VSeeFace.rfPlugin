using HarmonyLib;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the PropImage class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(PropImage))]
    public static class Patch_PropImage
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropImage.StartDragging))]
        static void StartDragging_Postfix(PropImage __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropImage)}.{nameof(PropImage.StartDragging)}");
        }
    }

}
