using UnityEngine;
using System.Collections;
using Riverlake;

public class MyTest : MonoBehaviour {

    GameObject[] goes = new GameObject[100];

    const string path = "Prefab/Model/horse/30007";

    // Use this for initialization
    void Start () {
        UnityEngine.Profiling.Profiler.BeginSample("init");
        for (int i = 0; i < goes.Length; i++)
        {
            goes[i] = ObjectPool.instance.PushToPool(path, goes.Length);
        }
        UnityEngine.Profiling.Profiler.EndSample();

        StartCoroutine(test());
    }

    void Sample(int index)
    {
        UnityEngine.Profiling.Profiler.BeginSample("recycle");
        for (int i = 0; i < goes.Length; i++)
        {
            goes[i].Recycle();
            goes[i] = null;
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("spawn");
        for (int i = 0; i < goes.Length; i++)
        {
            goes[i] = ObjectPool.instance.PushToPool(path, goes.Length);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    IEnumerator test()
    {
        int i = 0;
        while (i < 10)
        {
            Sample(i++);
            yield return Yielders.GetWaitForSeconds(0.2f);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
