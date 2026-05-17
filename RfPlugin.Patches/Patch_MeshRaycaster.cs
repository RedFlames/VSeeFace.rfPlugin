using HarmonyLib;
using UnityEngine;

namespace rfPlugin;

/*
  See RfPlugin.cs for the main class. These patches are part of it.
  Patches the MeshRaycaster class (of VSeeFace assembly)
*/
public partial class RfPlugin
{
    private static bool sphWasActive = false;
    
    [HarmonyPatch(typeof(MeshRaycaster), nameof(MeshRaycaster.RaycastMesh))]
    public static class Patch_MeshRaycaster_RaycastMesh
    {
        static bool Prefix(MeshRaycaster __instance)
        {
            sphWasActive = SpheresVisible;
            
            SpheresVisible = false;

            return true;
        }
        
        static void Postfix(MeshRaycaster __instance)
        {
            SpheresVisible = sphWasActive;
        }
    }
}