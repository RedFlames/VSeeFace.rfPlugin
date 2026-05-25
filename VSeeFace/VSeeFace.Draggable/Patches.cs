using HarmonyLib;

using UnityEngine;
using UnityEngine.EventSystems;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the Draggable class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(Draggable))]
    public static class Patch_Draggable
    {
    
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.Start))]
        static void Start_Prefix(Draggable __instance)
        {
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.Start))]
        static void Start_Postfix(Draggable __instance)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.OnPointerDown))]
        static void OnPointerDown_Prefix(Draggable __instance, PointerEventData dt)
        {
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.OnPointerDown))]
        static void OnPointerDown_Postfix(Draggable __instance, PointerEventData dt)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.OnPointerUp))]
        static void OnPointerUp_Prefix(Draggable __instance)
        {
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.OnPointerUp))]
        static void OnPointerUp_Postfix(Draggable __instance)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.Focus))]
        static void Focus_Prefix(Draggable __instance)
        {
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.Focus))]
        static void Focus_Postfix(Draggable __instance)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.Update))]
        static void Update_Prefix(Draggable __instance)
        {
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.Update))]
        static void Update_Postfix(Draggable __instance)
        {
        }
        
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.LateUpdate))]
        static void LateUpdate_Prefix(Draggable __instance)
        {
            var i = __instance;
            // duplicating logic from Draggable.LateUpdate
            if (!Draggable.escapePressed && i.target.GetSiblingIndex() == i.target.parent.childCount - 1 && Input.GetKeyUp(KeyCode.Escape))
            {
                RfPlugin.LogDebug($"Checking if draggable is prop settings: {i.GetInstanceID()} {MainPropSettingsWindow.GetComponent<Draggable>().GetInstanceID()}");
                // BUGFIX: The show vs. hide Prop Settings button in the Props Window wasn't updating when closing Prop Settings via Escape key.
                if (i == MainPropSettingsWindow.GetComponent<Draggable>())
                {
                    MainUI.propsWindow.showPropSettingsButton.SetActive(true);
                    MainUI.propsWindow.hidePropSettingsButton.SetActive(false);
                }
            }
        
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.LateUpdate))]
        static void LateUpdate_Postfix(Draggable __instance)
        {
        }
    
    }

}
