using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using UObj = UnityEngine.Object;

namespace rfPlugin.VSeeFace;

/*
  A kind of wrapper class to access VSeeFace internals more easily.
*/
public partial class VSeeFaceHelper
{
    // classes that hold a lot of GameObject references
    public static UI.LaunchUI LaunchUI;
    
    public static UI.MainUI MainUI;
    
    // VSeeFace's instance of its PropWindow class
    public static PropWindow MainPropWindow;
    public static PropSettingsWindow MainPropSettingsWindow;

    public static List<PropButtonWrapper> PropButtons = [];
    
    // keep track of buttons added to right menu
    public static List<GameObject> AddedMenuButtons = [];
    
    public static GameObject PrefabMenuButton { get; private set; }
    public static GameObject PrefabPropSettingsButton { get; private set; }
    public static GameObject PrefabPropSettingsSlider { get; private set; }
    
    private static bool initialized = false;
    
    public static void Init()
    {
        RfPlugin.Log("VSeeFaceHelper init...");
        LaunchUI = new();
        MainUI = new();
        
        // TODO: Fix me
        var comp = MainUI.propsWindow.propsWindow.GetComponent<PropWindow>();
        if (comp)
            MainPropWindow = comp;
        else
            RfPlugin.LogError($"Failed to get PropWindow component from Props Window GameObject!");
        
        PrefabMenuButton = UObj.Instantiate(MainUI.menuRight.GetChildWithComponent<Button>(), null);
        PrefabMenuButton.SetActive(false);
        PrefabPropSettingsButton = UObj.Instantiate(MainUI.propSettings.GetComponentInChildren<Button>().transform.gameObject, null);
        PrefabPropSettingsButton.SetActive(false);
        PrefabPropSettingsSlider = UObj.Instantiate(MainUI.propSettings.GetComponentInChildren<Slider>().transform.parent.gameObject, null);
        PrefabPropSettingsSlider.SetActive(false);
        
        var propSettings = MainUI.propSettings.GetComponent<PropSettingsWindow>();
        if (propSettings)
            MainPropSettingsWindow = propSettings;
        else
            RfPlugin.LogError($"Failed to get MainPropSettingsWindow component from Props settings Window GameObject!");
        
        var origRect = UObj.Instantiate(MainUI.propSettings.transform.GetComponent<RectTransform>());
        PropSettingsOrigRT[MainPropSettingsWindow.GetInstanceID()] = origRect;
        
        var rt = PrefabPropSettingsButton.transform.GetComponent<RectTransform>();
        PropSettingsOffset[MainPropSettingsWindow.GetInstanceID()] = rt.anchoredPosition.y - rt.rect.height/2f;
        RfPlugin.LogError($"PrefabPropSettingsButton is at {rt.anchoredPosition.y}");
        
        RfPlugin.Log("VSeeFaceHelper initialized.");
        initialized = true;
    }
    
    public static GameObject CreateMenuButton(string text, int siblingIndex = -1)
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.CreateMenuButton before Init!");
        
        GameObject newBtn = UObj.Instantiate(PrefabMenuButton, parent: MainUI.menuRight.transform);
        AddedMenuButtons.Add(newBtn);
        
        //newBtn.transform.SetParent(MainUI.menuRight.transform, false);
        
        siblingIndex = Math.Min(siblingIndex, MainUI.menuRight.transform.childCount-1);
        
        if (siblingIndex >= 0)
            newBtn.transform.SetSiblingIndex(siblingIndex);
        
        newBtn.SetActive(true);
        
        Text textComp = newBtn.GetComponentInChildren<Text>();
        if (textComp != null)
            textComp.text = text;
        else
            RfPlugin.LogError($"Could not find Text component for new rightMenu button '{text}'");
        
        RfPlugin.LogGameObject("New Button added", newBtn);
        RfPlugin.LogComponent("newBtn.Text", textComp);
        
        Button buttonComp = newBtn.GetComponentInChildren<Button>();
        RfPlugin.LogComponent("btn before", buttonComp);

        //UObj.DestroyImmediate(buttonComp);
        
        //buttonComp = newBtn.AddComponent<Button>();
        RfPlugin.LogComponent("btn after", buttonComp);
        
        buttonComp.onClick.RemoveAllListeners();
        buttonComp.onClick = new();
        
