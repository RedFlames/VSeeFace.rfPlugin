using UnityEngine;

namespace rfPlugin;

public static class UnityHelper
{
    public static GameObject CreateTransparentSphere(Color? color = null, float alpha = .5f)
    {
        Color col = new(1.0f, 1.0f, 1.0f, alpha);
        if (color.HasValue)
            col = color.Value;
        
        col.a = alpha;
        
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                
        var renderer = sphere.GetComponent<MeshRenderer>();

        renderer.material = new Material(Shader.Find("UI/Default")) { color = col };

        return sphere;
        //Material nMat = new Material(targetProp.GetComponentInChildren<MeshRenderer>().material.shader);
        //Material nMat = new Material(Shader.Find("Custom/TransparentDiffuse"));
        //Material nMat = new Material(Shader.Find("VRM/MToon"));
        //Material nMat = renderer.material;
        //nMat.shader = Shader.Find("VRM/MToon");
        
        // how the hell transparency in Unity??? wha
        //nMat.renderQueue = 2500;
        //nMat.SetFloat("_Mode", 3);
        //nMat.SetOverrideTag("RenderType", "Transparent");

        //nMat.SetColor("_Color", new(1.0f, .5f, .5f, 0.1f));
        //nMat.SetColor("Main Color", new(0f, .5f, .5f, 0.1f));

        //nMat.SetTexture("Texture", Resources.Load<Texture2D>("obimaterials/particle"));
        //nMat.SetTexture("_MainTex", Resources.Load<Texture2D>("gui/profiler_bck"));
    }
    
    public static void DrawRect2D (RectTransform rt, Color c, Vector3 offset = new())
    {
        Vector3[] corners = [new(), new(), new(), new()];
        rt.GetLocalCorners(corners);
        
        for (int i = 0; i < 4; i++)
        {
            corners[i] = corners[i] * 1.666f; // don't ask me why
        }
        
        Vector3 pos = rt.position + offset * 1.666f;
        
        DrawLine2D(corners[0], corners[1], c, pos);
        DrawLine2D(corners[2], corners[1], c, pos);
        DrawLine2D(corners[2], corners[3], c, pos);
        DrawLine2D(corners[0], corners[3], c, pos);
    }

    public static void DrawLine2D (Vector3 a, Vector3 b, Color col, Vector3 offset = new())
    {
        var rayA = Camera.main.ScreenPointToRay(a + offset);
        
        Vector3 worldA = rayA.origin + rayA.direction * .01f;
        
        var rayB = Camera.main.ScreenPointToRay(b + offset);
        
        Vector3 worldB = rayB.origin + rayB.direction * .01f;
        
        Vector3 worldAtoB = worldB - worldA;
        
        GizmosLibraryPlugin.GizmosAPI.DrawOnGlobalReference(() =>
        {
            GizmosLibraryPlugin.GizmosAPI.DrawVector(worldAtoB, .0f, worldA, col);
        });
    }
}