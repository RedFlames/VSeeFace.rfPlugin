using System;
using System.Collections.Generic;
using HarmonyLib;
using rfPlugin.VSeeFace;
using UnityEngine;
using UnityEngine.UI;

namespace rfPlugin;

/*
  See RfPlugin.cs for the main class. These patches are part of it.
  Patches the PropImage class (of VSeeFace assembly)
*/
public partial class RfPlugin
{
        
    [HarmonyPatch(typeof(PropImage))]
    public static class Patch_PropImage
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropImage.StartDragging))]
        static void StartDragging_Postfix(PropImage __instance)
        {
            LogDebug($"-- {nameof(PropImage)}.{nameof(PropImage.StartDragging)}");
        }
    
    
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropImage.OnPointerDown))]
        static void OnPointerDown_Postfix(PropImage __instance)
        {
            LogDebug($"-- {nameof(PropImage)}.{nameof(PropImage.OnPointerDown)}");
        }
    
    
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void Start_Postfix(PropImage __instance)
        {
            LogDebug($"-- {nameof(PropImage)}.Start");

            // when a PropImage gets created, make it transparent right away
            var image = __instance.GetComponent<RawImage>();
            if (image)
            {
                var c = image.color;
                image.color = new(c.r, c.g, c.b, 0.1f);
            }
        }
    
        [HarmonyPostfix]
        [HarmonyPatch("LateUpdate")]
        static void LateUpdate_Postfix(PropImage __instance)
        {
            var log = true;
            var image = __instance.GetComponent<RawImage>();
            
            attachedBone = null;
            
            // make the prop image red transparent to indicate NO hit (as a reset before going down this method)
            if (image)
                image.color = new(1.0f, 0f, 0f, 0.05f);
            
            float distance;
            int hitVertex;
	        Renderer renderer;
            Transform hitBone;
            
            distance = MeshRaycasterWrapper.RaycastMesh(out hitVertex, out renderer, out hitBone);
            
            if (hitBone == null) return;

            if (log) LogDebug($"-- LATE UPDATE -- {attachedBone}");
            
            attachedBone = hitBone;
            
            // make the prop image white/neutral transparent to indicate a raycast hit has happened
            if (image)
                image.color = new(1f, 1f, 1f, 0.05f);

            if (log) LogDebug($"-- LATE UPDATE -- {targetProp}");

            if (targetProp == null)
            {
                targetProp = VSeeFaceHelper.SpawnProp(__instance.prop.spriteTexture);
                targetProp.attachedBone = attachedBone;
            }
            
            // all of this is taken from uhhh an EndDrag somewhere in VSeeFace

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mouseRayHit = mouseRay.origin + mouseRay.direction * distance;

            Ray propImageRay = Camera.main.ScreenPointToRay(__instance.transform.position);
            // vector from propImageRay to mouseRayHit but projected onto propImageRay? Why not go `distance` down this ray as well
            Vector3 propImageRayHit = propImageRay.origin + Vector3.Project(mouseRayHit - propImageRay.origin, propImageRay.direction);

            attachmentRay.origin = attachedBone.InverseTransformPoint(propImageRayHit);
            attachmentRay.direction = attachedBone.InverseTransformDirection(-propImageRay.direction);
            
            //var attachedPosition = attachmentRay.origin;
            //var attachedRotation = Quaternion.Inverse(attachedBone.rotation) * targetProp.transform.rotation;
            
            targetProp.transform.position = attachedBone.TransformPoint(attachmentRay.origin + attachmentRay.direction * .1f); 
            targetProp.transform.rotation = Quaternion.LookRotation(mouseRay.direction * -1f, Camera.main.transform.rotation * __instance.transform.up);

            //targetProp.transform.rotation = Quaternion.LookRotation(.LookAt(), Camera.main.transform.rotation * __instance.transform.up);
            //targetProp.transform.LookAt(attachedBone);
            
            // sprite scaling logic from VSF as well
            var spriteScale = new Vector3();
            float rayAngle = Vector3.Angle(to: Camera.main.ScreenPointToRay(__instance.RightPoint()).direction, from: propImageRay.direction);
            spriteScale.x = distance * Mathf.Tan(rayAngle * ((float)System.Math.PI / 180f)) * 2f * Mathf.Sign(__instance.transform.localScale.x);
            rayAngle = Vector3.Angle(to: Camera.main.ScreenPointToRay(__instance.TopPoint()).direction, from: propImageRay.direction);
            spriteScale.y = distance * Mathf.Tan(rayAngle * ((float)System.Math.PI / 180f)) * 2f * Mathf.Sign(__instance.transform.localScale.y);
            
            //sprite.rotation = Quaternion.LookRotation(attachmentRay.direction, targetProp.transform.up);
            Vector3 normalized = Vector3.ProjectOnPlane(attachmentRay.direction, __instance.transform.up).normalized;
            float f = Vector3.Dot(__instance.transform.forward, normalized);
            f = Mathf.Sign(f) * Mathf.Sqrt(Mathf.Abs(f));
            
            targetProp.sprite.localScale = Vector3.Scale(spriteScale, new Vector3(f, 1f, 1f));
            
            // unhide the spheres again and update pos / scale

            SpheresVisible = true;

            var spherePos = attachedBone.TransformPoint(attachmentRay.origin);
            
            RfPlugin.UpdateSpheres(spherePos, 0.4f * (spriteScale.x + spriteScale.y)/2f);
            
            //LogWarn($"-- LATE UPDATE -- {_Sphere} {_Sphere.transform.localScale:R} {spriteScale.x:R} {spriteScale.y} {spriteScale.z}");
        
        }
    }
    
}