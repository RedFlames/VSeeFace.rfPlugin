using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using rfPlugin.VSeeFace;
using UnityEngine;

namespace rfPlugin;

/*
  Partial class, because all the HarmonyX patches in ./RfPlugin.Patches/ are static sub-classes
  and the logic inside them is effectively part of this class, but also grouped by the particular
  VSeeFace classes being patched.
*/
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public partial class RfPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    // regular logging shorthands
    public static void Log(object data) => Logger.LogInfo(data);
    public static void LogDebug(object data) => Logger.LogDebug(data);
    public static void LogWarn(object data) => Logger.LogWarning(data);
    public static void LogError(object data) => Logger.LogError(data);
    // debug log Unity game objects + components
    public static void LogGameObject(string prefix, GameObject go) => Logger.LogDebug($"[{prefix}] go: <{go.FullPath()}> {go} [{go.GetType()}] {go.gameObject} [{go.gameObject.GetType()}]");
    public static void LogComponent(string prefix, Component co) => Logger.LogDebug($"[{prefix}] co: {co} [{co.GetType()}] {co.gameObject} [{co.gameObject.GetType()}]");
    
    // needed for applying the patches
    public Harmony Harmony { get; } = new(MyPluginInfo.PLUGIN_NAME);
    // The plugin probably acts like a singleton, right?
    public static RfPlugin Instance { get; private set; }
    
    // used by ./RfPlugin.Patches/Patch_PropImage
    public static Ray attachmentRay = new();
    public static Transform attachedBone = null;
    
    private static Prop targetProp;
    
    private static GameObject _Sphere = null;
    
    // init plugin when Unity instance of it "wakes up"
    private void Awake()
    {
        Instance = this;

        // Plugin startup logic
        Logger = base.Logger;
        Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        // searches for Harmony annotations in the entire assembly
        Harmony.PatchAll();
        
        // grabs references to Unity game objects of UI elements etc.
        VSeeFaceHelper.Init();
        
        // just some debugging log dumps to find VSeeFace stuff
        VSeeFaceHelper.DumpObjectsByName();
        VSeeFaceHelper.DebugLogObjects();
        
        // To see what the Detect shader looks like in practice.
        // Might be missing the material setup stuff from RaycastMesh until you start attaching a prop.
        //Camera.main.SetReplacementShader(Shader.Find("Custom/Detect"), "RenderType");
    
        // --- Add new right-menu button(s) ---
        VSeeFaceHelper.CreateMenuButton("test", btn_callback);
    }

    
    private void Update()
    {
        // A lot of plugin related things happen in the Harmony patches.
    }
    
    /*
        from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnRenderObject.html:
        - OnRenderObject is called after camera has rendered the Scene.
        - runs on for every GameObject with a script that uses this callback
    */
    public void OnRenderObject()
    {
        // only interested in the main camera that renders the VSF avatar
        if (Camera.current != Camera.main)
            return;
        
        // using https://github.com/loco-choco/GizmosLibraryPlugin sample code
        // seems like it's a BepInEx vresion of an OWML (Outer Wilds Mod Loader) plugin?...
        // I only know that I had trouble with Debug.DrawLine not working, probably should use LineRenderer or idk
        // I don't know much about Unity and I just wanted gizmos like in RuntimeUnityEditor, and this worked well so far :3

        // Sets the default material for gizmos
        GizmosLibraryPlugin.GizmosAPI.SetDefaultMaterialPass();
        
        // This draws a hourglass on the world origin
        /*GizmosLibraryPlugin.GizmosAPI.DrawOnGlobalReference(() =>
        {
            GizmosLibraryPlugin.GizmosAPI.DrawWireframeCone(0f, 1f, Vector3.zero, Vector3.up, Color.yellow, 12);
            GizmosLibraryPlugin.GizmosAPI.DrawWireframeCone(0f, 1f, Vector3.zero, -Vector3.up, Color.yellow, 12);
        });*/

        // draw some gizmos after/while a VSF prop is being attached
        if (_Sphere != null && attachedBone != null)
        {
            // frame of reference is the bone that the prop attaches to
            GizmosLibraryPlugin.GizmosAPI.DrawWithReference(attachedBone,() =>
            {
                // draw some junk at the attachmentRay :juh:
                //GizmosLibraryPlugin.GizmosAPI.DrawWireframeCapsule(0.1f, Vector3.forward + Vector3.up * 0.1f, Vector3.forward - Vector3.up * 0.1f, Color.cyan, 12);
                GizmosLibraryPlugin.GizmosAPI.DrawWireframeCapsule(0.02f, attachmentRay.origin, attachmentRay.origin + attachmentRay.direction * .02f, Color.yellow, 12);
                GizmosLibraryPlugin.GizmosAPI.DrawVector(attachmentRay.direction.normalized, .02f, attachmentRay.origin, Color.magenta);
                
                GizmosLibraryPlugin.GizmosAPI.DrawVector(Vector3.up, .02f, Vector3.zero, Color.cyan);
            });
        }
    }
    
    // just a lil guy for the new menu button
    public void btn_callback()
    {
        LogDebug($"listenerrrrrrrr");
    }
    
}
