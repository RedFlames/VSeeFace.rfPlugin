
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UObj = UnityEngine.Object;

namespace rfPlugin.VSeeFace;

public class PropSettingsWindowWrapper
{
    public static GameObject PrefabPropSettingsButton { get; private set; }
    public static GameObject PrefabPropSettingsSlider { get; private set; }
    
    public PropSettingsWindow Wrapped { get; private set; }
    
    public GameObject Parent { get; private set; }

    public static GameObject MainPropSettings => VSeeFaceHelper.MainUI.propSettings;
    public static PropSettingsWindow MainPropSettingsWindow => VSeeFaceHelper.MainPropSettingsWindowVSF;
    public static float SettingsElementSpacing => VSeeFaceHelper.SettingsElementSpacing;

    // PropSettingsOffset is meant to track the anchor Y necessary to hit the bottom edge of last child in a prop settings window
    // NOT the center point of that last element because idk I don't wanna have to look up what half its size is when adding next?
    public static Dictionary<int, float> PropSettingsOffset = [];
    public static Dictionary<int, RectTransform> PropSettingsOrigRT = [];

    public static void InitStatic()
    {
        var buttonComponent = MainPropSettings.GetComponentInChildren<Button>();
        PrefabPropSettingsButton = UObj.Instantiate(buttonComponent.transform.gameObject, null);
        PrefabPropSettingsButton.SetActive(false);

        var sliderComponent = MainPropSettings.GetComponentInChildren<Slider>();
        PrefabPropSettingsSlider = UObj.Instantiate(sliderComponent.transform.parent.gameObject, null);
        PrefabPropSettingsSlider.SetActive(false);
        
        var origRect = UObj.Instantiate(MainPropSettings.transform.GetComponent<RectTransform>());
        PropSettingsOrigRT[MainPropSettingsWindow.GetInstanceID()] = origRect;
        
        var rt = PrefabPropSettingsButton.transform.GetComponent<RectTransform>();
        PropSettingsOffset[MainPropSettingsWindow.GetInstanceID()] = rt.anchoredPosition.y - rt.rect.height/2f;
        RfPlugin.LogDebug($"PrefabPropSettingsButton is at {rt.anchoredPosition.y}");
    }
    
    public PropSettingsWindowWrapper(PropSettingsWindow wrap)
    {
        Wrapped = wrap;
    
        Parent = wrap.gameObject;
    }
    
    public PropSettingsWindowWrapper(GameObject wrap)
    {
        Parent = wrap;
        Wrapped = wrap.GetComponentInChildren<PropSettingsWindow>();
        
        if (Wrapped == null)
        {
            RfPlugin.LogError($"PropSettingsWindowWrapper.ctor: Could not find PropSettingsWindow Child of {wrap}!");
        }
    }

    public static PropSettingsWindowWrapper CreateNewSettingsWindow()
    {
        var comp = MainPropSettings.GetComponent<PropSettingsWindow>();
        
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
        
        var window = UObj.Instantiate(comp, VSeeFaceHelper.MainUI.Settings.settings.transform);
        
        foreach (var c in origChildren)
        {
            c.transform.SetParent(comp.gameObject.transform);
        }
        
        var pt = window.transform.GetComponent<RectTransform>();
        pt.sizeDelta = new Vector2(pt.sizeDelta.x, pt.sizeDelta.y - subtractSize);
        
        PropSettingsOrigRT.Add(window.GetInstanceID(), pt);
        
        var preRT = PrefabPropSettingsButton.transform.GetComponent<RectTransform>();
        PropSettingsOffset[window.GetInstanceID()] = -SettingsElementSpacing/2f;
        
        RfPlugin.LogDebug($"CreateNewSettingsWindow elementOffset calculated to be {PropSettingsOffset[window.GetInstanceID()]} for empty window");
        
        window.gameObject.SetActive(true);

        PropSettingsWindowWrapper wrapper = new(window);
        
        return wrapper;
    }


    public GameObject CreatePropSetting<T>(string text, int siblingIndex = -1)
    where T : UObj
    {
        PropSettingsWindow window = Wrapped;
        
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
            origRect = UObj.Instantiate(MainPropSettings.transform.GetComponent<RectTransform>());
            
            PropSettingsOrigRT.Add(window.GetInstanceID(), origRect);
        }
        
        var rt = newObj.transform.GetComponent<RectTransform>();
        
        float elementOffset;
        
        if (!PropSettingsOffset.TryGetValue(window.GetInstanceID(), out elementOffset))
        {
            var preRT = PrefabPropSettingsButton.transform.GetComponent<RectTransform>();
            elementOffset = preRT.anchoredPosition.y - preRT.rect.height/2f;
            RfPlugin.LogDebug($"CreatePropSetting elementOffset inside {window.GetInstanceID()} is at {elementOffset} for the FIRST time");
        }
        elementOffset -= rt.rect.height + SettingsElementSpacing;
        
        PropSettingsOffset[window.GetInstanceID()] = elementOffset;
        RfPlugin.LogDebug($"CreatePropSetting elementOffset inside {window.GetInstanceID()} is at {elementOffset} now, {text} is {rt.rect.height} / {rt.sizeDelta.y}");
        
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, elementOffset + rt.rect.height/2f);
        
        var pt = window.transform.GetComponent<RectTransform>();
        pt.sizeDelta = new Vector2(pt.sizeDelta.x, pt.sizeDelta.y + rt.rect.height + SettingsElementSpacing);
        
        
        return newObj;
    
    }
    
    public void DebugMe()
    {
        Debug(Wrapped, Parent);
    }
    
    
    public static void Debug(PropSettingsWindow propSettingsWindow, GameObject gameObject)
    {
        if (gameObject != null)
            RfPlugin.LogGameObject($"PropSettingsWindowWrapper.Parent = {gameObject.GetInstanceID()}", gameObject);
        else
            RfPlugin.LogDebug($"PropSettingsWindowWrapper.Parent = null");
        
        if (propSettingsWindow != null && propSettingsWindow.gameObject != null)
            RfPlugin.LogGameObject($"PropSettingsWindowWrapper.Wrapped.gO = {propSettingsWindow.gameObject.GetInstanceID()}", propSettingsWindow.gameObject);
        else
            RfPlugin.LogDebug($"PropSettingsWindowWrapper.Wrapped.gO = null");

        var i = 0;
        foreach(var co in gameObject.GetComponents<Component>())
        {
            RfPlugin.LogComponent($"PropSettingsWindowWrapper.Parent.Component[{i}] = {co.GetInstanceID()}", co);
            i++;
        }
        
        i = 0;
        foreach (GameObject go in gameObject.TransChildren())
        {
            RfPlugin.LogGameObject($"PropSettingsWindowWrapper.Parent.Child[{i}] = {go.GetInstanceID()}", go);
            i++;
        }
    }
}