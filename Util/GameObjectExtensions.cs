using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace rfPlugin;

public static class GameObjectExtensions
{
    // to debug and find strings for GameObject.Find("...")
    public static string FullPath(this GameObject go) {
        
        // names of all the parent transforms
        var parNames = go.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToList();
        
        // nothing there?
        if (parNames.Count == 0)
            return go.name;
        
        // append own name unless it's identical to immediate parent, idk
        if (parNames.Last() != go.name)
            parNames.Add(go.name);
        
        return string.Join("/", parNames);
    }
    
    // get all children of the object's transform
    public static List<GameObject> TransChildren(this GameObject go)
    {
        List<GameObject> ret = [];
        
        if (go.transform == null)
            return ret;
        
        for (int i = 0; i < go.transform.childCount; i++)
            if (go.GetChildObject(i) is GameObject gogo)
                ret.Add(gogo);

        return ret;
    }
    
    public static GameObject GetChildObject(this GameObject go, int index)
    {
        if (go == null || go.transform == null || index >= go.transform.childCount || index < 0)
            return null;
        return go.transform.GetChild(index).gameObject;
    }

    
    public static GameObject GetChild<T>(this GameObject go, int index = 0)
    where T : Object
    {
        if (go == null)
            return null;
        int i = 0;
        
        RfPlugin.LogWarn($"Trying to GetChild<{typeof(T)}> on {go.name}");
        
        foreach (var gogo in go.TransChildren())
        {
            RfPlugin.LogWarn($"Checking trans child {gogo.name} of {go.name}: {gogo is T} {gogo is GameObject}");
            
            if (gogo.GetType().IsAssignableFrom(typeof(T)))
            {
                if (i == index)
                    return gogo;
                i++;
            }
        }
        return null;
    }

    public static GameObject GetChildWithObject<T>(this GameObject go, int index = 0)
    where T : Object
    {
        if (go == null)
            return null;
        int i = 0;
        
        RfPlugin.LogWarn($"Trying to GetChildWithObject<{typeof(T)}> on {go.name}");
        
        foreach (var gogo in go.TransChildren())
        {
            RfPlugin.LogWarn($"Checking trans child {gogo.name} of {go.name}: {gogo is T} {gogo is GameObject}");
            
            if (gogo.GetChild<T>() is T)
            {
                if (i == index)
                    return gogo;
                i++;
            }
        }
        return null;
    }
    
    // FIXME (?) I dunno if I should just accept that traversing to a specific game object is just a huge mess,
    // but not hide it away in these helpers because they now confuse me more when I look "in-game" what the hierarchies are...
    public static GameObject GetChildWithComponent<T>(this GameObject go, int index = 0)
    where T : Object
    {
        if (go == null)
            return null;
        int i = 0;
        
        RfPlugin.LogWarn($"Trying to GetChildWithComponent<{typeof(T)}> on {go.name}");
        
        foreach (var gogo in go.TransChildren())
        {
            RfPlugin.LogWarn($"Checking trans child {gogo.name} of {go.name}: {gogo is T} {gogo is GameObject}");
            gogo.GetComponents<Object>().Do(c => RfPlugin.LogWarn($"Child comp is {c.name} {c.GetType()} {c is T}"));
            
            if (gogo.GetComponent<T>() is T)
            {
                if (i == index)
                    return gogo;
                i++;
            }
        }
        return null;
    }

}
