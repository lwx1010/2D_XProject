using UnityEngine;
using System.Collections;
using System;
using Riverlake;

public class AutoDestroy : MonoBehaviour 
{
    public float lifetime = 2.0f;

    public Action onDestroyCallback = null;

    void OnEnable()
    {
        Invoke("AutoRecycle", lifetime);
    }

    void AutoRecycle()
    {
        //gameObject.RecycleDontDestroy();
    }

    void OnDisable()
    {
        CancelInvoke("AutoRecycle");
        if (onDestroyCallback != null)
            onDestroyCallback();
        onDestroyCallback = null;
        //Destroy(this);
    }
}
