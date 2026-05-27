using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the PropManager class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    public static PropManager PropManInstance => PropManager.Singleton;

    // PropSettings is part of Prop, but this might make more sense here
    public struct PropSettingsExtended
    {
        public float transparency;
        
        public PropSettingsExtended(float tp)
        {
            transparency = tp;
        }
    }
    
    // per Prop instance state
    public static Dictionary<int, PropSettingsExtended> currentSettingsExt = [];
    public static Dictionary<int, PropSettingsExtended> prevSettingsExt = [];
    
    public static PropSettingsExtended baseSettingsExt = new PropSettingsExtended(100f);

    public static float SettingsTransparency
    {
        get
        {
            return baseSettingsExt.transparency;
        }
        set
        {
            //RfPlugin.LogError($"SETTING baseSettingsExt.transparency -- {value}");
            baseSettingsExt.transparency = value;
            if (PropManInstance.selectedProp != null)
            {
                currentSettingsExt[PropManInstance.selectedProp.GetInstanceID()] = baseSettingsExt;
            }
        }
    }

    [HarmonyPatch(typeof(PropManager))]
    public static class Patch_PropManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PropManager.CreateProp))]
        static void CreateProp_Prefix(PropManager __instance, Texture2D texture, List<Prop.ImageDelay> animatedTextures)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropManager)}.{nameof(PropManager.CreateProp)}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropManager.CreateProp))]
        static Prop CreateProp_Postfix(Prop __result)
        {
            RfPlugin.Log($"{nameof(PropManager)}.{nameof(PropManager.CreateProp)} created {__result.GetInstanceID()}");
            
            currentSettingsExt[__result.GetInstanceID()] = baseSettingsExt;
            prevSettingsExt[__result.GetInstanceID()] = baseSettingsExt;
            
            SetPropTransparency(__result);
            
            return __result;
        }
    
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PropManager.SelectProp))]
        static void SelectProp_Prefix(PropManager __instance, Prop prop)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropManager)}.{nameof(PropManager.CreateProp)}");
            
            if (!currentSettingsExt.ContainsKey(prop.GetInstanceID()))
            {
                RfPlugin.LogWarn($"Could not find selected prop {prop.GetInstanceID()} in currentSettingsExt!");
                return;
            }
            
            //RfPlugin.LogError($"SELECTED NEW PROP NOW -- {prop.GetInstanceID()}");
            baseSettingsExt = currentSettingsExt[prop.GetInstanceID()];
        }


    }

}
