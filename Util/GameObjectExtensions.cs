using System.Collections.Generic;
using System.Linq;
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
        {
            Transform tf = go.transform.GetChild(i);
            if (tf != null && tf.gameObject != null)
                ret.Add(go.transform.GetChild(i).gameObject);
        }

        return ret;
    }
}
