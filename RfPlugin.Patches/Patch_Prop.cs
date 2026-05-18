using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rfPlugin;

/*
  See RfPlugin.cs for the main class. These patches are part of it.
  Patches the Prop class (of VSeeFace assembly)
*/
public partial class RfPlugin
{

    // TODO: Let VSeeFaceHelper patches handle a lot of spawned props stuff (targetProp)
        
    [HarmonyPatch(typeof(Prop))]
    public static class Patch_Prop
    {
        [HarmonyPostfix]
        [HarmonyPatch("LateUpdate")]
        static void LateUpdate_Postfix(Prop __instance)
        {
            /*if (targetProp != null && __instance == targetProp)
            {
                targetProp.gameObject.SetActive(true);
            }*/
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.EndDrag))]
        static bool EndDrag_Prefix(Prop __instance)
        {
            if(targetProp != null && __instance == targetProp)
                return false;
            return true;
        }

        // TODO ------ EndDrag is just wrong about bone weights --------- !!!!!!
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.EndDrag))]
        static void EndDrag_Postfix(Prop __instance)
        {
            LogDebug($"-- {nameof(Prop)}.{nameof(Prop.EndDrag)}");

            if (targetProp != null)
                targetProp.gameObject.SetActive(false);


        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.StartDrag))]
        static bool StartDrag_Prefix(Prop __instance)
        {
            if(targetProp != null && __instance == targetProp)
            {
                targetProp.gameObject.SetActive(true);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.StartDrag))]
        static void StartDrag_Postfix(Prop __instance)
        {
            LogDebug($"-- {nameof(Prop)}.{nameof(Prop.StartDrag)}");
            LogDebug($"-- {Input.GetKey(KeyCode.LeftShift)} -- {Input.GetKey(KeyCode.LeftControl)}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.OnBeginDrag))]
        static bool OnBeginDrag_Prefix(Prop __instance)
        {
            if(targetProp != null && __instance == targetProp)
            {
                targetProp.gameObject.SetActive(true);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.OnBeginDrag))]
        static void OnBeginDrag_Postfix(Prop __instance, PointerEventData eventData)
        {
            LogDebug($"-- {nameof(Prop)}.{nameof(Prop.OnBeginDrag)}");
            LogDebug($"-- {Input.GetKey(KeyCode.LeftShift)} -- {Input.GetKey(KeyCode.LeftControl)}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.OnDrag))]
        static bool OnDrag_Prefix(Prop __instance)
        {
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.OnDrag))]
        static void OnDrag_Postfix(Prop __instance, PointerEventData eventData)
        {
            LogDebug($"-- {nameof(Prop)}.{nameof(Prop.OnDrag)}");
            LogDebug($"-- {Input.GetKey(KeyCode.LeftShift)} -- {Input.GetKey(KeyCode.LeftControl)}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.OnEndDrag))]
        static bool OnEndDrag_Prefix(Prop __instance)
        {
            if(targetProp != null && __instance == targetProp)
            {
                targetProp.gameObject.SetActive(false);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.OnEndDrag))]
        static void OnEndDrag_Postfix(Prop __instance, PointerEventData eventData)
        {
            LogDebug($"-- {nameof(Prop)}.{nameof(Prop.OnEndDrag)}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Prop.OnPointerDown))]
        static bool OnPointerDown_Prefix(Prop __instance)
        {
            if(targetProp != null && __instance == targetProp)
                return false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Prop.OnPointerDown))]
        static void OnPointerDown_Postfix(PointerEventData eventData)
        {
            LogDebug($"-- {nameof(Prop)}.{nameof(Prop.OnPointerDown)}");
        }
    }
    
}