using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace rfPlugin.VSeeFace;

/*
  See VSeeFaceHelper.cs for the main class. These patches are part of it.
  Patches the PropButton class (of VSeeFace assembly)
*/
public partial class VSeeFaceHelper
{
    [HarmonyPatch(typeof(PropButton))]
    public static class Patch_PropButton
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropButton.OnBeginDrag))]
        static void OnBeginDrag_Postfix(PropButton __instance)
        {
            // RfPlugin.Log($"is this thing on -- {nameof(PropButton)}.{nameof(PropButton.OnBeginDrag)}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void Start_Postfix(PropButton __instance)
        {
            RfPlugin.Log($"Postfix -- {nameof(PropButton)}.Start");
            
            PropButtonWrapper wrapped = new(__instance);
            if(wrapped == null || wrapped.Wrapped == null || wrapped.Parent == null)
            {
                RfPlugin.LogError($"PropButton.Start Postfix patch failed to wrap instance! {__instance.GetInstanceID()}");
                return;
            }
            PropButtons.Add(wrapped);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PropButton.Remove))]
        static void Remove_Prefix(PropButton __instance)
        {
            RfPlugin.Log($"Prefix -- {nameof(PropButton)}.{nameof(PropButton.Remove)}");
            var item = PropButtons.First(wrap => wrap.Wrapped == __instance);
            if (item == null)
            {
                RfPlugin.LogError($"PropButton.Remove Prefix patch failed to find in list! {__instance.GetInstanceID()}");
                return;
            }
            PropButtons.Remove(item);
        }
        
        /*
        This doesn't work but idk why :3

        [HarmonyPostfix]
        [HarmonyPatch("DownloadImage")]
        static IEnumerator CoroutineWrapper(IEnumerator __result)
        {
            // idk how I'd access the PropButton __instance during this, so I'll have to figure out transpiler for now
            // ... After writing the transpiler, I think I could've also accessed ""<>4__this" on this enumerator maybe?

            //RfPlugin.LogError($"uhh where am i {__result} {__result.Current}");
            yield return new WaitForSeconds(5);
            
            // Run original enumerator code
            while (__result.MoveNext())
                yield return __result.Current;
        }
        */
        

        /*
          This patch essentially acts as if there was this at the 
          start of the original PropButton.DownloadImage coroutine:

              if(imagePath.IsNullOrWhiteSpace())
                  yield break;
          
          idk if this could "race condition" with imagePath being set after the prop button gets created. But no,
          imagePath gets set at instantiation, and the coroutine only starts in Start.
          
          ...I could've just prefixed Start and left out the coroutine and returned false from prefix, huh...
        */
        
        [HarmonyTranspiler]
        [HarmonyPatch("DownloadImage", MethodType.Enumerator)]
        static IEnumerable<CodeInstruction> DownloadImage_TranspileMoveNext(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // put a label on what used to be the start of the generated MoveNext
            Label keepGoing = il.DefineLabel();
            instructions.First().labels.Add(keepGoing);
            
            // get access to the field in the generated coroutine that holds the ref to "this" PropButton
            var coroutine = AccessTools.FirstInner(typeof(PropButton), t => t.Name.Contains("<DownloadImage>"));
            var coroutine_this = coroutine.GetField("<>4__this");
            
            // load the PropButton ref to hand to delegate func
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, coroutine_this);
            
            // delegate takes PropButton ref from stack and puts a bool
            yield return Transpilers.EmitDelegate<Func<PropButton, bool>>(instance =>
            {
                //RfPlugin.LogError($"uhh where am i {instance}");

                // checks whether or not the imagePath is unusable and would cause error
                bool isInvalidPath = instance.imagePath.IsNullOrWhiteSpace();
                
                // hide the empty gray image for now
                if (isInvalidPath && instance.GetComponent<RawImage>() is RawImage img)
                    img.enabled = false;
                
                return isInvalidPath;
            });
            // if imagePath is not a problem (hopefully), resume executing original method
            yield return new CodeInstruction(OpCodes.Brfalse_S, keepGoing);
            // otherwise return false, ending the coroutine
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Ret);
            
            // rest of the instructions unchanged
            foreach(var i in instructions)
            {
                yield return i;
            }
        }
    
    }

}
