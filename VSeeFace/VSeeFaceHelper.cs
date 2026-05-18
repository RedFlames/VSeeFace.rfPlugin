using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using rfPlugin.VSeeFace.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public static List<PropButtonWrapper> PropButtons = [];
    
    // keep track of buttons added to right menu
    public static List<GameObject> AddedMenuButtons = [];
    
    private static bool initialized = false;
    
    public static void Init()
    {
        RfPlugin.Log("VSeeFaceHelper init...");
        LaunchUI = new();
        MainUI = new();
        
        // TODO: Fix me
        var comp = MainUI.propsWindow.GetComponent<PropWindow>();
        if (comp)
            MainPropWindow = comp;
        else
            RfPlugin.LogError($"Failed to get PropWindow component from Props Window GameObject!");
        
        RfPlugin.Log("VSeeFaceHelper initialized.");
        initialized = true;
    }
    
    public static GameObject CreateMenuButton(string text, UnityEngine.Events.UnityAction callback, int siblingIndex = -1)
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.CreateMenuButton before Init!");
        
        var menuRightFirst = MainUI.menuRight.transform.GetChild(0);
        
        GameObject newBtn = GameObject.Instantiate(menuRightFirst.gameObject);
        AddedMenuButtons.Add(newBtn);
        
        newBtn.transform.SetParent(MainUI.menuRight.transform, false);
        
        siblingIndex = Math.Min(siblingIndex, MainUI.menuRight.transform.childCount-1);
        
        if (siblingIndex >= 0)
            newBtn.transform.SetSiblingIndex(siblingIndex);

        newBtn.SetActive(true);
        
        Text textComp = newBtn.GetComponentInChildren<Text>();
        if (textComp != null)
            textComp.text = text;
        
        RfPlugin.LogGameObject("New Button added", newBtn);
        RfPlugin.LogComponent("newBtn.Text", textComp);
        
        Button buttonComp = newBtn.GetComponent<Button>();
        RfPlugin.LogComponent("btn before", buttonComp);
        
        GameObject.DestroyImmediate(buttonComp);

        buttonComp = newBtn.AddComponent<Button>();
        RfPlugin.LogComponent("btn after", buttonComp);
        
        //btn.onClick.RemoveAllListeners();
        //btn.onClick = new();
        buttonComp.onClick.AddListener(callback);
        
        return newBtn;
    
    }

    public static PropWindow CreateNewPropWindow()
    {
        var window = GameObject.Instantiate(MainPropWindow);
        window.transform.parent = MainUI.Settings.settings.transform;
        return window;
    }

    public static PropButtonWrapper CreatePropButton(string text, PropWindow window = null)
    {
        if (!initialized)
            RfPlugin.LogError("Attempting to run VSeeFaceHelper.CreatePropButton before Init!");

        if (window == null)
            window = MainPropWindow;
        
        var newEntry = GameObject.Instantiate(window.propButtonPrefab, window.content);
        
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
        
        //Traverse.Create(targetProp).Field<Transform>("attachedBone").Value = attachedBone;
        //targetProp.propImage.transform.position = __instance.prop.transform.position;
        newProp.propImage.gameObject.SetActive(false);
        //var sprI = Traverse.Create(__instance.prop).Field<Transform>("sprite").Value;
        //var sprT = Traverse.Create(newProp).Field<Transform>("sprite").Value;
        //sprT.localScale = sprI.localScale;
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
