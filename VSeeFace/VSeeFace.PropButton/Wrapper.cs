
using UnityEngine;

namespace rfPlugin.VSeeFace;

public class PropButtonWrapper
{
    public PropButton Wrapped { get; private set; }
    
    public GameObject Parent { get; private set; }
    
    public PropButtonWrapper(PropButton wrap)
    {
        Wrapped = wrap;
    
        Parent = wrap.gameObject;
    }

    public PropButtonWrapper(GameObject wrap)
    {
        Parent = wrap;
        Wrapped = wrap.GetComponentInChildren<PropButton>();

        if (Wrapped == null)
        {
            RfPlugin.LogError($"PropButtonWrapper.ctor: Could not find PropButton Child of {wrap}!");
        }
    }

    public void DebugMe()
    {
        Debug(Wrapped, Parent);
    }
    
    
    public static void Debug(PropButton propButton, GameObject gameObject)
    {
        if (gameObject != null)
            RfPlugin.LogGameObject($"PropButtonWrapper.Parent = {gameObject.GetInstanceID()}", gameObject);
        else
            RfPlugin.LogDebug($"PropButtonWrapper.Parent = null");
        
        if (propButton != null && propButton.gameObject != null)
            RfPlugin.LogGameObject($"PropButtonWrapper.Wrapped.gO = {propButton.gameObject.GetInstanceID()}", propButton.gameObject);
        else
            RfPlugin.LogDebug($"PropButtonWrapper.Wrapped.gO = null");

        var i = 0;
        foreach(var co in gameObject.GetComponents<Component>())
        {
            RfPlugin.LogComponent($"PropButtonWrapper.Parent.Component[{i}] = {co.GetInstanceID()}", co);
            i++;
        }
        
        i = 0;
        foreach (GameObject go in gameObject.TransChildren())
        {
            RfPlugin.LogGameObject($"PropButtonWrapper.Parent.Child[{i}] = {go.GetInstanceID()}", go);
            i++;
        }
    }
}