        return newBtn;
    
    }
    
    // PropSettingsOffset is meant to track the anchor Y necessary to hit the bottom edge of last child in a prop settings window
    // NOT the center point of that last element because idk I don't wanna have to look up what half its size is when adding next?
    public static Dictionary<int, float> PropSettingsOffset = [];
    public static Dictionary<int, RectTransform> PropSettingsOrigRT = [];

    public static float SettingsElementSpacing = 8f;
    
    public static GameObject CreatePropSetting<T>(string text, PropSettingsWindow window = null, int siblingIndex = -1)
    where T : UObj
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.CreatePropSetting before Init!");
        
        window ??= MainPropSettingsWindow;
        
        GameObject newObj;
        
        // this is mega pointless
        bool isButton = typeof(Button).IsAssignableFrom(typeof(T));
        bool isSlider = typeof(Slider).IsAssignableFrom(typeof(T));
        //bool isCheckbox = typeof(Checkbox).IsAssignableFrom(typeof(T));
        
        if (isButton)
            newObj = UObj.Instantiate(PrefabPropSettingsButton, parent: window.transform);
            //newObj = UObj.Instantiate(MainUI.propSettings.GetChildWithComponent<Button>(), parent: MainUI.propSettings.transform);
        else if (isSlider)
        {
            
            newObj = UObj.Instantiate(PrefabPropSettingsSlider, parent: window.transform);
            //newObj = UObj.Instantiate(MainUI.propSettings.TransChildren().First(ch => ch.GetComponentInChildren<Slider>()), parent: MainUI.propSettings.transform);
        }
        else
        {
            RfPlugin.LogError($"Cannot determine what UI element to instantiate as '{typeof(T)}', sorry.");
            return null;
        }
        
        //TODO
        //AddedMenuButtons.Add(newBtn);
        
        //newBtn.transform.SetParent(MainUI.menuRight.transform, false);
        
        siblingIndex = Math.Min(siblingIndex, window.transform.childCount-1);
        
        if (siblingIndex >= 0)
            newObj.transform.SetSiblingIndex(siblingIndex);
        
        newObj.SetActive(true);
        
        Text textComp = newObj.GetComponentInChildren<Text>();
        if (textComp != null)
            textComp.text = text;
        else
            RfPlugin.LogError($"Could not find Text component for new prop setting '{text}'");
        
        RfPlugin.LogGameObject($"New PS {typeof(T)} = {newObj.GetType()} added", newObj);
        RfPlugin.LogComponent("newObj.Text", textComp);
        
        if (isButton)
        {
            Button comp = newObj.GetComponentInChildren<Button>();
            comp.onClick.RemoveAllListeners();
            comp.onClick = new();
        } else if (isSlider)
        {
            Slider comp = newObj.GetComponentInChildren<Slider>();
            comp.onValueChanged.RemoveAllListeners();
            comp.onValueChanged = new();
        }
        
        RectTransform origRect;
        if (!PropSettingsOrigRT.TryGetValue(window.GetInstanceID(), out origRect))
        {
            origRect = UObj.Instantiate(MainUI.propSettings.transform.GetComponent<RectTransform>());
            
            PropSettingsOrigRT.Add(window.GetInstanceID(), origRect);
        }
        
        var rt = newObj.transform.GetComponent<RectTransform>();
        
        float elementOffset;
        
        if (!PropSettingsOffset.TryGetValue(window.GetInstanceID(), out elementOffset))
        {
            var preRT = PrefabPropSettingsButton.transform.GetComponent<RectTransform>();
            elementOffset = preRT.anchoredPosition.y - preRT.rect.height/2f;
            RfPlugin.LogError($"CreatePropSetting elementOffset inside {window.GetInstanceID()} is at {elementOffset} for the FIRST time");
        }
        elementOffset -= rt.rect.height + SettingsElementSpacing;
        
        PropSettingsOffset[window.GetInstanceID()] = elementOffset;
        RfPlugin.LogError($"CreatePropSetting elementOffset inside {window.GetInstanceID()} is at {elementOffset} now, {text} is {rt.rect.height} / {rt.sizeDelta.y}");
        
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, elementOffset + rt.rect.height/2f);
        
        var pt = window.transform.GetComponent<RectTransform>();
        pt.sizeDelta = new Vector2(pt.sizeDelta.x, pt.sizeDelta.y + rt.rect.height + SettingsElementSpacing);
        
        
        return newObj;
    
    }
    
    public static PropWindow CreateNewPropWindow()
    {
        var window = UObj.Instantiate(MainPropWindow, MainUI.Settings.settings.transform);
        return window;
    }
    
    public static PropSettingsWindow CreateNewSettingsWindow()
    {
        var comp = MainUI.propSettings.GetComponent<PropSettingsWindow>();
        
        var origChildren = comp.gameObject.TransChildren();

        // calculating how much to shrink the new window because it will have no children
        // should probably instead just take the abs anchor Y of the last child + height/2 - anchor Y of first Child + height/2
        float subtractSize = -2 * SettingsElementSpacing * 1.5f;
        foreach (var c in origChildren)
        {
            var rt = c.transform.GetComponent<RectTransform>();
            // rough might average out for the real spacings on the original...
            subtractSize += rt.rect.height + SettingsElementSpacing;
            
            c.transform.SetParent(null);
        }

        var window = UObj.Instantiate(comp, MainUI.Settings.settings.transform);
        
        foreach (var c in origChildren)
        {
            c.transform.SetParent(comp.gameObject.transform);
        }
        
        var pt = window.transform.GetComponent<RectTransform>();
        pt.sizeDelta = new Vector2(pt.sizeDelta.x, pt.sizeDelta.y - subtractSize);
        
        PropSettingsOrigRT.Add(window.GetInstanceID(), pt);

        var preRT = PrefabPropSettingsButton.transform.GetComponent<RectTransform>();
        PropSettingsOffset[window.GetInstanceID()] = -SettingsElementSpacing/2f;
        
        RfPlugin.LogError($"CreateNewSettingsWindow elementOffset calculated to be {PropSettingsOffset[window.GetInstanceID()]} for empty window");
        
        window.gameObject.SetActive(true);
        
        return window;
    }

    public static PropButtonWrapper CreatePropButton(string text, PropWindow window = null)
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.CreatePropButton before Init!");

        if (window == null)
            window = MainPropWindow;
        
        var newEntry = UObj.Instantiate(window.propButtonPrefab, window.content);
        
        PropButton componentInChildren = newEntry.GetComponentInChildren<PropButton>();

        PropButtonWrapper newWrapped = new(newEntry);
        PropButtonWrapper newWrapped2 = new(componentInChildren);
        
        RfPlugin.LogWarn("PropButtonWrapper.DebugMe newWrapped");
        
        newWrapped.DebugMe();
        RfPlugin.LogWarn("PropButtonWrapper.DebugMe newWrapped2");
        newWrapped2.DebugMe();
        
        componentInChildren.window = window;
        //Prop component = gO_Text.GetComponent<Prop>();
        //component.currentSettings = PropManager.Singleton.baseSettings;
        //component.gameObject.SetActive(value: false);
        //component.Init();
        //component.enabled = false;
        newEntry.SetActive(true);

        // TODO: Fix me

        var textUI = newEntry.GetComponent<TextMeshProUGUI>();

        if (!textUI)
            textUI = newEntry.GetComponentInChildren<TextMeshProUGUI>();

        if (!textUI)
            textUI = newEntry.AddComponent<TextMeshProUGUI>();
        
        // junk, yippie
        
        if (textUI)
        {
            textUI.enabled = true;
            textUI.text = text;
        }

        return newWrapped;
    }

    public static Prop SpawnProp(Texture2D tex)
    {
        var newProp = PropManager.Singleton.CreateProp(tex,  new List<Prop.ImageDelay>());
        
        newProp.propImage.gameObject.SetActive(false);
        var mr = newProp.GetComponentInChildren<MeshRenderer>();
        mr.material.SetInt("_ZTest", 0);
        mr.material.renderQueue = 5000;
        //mr.material.mainTextureScale = new(.5f, .5f);
        RfPlugin.LogDebug($"-- spawning prop -- {newProp} {tex}");
        //mr.enabled = false;
        return newProp;
    }
    
    // Log dump looking at all the children and components of the main UI?
    public static void DebugLogObjects()
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.DebugLogObjects before Init!");
        
        foreach (GameObject go in MainUI.mainUI.TransChildren())
        {
            
            RfPlugin.LogGameObject("mainUI child", go);
        }

        foreach(var i in MainUI.menuRight.GetComponents<Component>())
        {
            RfPlugin.LogDebug($"menuRight {i} {i.GetType()} {i.name}");
        }
        var mrVertical = MainUI.menuRight.GetComponent<VerticalLayoutGroup>();
        RfPlugin.LogDebug($"mrVertical {mrVertical} {mrVertical.GetType()}");

        foreach(var i in mrVertical.GetComponents<Component>())
        {
            RfPlugin.LogDebug($"mrVertical {i} {i.GetType()} {i.name}");
        }

        for (int i = 0; i < MainUI.menuRight.transform.childCount; i++)
        {
            Transform t = MainUI.menuRight.transform.GetChild(i);
            RfPlugin.LogComponent($"menuRight tf child[{i}]", t);
        }

    }
    
    // Trying to find the unity object names of everything with "VSee" in its full path/name
    public static void DumpObjectsByName()
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.DumpObjectsByName before Init!");
        
        RfPlugin.Log($"{Resources.FindObjectsOfTypeAll(typeof(GameObject)).Count()}");
        RfPlugin.Log($"{Resources.FindObjectsOfTypeAll<GameObject>().Count()}");

        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>()) 
        {
            var fullName = go.FullPath();
            if (go.name.IContains("vsee") || fullName.IContains("vsee")){
                RfPlugin.LogGameObject(fullName, go);
            }
        }
    }
}
