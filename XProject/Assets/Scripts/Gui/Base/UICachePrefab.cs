using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICachePrefab : MonoBehaviour {

    //private int orginalWith;
    //private int orginalHeight;
    public string prefabName;
    private GameObject cachePrefab;

    void Awake()
    {
        cachePrefab = UICachePool.Spawn(prefabName);
        if (cachePrefab != null)
            cachePrefab.transform.SetParent(transform);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnDisable()
    {
        UICachePool.Recycle(prefabName, cachePrefab);
    }
}
