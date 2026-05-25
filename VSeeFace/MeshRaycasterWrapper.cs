using System;
using System.Linq;

using UnityEngine;

namespace rfPlugin.VSeeFace;

/*
  A wrapper for VSeeFace's MeshRaycaster that includes some of the logic from Prop.EndDrag
  to find the attachment bone and stuff.
*/
public class MeshRaycasterWrapper
{

    public static float RaycastMesh(out int hitVertex, out Renderer hitRenderer, out Transform hitBone)
{
        hitVertex = -1;
        hitBone = null;

        float distance;
        hitRenderer = MeshRaycaster.Singleton.RaycastMesh(out hitVertex, out distance);

        var log = false;
        if (log) RfPlugin.LogDebug($"-- MeshRaycasterWrapper -- {hitRenderer} {hitRenderer is MeshRenderer} {hitRenderer is SkinnedMeshRenderer}, v={hitVertex}, d={distance}");

        if (hitRenderer == null || distance < 0)
        {
            return -1f;
        }
        
        if (hitRenderer is MeshRenderer)
        {
            hitBone = hitRenderer.transform;
        }
        else if (hitRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (hitVertex < skinnedMeshRenderer.sharedMesh.vertexCount)
            {
                BoneWeight bW = skinnedMeshRenderer.sharedMesh.boneWeights[hitVertex];

                // me when I turn 9 greater-than weight comparisons into collection expression linq madness
                int hitBoneIdx = new []  { bW.boneIndex0, bW.boneIndex1, bW.boneIndex2, bW.boneIndex3 }
                                .Zip(    [ bW.weight0,    bW.weight1,    bW.weight2,    bW.weight3    ], (idx, w) => new {idx, w})
                                .Aggregate((ag, el) => el.w > ag.w ? el : ag).idx;
                
                // an alert for when the logical mistake in Prop.EndDrag would've happened.
                if ((bW.weight1 == bW.weight2 && bW.weight1 > Math.Max(bW.weight0, bW.weight3)) ||
                    (bW.weight1 == bW.weight3 && bW.weight1 > Math.Max(bW.weight0, bW.weight2)) ||
                    (bW.weight3 == bW.weight2 && bW.weight3 > Math.Max(bW.weight0, bW.weight1)))
                    RfPlugin.LogError($"-- MeshRaycasterWrapper -- orig would be wrong!!!! {new []  { bW.weight0,    bW.weight1,    bW.weight2,    bW.weight3 }}");

                if (hitBoneIdx < skinnedMeshRenderer.bones.Length)
                {
                    hitBone = skinnedMeshRenderer.bones[hitBoneIdx];
                }
            }
        }

        return distance;
    }
}
