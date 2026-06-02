using HarmonyLib;

using UnityEngine;

using VSeeFace_Utility = Utility;

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
            //__instance.spriteMaterial = new Material(Shader.Find("Standard")) { color = new(1.0f, 1.0f, 1.0f, 1.0f) };
            /*
            // ugh lol I can't be bothered with this... 
            var material = new Material(Shader.Find("VRM10/MToon10")) { color = new(1.0f, 1.0f, 1.0f, 1.0f) };
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0.0f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            __instance.spriteMaterial = material;*/

            __instance.spriteMaterial.SetTexture("_MainTex", texture);
            
            mr.material = __instance.spriteMaterial;
            
        }
    
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.UpdateOnTop))]
        static bool UpdateOnTop_Prefix(Prop __instance, bool onTop)
        {
            //RfPlugin.Log($"is this thing on -- {nameof(Prop)}.{nameof(Prop.UpdateOnTop)}");
            
            // TODO: Could have an option to set another alpha value for on top vs. regular, so that obscured parts get their own alpha...
            // i.e. kinda x-ray style like the debug gizmos atm. Except would need to duplicate Sprite and possibly another camera(?)
            // so leaving this for later
            if (onTop)
            {
                //__instance.gameObject.layer = 21;
                VSeeFace_Utility.SetLayerRecursively(__instance.gameObject, RfPlugin.onTopPropsLayer);
            } else
            {
                //__instance.gameObject.layer = 14;
                VSeeFace_Utility.SetLayerRecursively(__instance.gameObject, LayerMask.NameToLayer("Props"));
            }
            return false;
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
