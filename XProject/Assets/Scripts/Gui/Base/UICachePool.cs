using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICachePool : MonoBehaviour {

    public UICachePool instance;

    private const int INIT_SIZE = 5;

    private static Dictionary<string, List<GameObject>> _pools = new Dictionary<string, List<GameObject>>();

    private static Dictionary<string, GameObject> _poolItem = new Dictionary<string, GameObject>();

    void Awake()
    {
        instance = this;
        _poolItem.Add("Bg", Resources.Load<GameObject>(""));
        _poolItem.Add("FakeItem", Resources.Load<GameObject>(""));
        init();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void init()
    {
        //在初进游戏时就默认先加载背景底框prefab
        GameObject prefab = Resources.Load<GameObject>("");
        List<GameObject> prefabList = new List<GameObject>();
        for(int i=0;i<2;i++)
        {
            GameObject go = (GameObject)UnityEngine.Object.Instantiate(prefab);
            prefabList.Add(go);
            go.transform.SetParent(transform);
        }
        _pools.Add("Bg", prefabList);
    }

    public static GameObject Spawn(string prefabName)
    {
        GameObject tmpGo = null;
        if(_pools.ContainsKey(prefabName))
        {
            if(_pools[prefabName].Count > 0)
            {
                tmpGo = _pools[prefabName][0];
                _pools[prefabName].RemoveAt(0);
            }
            else
            {
                tmpGo = (GameObject)UnityEngine.Object.Instantiate(_poolItem[prefabName]);
            }
        }
        return tmpGo;
    }

    public static void Recycle(string prefabName, GameObject go)
    {
        if (!_pools.ContainsKey(prefabName))
        {
            Debug.LogError("回收了不该放在UI对象池的对象:" + prefabName);
            return;
        }
        _pools[prefabName].Add(go);
        //go.transform.SetParent(transform);
    }
}
