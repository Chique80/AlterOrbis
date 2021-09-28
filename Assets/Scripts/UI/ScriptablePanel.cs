using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptablePanel : MonoBehaviour
{
    protected virtual void Awake()
    {
        Setup();
    }

    #region Abstract

    protected abstract void Setup();
    
    public abstract void Activate();            //find a way to call both Activate and SetActive(true)
    public abstract void Deactivate();         //find a way to call both Deactivate and SetActive(false)

    #endregion
}
