using System.Collections.Generic;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using rfPlugin.VSeeFace;

using UnityEngine;
using UnityEngine.UI;

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
    
    // see patches in Util.cs, just cleaning up the BepInEx console while debugging
    public static bool SuppressOtherLogSpam { get; private set; } = true;

    // needed for applying the patches
    public Harmony Harmony { get; } = new(MyPluginInfo.PLUGIN_NAME);

    // The plugin probably acts like a singleton, right?
    public static RfPlugin Instance { get; private set; }
    
    // used by ./RfPlugin.Patches/Patch_PropImage
    public static Ray attachmentRay = new();
    public static Transform attachedBone = null;
    
    private static Prop targetProp;

    public static PropWindow ExtraPropWindow { get; private set; }
    public static PropSettingsWindow ExtraSettingsWindow { get; private set; }
    
    public static bool AdvancedPropSettingsVisible { 
        get { return _advancedPropSettingsVisible; }
        set
        {
            // auto-implemented properties ftw... nvm I'm not on a Lang version where I can use "field"...
            ToggleAdvancedPropSettings(value);
        }
    }
    private static bool _advancedPropSettingsVisible = false;

    public static List<GameObject> AdvancedPropSettings = [];
    
    private static List<GameObject> _Spheres = [];
    
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

        ExtraPropWindow = VSeeFaceHelper.CreateNewPropWindow();

        ExtraSettingsWindow = VSeeFaceHelper.CreateNewSettingsWindow();
        
        // just some debugging log dumps to find VSeeFace stuff
        //VSeeFaceHelper.DumpObjectsByName();
        //VSeeFaceHelper.DebugLogObjects();
        
        // To see what the Detect shader looks like in practice.
        // Might be missing the material setup stuff from RaycastMesh until you start attaching a prop.
        //Camera.main.SetReplacementShader(Shader.Find("Custom/Detect"), "RenderType");
        
        for (int i = 0; i < 5; i++)
            _Spheres.Add(UnityHelper.CreateTransparentSphere(alpha: .05f + .1f * i));
    
        // --- Add new right-menu button(s) ---
        var btn = VSeeFaceHelper.CreateMenuButton("test", 3).OnClick(btn_callback);
        var btn2 = VSeeFaceHelper.CreateMenuButton("test2").OnClick(btn_callback2);
        
        var btn3 = VSeeFaceHelper.CreatePropSetting<Button>("Advanced Settings").OnClick(advanced_settings_callback);
        var testslider = VSeeFaceHelper.CreatePropSetting<Slider>("test4").OnSlide(slider_callback);
        AddAdvancedPropSetting(testslider);
        
        //var slider = btn4.GetComponentInChildren<Slider>();
        var adv_btn = VSeeFaceHelper.CreatePropSetting<Button>("Some button").OnClick(btn_callback3);
        AddAdvancedPropSetting(adv_btn);
        
        //btn4 = VSeeFaceHelper.MainUI.propSettings.TransChildren().First(ch => ch.GetChildWithComponent<Slider>());
        //slider = btn4.GetComponentInChildren<Slider>();

        var btn5 = VSeeFaceHelper.CreatePropSetting<Button>("test3", ExtraSettingsWindow).OnClick(btn_callback3);
        var btn6 = VSeeFaceHelper.CreatePropSetting<Slider>("test4", ExtraSettingsWindow).OnSlide(slider_callback2);    
    }
    
    private static bool _spheresVisible = false;
    public static bool SpheresVisible {
        get
        {
            return _spheresVisible;
        }
        set
        {
            if (value != _spheresVisible)
                ToggleSpheres(value);
        }
    }
    
    public static void ToggleSpheres(bool active = true)
    {
        _spheresVisible = active;
        foreach (var sphere in _Spheres)
        {
            sphere.SetActive(active);
            if (sphere.GetComponent<MeshRenderer>() is MeshRenderer mr)
                mr.enabled = active;
        }
    }
    
    public static void UpdateSpheres(Vector3 pos, float locScale = 1f) => UpdateSpheres(pos, new Vector3(locScale, locScale, locScale));

    public static void UpdateSpheres(Vector3 pos, Vector3 locScale)
    {
        int i = 1;
        foreach (var sphere in _Spheres)
        {
            var factor = 2f / i;
            //var factor = .1f * i;
            sphere.transform.position = pos;
            sphere.transform.localScale = locScale * factor;
            
            i++;
        }
    }
    
    private void Update()
    {
        // A lot of plugin related things happen in the Harmony patches.

        // TODO: make this happen in a PropWindow.Update patch or something
        var title = VSeeFaceHelper.MainPropWindow.transform.Find("Title");
        if (title && title.GetComponent<Text>() is Text textUI)
        {
            textUI.text = $"Props ({VSeeFaceHelper.PropButtons.Count})";
        } else
        {
            // log warn once?
        }
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
        
        // draw some gizmos after/while a VSF prop is being attached
        if (attachedBone != null)
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
    
        var drags = Resources.FindObjectsOfTypeAll<Draggable>();
        foreach (var drag in drags)
        {
            if (drag.isMouseDown)
            {
                RectTransform prt = drag.transform.GetComponent<RectTransform>();
                
                Vector2 size = new(prt.rect.width, prt.rect.height);
                UnityHelper.DrawRect2D(prt, Color.red, new(size.x, 0f, 0f));
                UnityHelper.DrawRect2D(prt, Color.red, new(0f, size.y, 0f));
                
                int i = 1;
                foreach (var c in drag.gameObject.TransChildren())
                {
                    RectTransform rt = c.transform.GetComponent<RectTransform>();
                
                    UnityHelper.DrawRect2D(rt, Color.red * 1f/i, new(size.x, 0f, 0f));
                    UnityHelper.DrawRect2D(rt, Color.red * 1f/i, new(0f, size.y, 0f));
                    i++;
                }
            }
        }
        
    }
    
    // just a lil guy for the new menu button
    public void btn_callback()
    {
        var btn = VSeeFaceHelper.CreatePropButton("Testing");
        
        var p = VSeeFaceHelper.MainPropWindow;
        
        LogWarn("PropButtonWrapper.Debug menuRightFirst PropButton / gO ");
        PropButtonWrapper.Debug(p.GetComponentInChildren<PropButton>(), p.gameObject);
        
        Log($"Tracked prop buttons: {VSeeFaceHelper.PropButtons.Count}");
    }

    public void advanced_settings_callback()
    {
        Log($"advanced settings toggled: {!AdvancedPropSettingsVisible}");
        ToggleAdvancedPropSettings();
    }

    // TODO: this type of show/hide bottom half settings of prop settings window should be
    // part of a prop settings window wrapper so that it can apply to each one individually
    // and not like currently just for the main one...
    
    public static void AddAdvancedPropSetting(GameObject go)
    {
        var prevVisibility = AdvancedPropSettingsVisible;

        AdvancedPropSettingsVisible = true;

        AdvancedPropSettings.Add(go);
        go.SetActive(prevVisibility);
    
        AdvancedPropSettingsVisible = prevVisibility;
    }

    public static void ToggleAdvancedPropSettings(bool? newValue = null)
    {
        if (newValue is not bool newVal)
            newVal = !_advancedPropSettingsVisible;
        
        if (_advancedPropSettingsVisible == newVal)
            return;

        _advancedPropSettingsVisible = newVal;
        
        var heightChange = 0f;

        foreach (var go in AdvancedPropSettings)
        {
            Log($"setting element to {newVal}: {go.name}");
            go.SetActive(newVal);
            
            var rt = go.transform.GetComponent<RectTransform>();
            
            heightChange += rt.rect.height + VSeeFaceHelper.SettingsElementSpacing;
        }
        
        var pt = VSeeFaceHelper.MainPropSettingsWindow.transform.GetComponent<RectTransform>();

        if (newVal)
        {
            pt.sizeDelta = new Vector2(pt.sizeDelta.x, pt.sizeDelta.y + heightChange);
            pt.position = new Vector2(pt.position.x, pt.position.y - heightChange + VSeeFaceHelper.SettingsElementSpacing * 1.5f);
        } else {
            pt.sizeDelta = new Vector2(pt.sizeDelta.x, pt.sizeDelta.y - heightChange);
            pt.position = new Vector2(pt.position.x, pt.position.y + heightChange - VSeeFaceHelper.SettingsElementSpacing * 1.5f);
        }
    

    }
    
    public void btn_callback2()
    {
        //var btn = VSeeFaceHelper.CreatePropButton("Zesting", ExtraPropWindow);
        LogWarn($"btn_callback2");
    }
    
    public void btn_callback3()
    {
        LogWarn($"btn_callback3");
    }
    
    
    public void slider_callback(float f)
    {
        
        LogWarn($"slider: {f}");
    }

    
    public void slider_callback2(float f)
    {
        
        LogWarn($"slider2222222: {f}");
    }
    
}
