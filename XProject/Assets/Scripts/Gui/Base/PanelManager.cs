using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using LuaInterface;
using AL.Resources;
using System;
using System.Text;
using LuaFramework;
using AL;
using System.IO;

class PanelData
{
    public string Name { get; set; }
    public int Weight { get; set; }
    public float CreateTime { get; set; }
    public GameObject PanelObject { get; set; }
    public LuaBehaviour behaviour { get; set; }
    /// <summary>
    /// 根节点的sortingOrder
    /// </summary>
    public int order { get; set; }

    public void AddOrder(int depth)
    {
        order = depth * 10;
        Canvas[] cans = PanelObject.GetComponentsInChildren<Canvas>();
        for (int i = 0, count = cans.Length;i<count;++i)
        {
            cans[i].sortingOrder = cans[i].sortingOrder + order;
        }
        ParticleSystem[] ps = PanelObject.GetComponentsInChildren<ParticleSystem>();
        for(int j=0,length=ps.Length;j<length;++j)
        {
            Renderer[] renders = ps[j].GetComponentsInChildren<Renderer>();
            for(int k=0, size=renders.Length;k<size;++k)
            {
                renders[k].sortingOrder = renders[k].sortingOrder + order;
            }
        }
    }

    public void ResetOrder()
    {
        Canvas[] cans = PanelObject.GetComponentsInChildren<Canvas>();
        for (int i = 0, count = cans.Length; i < count; ++i)
        {
            cans[i].sortingOrder = cans[i].sortingOrder - order;
        }
        ParticleSystem[] ps = PanelObject.GetComponentsInChildren<ParticleSystem>();
        for (int j = 0, length = ps.Length; j < length; ++j)
        {
            Renderer[] renders = ps[j].GetComponentsInChildren<Renderer>();
            for (int k = 0, size = renders.Length; k < size; ++k)
            {
                renders[k].sortingOrder = renders[k].sortingOrder - order;
            }
        }
        order = 0;
    }
}

public class PanelManager : Singleton<PanelManager> {

    public static PanelManager GetSingleton()
    {
        return PanelManager.Instance; 
    }

    private LFUCache<string, PanelData> uiList = new LFUCache<string, PanelData>(5);

    private List<AssetLoader> loaderList = new List<AssetLoader>();

    private AssetLoader panelLoader;

    private StringBuilder sb = new StringBuilder();

    private ObjectPoolManager poolManager;

    private const int START_ORDER = 10;

    private const int OFFSET_ORDER = 10;

    private int _curDepth;
    private string _curShowPanel;

    /// <summary>
    /// 显示中的UI界面列表
    /// </summary>
    private List<string> uiShowList = new List<string>();

    public PanelManager()
    {
        panelLoader = new AssetLoader(this);
        poolManager = AppFacade.Instance.GetManager<ObjectPoolManager>();
        poolManager.CreatePool<PanelData>(OnGetPanelData, OnReleasePanelData);
        _curDepth = 0;
        _curShowPanel = string.Empty;
    }

    public Transform GetNotifyTrans()
    {
        return this.FindParent().transform;
    }

    private GameObject FindParent()
    {
        GameObject rootGo = GameObject.Find("UIRoot");
        if(rootGo == null)
        {
            rootGo = new GameObject("UIRoot");
            rootGo.layer = LayerMask.NameToLayer("UI");
        }
        return rootGo;
    }

    /// <summary>
    /// 创建面板，请求资源管理器
    /// </summary>
    /// <param name="type"></param>
    public void CreatePanel(string path, int weight, LuaFunction func = null) {
        string name = Path.GetFileNameWithoutExtension(path);
        if (uiList.GetValue(path) != null) return;

        AssetWidget uiWidget = new AssetWidget(path, weight, (ao) => 
        {
            var prefab = ao.GetAsset<GameObject>();
            if (prefab == null)
            {
                Debug.LogError(string.Format("Panel load error: no panel founded! {0}, {1}", ao.assetPath, name));
                return;
            }
            GameObject go = Instantiate(prefab) as GameObject;
            go.name = name;
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(FindParent().transform, false);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            LuaBehaviour behaviour = go.transform.GetOrAddComponent<LuaBehaviour>();
            behaviour.OnInit();
            PanelData panelData;
            try
            {
                panelData = poolManager.Get<PanelData>();
            }
            catch
            {
                panelData = new PanelData();
            }
            panelData.Name = name;
            panelData.Weight = weight;
            panelData.PanelObject = go;
            panelData.behaviour = behaviour;
            uiList.Put(name, panelData);
            uiShowList.Add(name);
            if (func != null) func.Call(go);
            _curShowPanel = name;
            _curDepth++;
            panelData.AddOrder(_curDepth);
            Debug.LogWarning("CreatePanel::>> " + name + " " + prefab);
        });
        panelLoader.LoadAsset(uiWidget);
    }

    private void Update()
    {
        panelLoader.Update();
    }

    private void OnGetPanelData(PanelData data)
    {
        data.CreateTime = Time.realtimeSinceStartup;
    }

    private void OnReleasePanelData(PanelData data)
    {
        uiList.RemoveKey(data.Name);
        if (data.PanelObject != null)
            Destroy(data.PanelObject);
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="name"></param>
    public void ClosePanel(string name) {
        uiShowList.Remove(name);
        PanelData panelObj = uiList.GetValue(name);
        if (panelObj != null)
        {
            if (_curShowPanel == name)
                _curDepth--;
            panelObj.ResetOrder();
            panelObj.behaviour.OnClose();
            poolManager.Release<PanelData>(panelObj);
        }
            
    }

    public void Clear()
    {
        panelLoader.Clear();
    }
}