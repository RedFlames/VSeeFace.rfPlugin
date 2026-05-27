using HarmonyLib;

using UnityEngine;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the Prop class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    
    [HarmonyPatch(typeof(Prop))]
    public static class Patch_Prop
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.Update))]
        static void Update_Prefix(Prop __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(Prop)}.{nameof(Prop.Update)}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.Update))]
        static void Update_Postfix(Prop __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(Prop)}.{nameof(Prop.Update)}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.LateUpdate))]
        static void LateUpdate_Postfix(Prop __instance)
        {
            var iID = __instance.GetInstanceID();
            if (!prevSettingsExt.ContainsKey(iID) || !currentSettingsExt.ContainsKey(iID))
            {
                RfPlugin.LogWarn($"Prop LateUpdate did not find {iID} in prevSettingsExt or currentSettingsExt");
                return;
            }
            
            if (prevSettingsExt[iID].transparency != currentSettingsExt[iID].transparency)
            {
                SetPropTransparency(__instance);
            }
            prevSettingsExt[iID] = currentSettingsExt[iID];
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.Init))]
        static void Init_Postfix(Prop __instance)
        {
            var iID = __instance.GetInstanceID();
            
            var mr = __instance.GetComponentInChildren<MeshRenderer>();

            Texture2D texture = __instance.spriteTexture;
            if (__instance.spriteTexture == null)
            {
                texture = __instance.animatedTextures[0].texture;
            }
            __instance.spriteMaterial = new Material(Shader.Find("UI/Default")) { color = new(1.0f, 1.0f, 1.0f, 1.0f) };
            __instance.spriteMaterial.SetTexture("_MainTex", texture);
        
            mr.material = __instance.spriteMaterial;
            
        }
    

    }

    public static void SetPropTransparency(Prop prop)
    {
        var iID = prop.GetInstanceID();
        var transp = currentSettingsExt[iID].transparency;
        RfPlugin.LogWarn($"Setting transparency of Prop {iID} to {transp}");

        // TODO or FIXME or IDK: check if animated textures updating works fine with original logic + transp
        
        Color col = new(1.0f, 1.0f, 1.0f, transp/100f);

        prop.spriteMaterial.color = col;
    
    }

}
