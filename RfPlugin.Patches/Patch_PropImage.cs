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
            MeshRenderer sphRenderer = null;
            var dragged = Traverse.Create(__instance.prop).Field<bool>("dragged").Value;
            
            // hide sphere from ray
            if (dragged && _Sphere != null)
            {
                _Sphere.SetActive(false);
                sphRenderer = _Sphere.GetComponent<MeshRenderer>();
                if (sphRenderer != null)
                    sphRenderer.enabled = false;
            }
            
            attachedBone = null;

            var log = false;
            
            // make the prop image red transparent to indicate NO hit (as a reset before going down this method)
            var image = __instance.GetComponent<RawImage>();
            if (image)
            {
                var c = image.color;
                image.color = new(1.0f, 0f, 0f, 0.1f);
            }
            
            int hitVertex;
            float distance;

	        // TODO kill section below because it's in MeshRaycasterWrapper now

	        Renderer renderer = MeshRaycaster.Singleton.RaycastMesh(out hitVertex, out distance);
            
            if (log) LogDebug($"-- LATE UPDATE -- {renderer} {renderer is MeshRenderer} {renderer is SkinnedMeshRenderer}");
            
            
            if (renderer == null)
            {
                return;
            }
            
            if (renderer is MeshRenderer)
            {
                attachedBone = renderer.transform;
            }
            else if (renderer is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                if (hitVertex < skinnedMeshRenderer.sharedMesh.vertexCount)
                {
                    
                    
                    
                    BoneWeight boneWeight = skinnedMeshRenderer.sharedMesh.boneWeights[hitVertex];
                    int num = boneWeight.boneIndex0;
                    if (boneWeight.weight1 > boneWeight.weight0 && boneWeight.weight1 > boneWeight.weight2 && boneWeight.weight1 > boneWeight.weight3)
                    {
                        num = boneWeight.boneIndex1;
                    }
                    else if (boneWeight.weight2 > boneWeight.weight0 && boneWeight.weight2 > boneWeight.weight1 && boneWeight.weight2 > boneWeight.weight3)
                    {
                        num = boneWeight.boneIndex2;
                    }
                    else if (boneWeight.weight3 > boneWeight.weight0 && boneWeight.weight3 > boneWeight.weight1 && boneWeight.weight3 > boneWeight.weight2)
                    {
                        num = boneWeight.boneIndex3;
                    }
                    
                    
                    
                    if (num < skinnedMeshRenderer.bones.Length)
                    {
                        attachedBone = skinnedMeshRenderer.bones[num];
                    }
                }
            }

            // TODO kill above
            
            float wdist = MeshRaycasterWrapper.RaycastMesh(out int wHitVertex, out Renderer wHitRenderer, out Transform wHitBone);

            if (Math.Abs(wdist - distance) > 0.000001f || wHitVertex != hitVertex || wHitRenderer != renderer || wHitBone != attachedBone)
                LogWarn($" MeshRaycasterWrapper failure!!!!!!! {distance} {wdist} {hitVertex} {wHitVertex} {renderer} {wHitRenderer} {attachedBone} {wHitBone}");
            
            if (log) LogDebug($"-- LATE UPDATE -- {attachedBone}");
            
            if (attachedBone == null) return;
            
            // make the prop image white/neutral transparent to indicate a raycast hit has happened
            if (image)
            {
                var c = image.color;
                image.color = new(1f,1f,1f, 0.1f);
            }

            if (log) LogDebug($"-- LATE UPDATE -- {targetProp}");

            if (targetProp == null)
            {
                targetProp = PropManager.Singleton.CreateProp(__instance.prop.spriteTexture,  new List<Prop.ImageDelay>());
                
                Traverse.Create(targetProp).Field<Transform>("attachedBone").Value = attachedBone;
		        //targetProp.propImage.transform.position = __instance.prop.transform.position;
                targetProp.propImage.gameObject.SetActive(false);
                var sprI = Traverse.Create(__instance.prop).Field<Transform>("sprite").Value;
                var sprT = Traverse.Create(targetProp).Field<Transform>("sprite").Value;
                sprT.localScale = sprI.localScale;
                var mr = targetProp.GetComponentInChildren<MeshRenderer>();
                mr.material.SetInt("_ZTest", 0);
		        mr.material.renderQueue = 5000;
                mr.material.mainTextureScale = new(.5f, .5f);
                LogDebug($"-- LATE UPDATE -- {targetProp} {sprT} {sprT.localScale} {mr} {mr.material}");
                //mr.enabled = false;
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
            var sprite = Traverse.Create(targetProp).Field<Transform>("sprite").Value;
            var spriteScale = new Vector3();
            float rayAngle = Vector3.Angle(to: Camera.main.ScreenPointToRay(__instance.RightPoint()).direction, from: propImageRay.direction);
            spriteScale.x = distance * Mathf.Tan(rayAngle * ((float)System.Math.PI / 180f)) * 2f * Mathf.Sign(__instance.transform.localScale.x);
            rayAngle = Vector3.Angle(to: Camera.main.ScreenPointToRay(__instance.TopPoint()).direction, from: propImageRay.direction);
            spriteScale.y = distance * Mathf.Tan(rayAngle * ((float)System.Math.PI / 180f)) * 2f * Mathf.Sign(__instance.transform.localScale.y);
            
            //sprite.rotation = Quaternion.LookRotation(attachmentRay.direction, targetProp.transform.up);
            Vector3 normalized = Vector3.ProjectOnPlane(attachmentRay.direction, targetProp.transform.up).normalized;
            float f = Vector3.Dot(targetProp.transform.forward, normalized);
            f = Mathf.Sign(f) * Mathf.Sqrt(Mathf.Abs(f));
            sprite.localScale = Vector3.Scale(spriteScale, new Vector3(f, 1f, 1f));
            
            // the s p h e r e (just testing stuff)
            if (_Sphere == null)
            {
                _Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                sphRenderer = _Sphere.GetComponent<MeshRenderer>();
                Material nMat = new Material(targetProp.GetComponentInChildren<MeshRenderer>().material.shader);
                //Material nMat = new Material(Shader.Find("Custom/TransparentDiffuse"));
                //Material nMat = new Material(Shader.Find("VRM/MToon"));
                //Material nMat = sphRenderer.material;
                //nMat.shader = Shader.Find("VRM/MToon");

                // how the hell transparency in Unity??? wha
                nMat.renderQueue = 2500;
                nMat.SetFloat("_Mode", 3);
                nMat.SetOverrideTag("RenderType", "Transparent");

                nMat.color = new(0.0f, 0.0f, 1.0f, 0.1f);
                nMat.SetColor("_Color", new(1.0f, .5f, .5f, 0.1f));
                nMat.SetColor("Main Color", new(0f, .5f, .5f, 0.1f));

                nMat.SetTexture("Texture", Resources.Load<Texture2D>("obimaterials/particle"));
                nMat.SetTexture("_MainTex", Resources.Load<Texture2D>("obimaterials/particle"));
                sphRenderer.material = nMat;

            }
            
            _Sphere.SetActive(true);
            
            // unhide the sphere again
            sphRenderer = _Sphere.GetComponent<MeshRenderer>();
            if (sphRenderer != null)
                sphRenderer.enabled = true;
            

            _Sphere.transform.position = targetProp.transform.position;
            _Sphere.transform.localScale = Vector3.Scale(spriteScale, new Vector3(f, 1f, 1f)) * .5f;

        }
    }
    
}