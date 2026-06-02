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
    public static PropSettingsWindowWrapper ExtensionSettings { get; private set; }
    
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
    
    private GameObject transparencySlider;

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

        ExtensionSettings = PropSettingsWindowWrapper.CreateNewSettingsWindow();
        
        // just some debugging log dumps to find VSeeFace stuff
        //VSeeFaceHelper.DumpObjectsByName();
        //VSeeFaceHelper.DebugLogObjects();
        
        // To see what the Detect shader looks like in practice.
        // Might be missing the material setup stuff from RaycastMesh until you start attaching a prop.
        //Camera.main.SetReplacementShader(Shader.Find("Custom/Detect"), "RenderType");
        
        for (int i = 0; i < 5; i++)
            _Spheres.Add(UnityHelper.CreateTransparentSphere(alpha: .05f + .1f * i));
    
        // --- Add new right-menu button(s) ---
        var btn = VSeeFaceHelper.CreateMenuButton("Extension Settings", 2).OnClick(extension_settings_callback);
        var btn2 = VSeeFaceHelper.CreateMenuButton("test2").OnClick(btn_callback2);
        
        var btn3 = VSeeFaceHelper.MainPropSettingsWindow.CreatePropSetting<Button>("Advanced Settings").OnClick(advanced_settings_callback);
        transparencySlider = VSeeFaceHelper.MainPropSettingsWindow.CreatePropSetting<Slider>("Opacity").OnSlide(transparency_slider_callback);
        AddAdvancedPropSetting(transparencySlider);
        transparencySlider.GetComponentInChildren<Slider>().maxValue = 100f;

        VSeeFaceHelper.onRefreshSettings += RefreshSettings;
        
        // needed to move this to Update() because the Singleton isn't set until PropManager.Start?
        // PropManager.Singleton.onSelectedPropChange += RefreshSettings;

        //var slider = btn4.GetComponentInChildren<Slider>();
        var adv_btn = VSeeFaceHelper.MainPropSettingsWindow.CreatePropSetting<Button>("Some button").OnClick(btn_callback3);
        AddAdvancedPropSetting(adv_btn);
        
        //btn4 = VSeeFaceHelper.MainUI.propSettings.TransChildren().First(ch => ch.GetChildWithComponent<Slider>());
        //slider = btn4.GetComponentInChildren<Slider>();
        
        ExtensionSettings.CreatePropSetting<Button>("gizmo cam").OnClick(delegate
        {
            if (!DebugGizmoCamera) return;
            DebugGizmoCamera.enabled = !DebugGizmoCamera.enabled;
        });
        ExtensionSettings.CreatePropSetting<Button>("gizmo top cam").OnClick(delegate
        {
            if (!OverlayGizmoCamera) return;
            OverlayGizmoCamera.enabled = !OverlayGizmoCamera.enabled;
        });
        ExtensionSettings.CreatePropSetting<Button>("on top cam").OnClick(delegate
        {
            if (!AlwaysTopPropsCam) return;
            AlwaysTopPropsCam.enabled = !AlwaysTopPropsCam.enabled;
            // TODO does spoutExport need update?
        });
        ExtensionSettings.CreatePropSetting<Button>("Camera.main").OnClick(delegate
        {
            if (!Camera.main) return;
            Camera.main.enabled = !Camera.main.enabled;
        });
        


        var btn6 = ExtensionSettings.CreatePropSetting<Slider>("test4").OnSlide(slider_callback2);
        var btn7 = ExtensionSettings.CreatePropSetting<Toggle>("test5").OnToggle(toggle_callback);
        var btn8 = ExtensionSettings.CreatePropSetting<Dropdown>("test6").OnDropdownChange(dropdown_callback);
    
    }
    
    public static Camera DebugGizmoCamera { get; private set; }
    public static Camera OverlayGizmoCamera { get; private set; }
    public static Camera AlwaysTopPropsCam { get; private set; }
    
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
    
    public static int spheresLayer = 20; //LayerMask.NameToLayer("HiddenAvatar");
    public static int onTopPropsLayer = 21; //LayerMask.NameToLayer("HiddenAvatar");
    
    public static void UpdateSpheres(Vector3 pos, Vector3 locScale)
    {
        int i = 1;
        foreach (var sphere in _Spheres)
        {
            var factor = 2f / i;
            //var factor = .1f * i;
            sphere.transform.position = pos;
            sphere.transform.localScale = locScale * factor;
            sphere.layer = spheresLayer;
            i++;
        }
        //MyCoolCamera.CopyFrom(Camera.main);
        //MyCoolCamera.cullingMask = LayerMask.GetMask(LayerMask.LayerToName(spheresLayer));
        //MyCoolCamera.clearFlags = CameraClearFlags.Nothing;
        //MyCoolCamera.depth = 1;
    }

    //public static int setGizmoCamDepth = -1;
    public static List<int> myCamCullingMask = [spheresLayer];
    
    private void Update()
    {
        // A lot of plugin related things happen in the Harmony patches.
        
        // TODO: rewire all this messing around with cameras to be cleaner...!

        if (DebugGizmoCamera == null)
        {
            //if (Camera.main)
            //    DebugGizmoCamera = Instantiate(Camera.main);
            //DebugGizmoCamera = new();
            var go = new GameObject("DebugGizmoCamera");
            DebugGizmoCamera = go.AddComponent<Camera>();
        
        }
        if (Camera.main)
            DebugGizmoCamera.CopyFrom(Camera.main);
        //DebugGizmoCamera.cullingMask = LayerMask.GetMask(LayerMask.LayerToName(spheresLayer));
        //DebugGizmoCamera.cullingMask = LayerMask.GetMask(myCamCullingMask.Select(i => LayerMask.LayerToName(i)).ToArray());
        DebugGizmoCamera.cullingMask = 1 << spheresLayer;
        
        DebugGizmoCamera.clearFlags = CameraClearFlags.Nothing;
        DebugGizmoCamera.depth = -0.97f;
        
        if (AlwaysTopPropsCam == null)
        {
            //if (Camera.main)
            //    AlwaysTopPropsCam = Instantiate(Camera.main);
            //AlwaysTopPropsCam = new();
            var go = new GameObject("AlwaysTopPropsCam");
            AlwaysTopPropsCam = go.AddComponent<Camera>();
        
        }
        if (Camera.main)
            AlwaysTopPropsCam.CopyFrom(Camera.main);
        //AlwaysTopPropsCam.cullingMask = LayerMask.GetMask(LayerMask.LayerToName(spheresLayer));
        AlwaysTopPropsCam.cullingMask = 1 << onTopPropsLayer;
        
        // FIXME: The Always-Top props end up on top of post-processing....
        // => apply post-processing to this camera as well?... mayyybe
        AlwaysTopPropsCam.clearFlags = CameraClearFlags.Depth;
        AlwaysTopPropsCam.depth = -1f;
        
        if (OverlayGizmoCamera == null)
        {
            //if (Camera.main)
            //    OverlayGizmoCamera = Instantiate(Camera.main);
            //OverlayGizmoCamera = new();
            var go = new GameObject("OverlayGizmoCamera");
            OverlayGizmoCamera = go.AddComponent<Camera>();
        
        }
        if (Camera.main)
            OverlayGizmoCamera.CopyFrom(Camera.main);
        OverlayGizmoCamera.cullingMask = 1 << spheresLayer;
        
        OverlayGizmoCamera.clearFlags = CameraClearFlags.Depth;
        OverlayGizmoCamera.depth = 1f;
        
        
        /*if (MyCoolCamera && Camera.main && !gotCameraCopyFrom)
        {
            MyCoolCamera.CopyFrom(Camera.main);
            //MyCoolCamera.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
            MyCoolCamera.cullingMask = LayerMask.GetMask("Gizmos");
            //MyCoolCamera.depthTextureMode = DepthTextureMode.None;
            MyCoolCamera.clearFlags = CameraClearFlags.Depth;
            gotCameraCopyFrom = true;
        }*/
    

    }
    
    /*
        from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnRenderObject.html:
        - OnRenderObject is called after camera has rendered the Scene.
        - runs on for every GameObject with a script that uses this callback
    */
    public void OnRenderObject()
    {
        

        // only interested in the main camera that renders the VSF avatar
        if (Camera.current != DebugGizmoCamera && Camera.current != OverlayGizmoCamera)
            return;
        
        // using https://github.com/loco-choco/GizmosLibraryPlugin sample code
        // seems like it's a BepInEx vresion of an OWML (Outer Wilds Mod Loader) plugin?...
        // I only know that I had trouble with Debug.DrawLine not working, probably should use LineRenderer or idk
        // I don't know much about Unity and I just wanted gizmos like in RuntimeUnityEditor, and this worked well so far :3

        // Sets the default material for gizmos
        GizmosLibraryPlugin.GizmosAPI.SetDefaultMaterialPass();
        
        Color colorAttachRay = Color.magenta;
        Color colorAttachCapsule = Color.yellow;
        Color colorBone = Color.cyan;

        if (Camera.current == OverlayGizmoCamera)
        {
            colorAttachRay.a = .12f;
            colorAttachCapsule.a = .12f;
            colorBone.a = .3f;
        } else if (Camera.current == DebugGizmoCamera)
        {
            colorBone.a = .5f;
        }
        
        // draw some gizmos after/while a VSF prop is being attached
        if (attachedBone != null)
        {
            // frame of reference is the bone that the prop attaches to
            GizmosLibraryPlugin.GizmosAPI.DrawWithReference(attachedBone,() =>
            {
                // draw some junk at the attachmentRay :juh:
                //GizmosLibraryPlugin.GizmosAPI.DrawWireframeCapsule(0.1f, Vector3.forward + Vector3.up * 0.1f, Vector3.forward - Vector3.up * 0.1f, Color.cyan, 12);
                GizmosLibraryPlugin.GizmosAPI.DrawWireframeCapsule(0.02f, attachmentRay.origin, attachmentRay.origin + attachmentRay.direction * .02f, colorAttachCapsule, 12);
                GizmosLibraryPlugin.GizmosAPI.DrawVector(attachmentRay.direction.normalized, .02f, attachmentRay.origin, colorAttachRay);
                
                GizmosLibraryPlugin.GizmosAPI.DrawVector(Vector3.up, .02f, Vector3.zero, colorBone);
            });
        }

        if (Camera.current == OverlayGizmoCamera)
            return;
    
        var drags = Resources.FindObjectsOfTypeAll<Draggable>();
        foreach (var drag in drags)
        {
            if (drag.isMouseDown)
            {
                RectTransform prt = drag.transform.GetComponent<RectTransform>();
                
                Vector2 size = new(prt.rect.width, prt.rect.height);
                UnityHelper.DrawRect2D(prt, Color.red, new(size.x, 0f, 0f), DebugGizmoCamera);
                UnityHelper.DrawRect2D(prt, Color.red, new(0f, size.y, 0f), DebugGizmoCamera);
                
                int i = 1;
                foreach (var c in drag.gameObject.TransChildren())
                {
                    RectTransform rt = c.transform.GetComponent<RectTransform>();
                
                    UnityHelper.DrawRect2D(rt, Color.red * 1f/i, new(size.x, 0f, 0f), DebugGizmoCamera);
                    UnityHelper.DrawRect2D(rt, Color.red * 1f/i, new(0f, size.y, 0f), DebugGizmoCamera);
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
    
    public void toggle_callback(bool val)
    {
        Log($"toggle_callback: {val}");
    }
    
    public void dropdown_callback(int val)
    {
        Log($"dropdown_callback: {val}");
    }
    
    public void extension_settings_callback()
    {
        var windowVisible = ExtensionSettings.Parent.activeSelf;
        Log($"extension_settings_callback: {windowVisible}");
    
        ExtensionSettings.Parent.SetActive(!windowVisible);
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
        
        var pt = VSeeFaceHelper.MainPropSettingsWindowVSF.transform.GetComponent<RectTransform>();

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
        LogWarn($"btn_callback2");
    }
    
    public void btn_callback3()
    {
        LogWarn($"btn_callback3");
    }
    
    public void transparency_slider_callback(float f)
    {
        //LogError($"transparency_slider_callback runs NOW -- settingTheDamnValueNow is {settingTheDamnValueNow}, {f}");
        LogWarn($"slider: {f}");
        VSeeFaceHelper.SettingsTransparency = f;
    }
    
    public void RefreshSettings()
    {
        //LogError($"RefreshSettings runs NOW -- setting slider to {VSeeFaceHelper.baseSettingsExt.transparency}");
        transparencySlider.GetComponentInChildren<Slider>().value = VSeeFaceHelper.baseSettingsExt.transparency;
    }
    
    public void slider_callback2(float f)
    {
        
        LogWarn($"slider2222222: {f}");
    }
    
}
