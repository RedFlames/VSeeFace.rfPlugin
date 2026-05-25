using HarmonyLib;

using UnityEngine.EventSystems;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the Draggable class (of VSeeFace assembly)

  This one was JUST for debug logging to see if "checkActive" or "not" ever do anything at all
*/
public partial class VSeeFaceHelper
{
    // -- put these back in place to make this stuff run,
    // -- otherwise this is currently an inactive patch set
    //[HarmonyPatch(typeof(Draggable))]
    //public static class Patch_Draggable
    public class Patch_Draggable_Debug
    {
        // Looking around with ILSpy, the instance var "not" never gets assigned ever...
        // but I didn't trust Unity not to have something built somewhere that'll change it
        // where I couldn't see it maybe? But from these checks it seems like "not" is
        // always false
        public static bool prevNot = false;
        
        public static Draggable monitoring;
    
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.Start))]
        static void Start_Prefix(Draggable __instance)
        {
            if (monitoring == null)
            {
                monitoring = __instance;
                RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.Start)} -- monitoring {monitoring} now");
            }
            
            RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.Start)} -- {__instance.not} -- {__instance.checkActive == null}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.Start))]
        static void Start_Postfix(Draggable __instance)
        {
            RfPlugin.Log($"postfix -- {nameof(Draggable)}.{nameof(Draggable.Start)} -- {__instance.not} -- {__instance.checkActive == null}");
            prevNot = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.OnPointerDown))]
        static void OnPointerDown_Prefix(Draggable __instance, PointerEventData dt)
        {
            RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.OnPointerDown)} -- {__instance.not} -- {__instance.checkActive == null}");
            
            var i = __instance;
            // the condition from the original function as I see it in ILSpy
            if (!(i.checkActive != null) || ((i.checkActive.activeSelf || i.not) && (!i.checkActive.activeSelf || !i.not)))
            {
                
            }

            // if we assume that "not" always == false
            bool _if;
            
            _if = !(i.checkActive != null) || ((i.checkActive.activeSelf || false) && (!i.checkActive.activeSelf || true));
            
            _if = !(i.checkActive != null) || (i.checkActive.activeSelf && true);

            _if = !(i.checkActive != null) || i.checkActive.activeSelf;

            _if = i.checkActive == null || i.checkActive.activeSelf;
        
            // wait does checkActive ever get set to anything ever ... I think it's just always null and this is always true
            // these functions don't run anyways when your self isn't active, right
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.OnPointerDown))]
        static void OnPointerDown_Postfix(Draggable __instance, PointerEventData dt)
        {
            RfPlugin.Log($"postfix -- {nameof(Draggable)}.{nameof(Draggable.OnPointerDown)} -- {__instance.not} -- {__instance.checkActive == null}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.OnPointerUp))]
        static void OnPointerUp_Prefix(Draggable __instance)
        {
            RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.OnPointerUp)} -- {__instance.not} -- {__instance.checkActive == null}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.OnPointerUp))]
        static void OnPointerUp_Postfix(Draggable __instance)
        {
            RfPlugin.Log($"postfix -- {nameof(Draggable)}.{nameof(Draggable.OnPointerUp)} -- {__instance.not} -- {__instance.checkActive == null}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.Focus))]
        static void Focus_Prefix(Draggable __instance)
        {
            RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.Focus)} -- {__instance.not} -- {__instance.checkActive == null}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.Focus))]
        static void Focus_Postfix(Draggable __instance)
        {
            RfPlugin.Log($"postfix -- {nameof(Draggable)}.{nameof(Draggable.Focus)} -- {__instance.not} -- {__instance.checkActive == null}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.Update))]
        static void Update_Prefix(Draggable __instance)
        {
            if (__instance.not != prevNot)
                RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.Update)} -- {__instance.not} -- {__instance.checkActive == null}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.Update))]
        static void Update_Postfix(Draggable __instance)
        {
            if (__instance.not != prevNot)
                RfPlugin.Log($"postfix -- {nameof(Draggable)}.{nameof(Draggable.Update)} -- {__instance.not} -- {__instance.checkActive == null}");
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(Draggable.LateUpdate))]
        static void LateUpdate_Prefix(Draggable __instance)
        {
            if (__instance.not != prevNot)
                RfPlugin.Log($"prefix -- {nameof(Draggable)}.{nameof(Draggable.LateUpdate)} -- {__instance.not} -- {__instance.checkActive == null}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Draggable.LateUpdate))]
        static void LateUpdate_Postfix(Draggable __instance)
        {
            if (__instance.not != prevNot)
                RfPlugin.Log($"postfix -- {nameof(Draggable)}.{nameof(Draggable.LateUpdate)} -- {__instance.not} -- {__instance.checkActive == null}");
        
            prevNot = __instance.not;
        }
    
    
    }

}
