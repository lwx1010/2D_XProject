using UnityEngine;
using System.Collections;
using LuaFramework;
using System.Collections.Generic;
using AL;

public sealed class PrefabLoader : MonoBehaviour
{
    class ParticlePrefabCache
    {
        public List<Renderer> renderCache = new List<Renderer>();
        public List<ParticleSystem> psCache = new List<ParticleSystem>();
        public List<ParticleSystemRenderer> psrCache = new List<ParticleSystemRenderer>();
        public List<string> shaderNameCache = new List<string>();
    }

    public GameObject[] prefabs;
    public Vector3[] scale;
    public Vector3[] position;
    public Vector3[] rotation;
    public int renderQ = 0;
    public float scaleFactor = 1;
    [HideInInspector]
    public bool changeToClipShader = false;
    public bool Done { get; private set; }

    private Transform transCache;

    private LayerMask effectLayer;

    private Dictionary<int, ParticlePrefabCache>  prefabCaches = new Dictionary<int, ParticlePrefabCache>();

    private List<GameObject> spawnedGoes = new List<GameObject>();

    void Awake()
    {
        effectLayer = LayerMask.NameToLayer("RoleEffect");
        //for (int i = 0; i < prefabs.Length; ++i)
        //{
        //    ObjectPool.CreatePool(prefabs[i], 1);
        //}
        transCache = transform;
        LoadPrefab();
    }

    void OnEnable()
    {
        DeactivePrefab();
        Invoke("SetPrefabInfo", 0.3f);
    }

    void LoadPrefab()
	{
        for (int i = 0; i < prefabs.Length; ++i)
		{
			if (prefabs[i] == null)
				continue;
			//var go = prefabs[i].Spawn();
   //         spawnedGoes.Add(go);
   //         var trans = go.transform;
   //         trans.SetParent(transCache);
   //         trans.localScale = scale.Length == 0 ? Vector3.one : scale[i];
   //         trans.localPosition = position.Length == 0 ? Vector3.zero : position[i];
   //         trans.localRotation = rotation.Length == 0 ? Quaternion.identity : Quaternion.Euler(rotation[i]);
        }
	}

    void DeactivePrefab()
    {
        for (int i = 0; i < spawnedGoes.Count; ++i)
        {
            if (spawnedGoes[i] != null)
                spawnedGoes[i].SetActive(false);
        }
    }

    void SetPrefabInfo()
    {
        if (spawnedGoes == null) return;
        for (int i = 0; i < spawnedGoes.Count; ++i)
        {
            var go = spawnedGoes[i];
            if (go == null) continue;
            go.SetActive(true);
            var trans = go.transform;
            //NGUITools.SetLayer(go, effectLayer);

            ParticlePrefabCache cache;
            if (!prefabCaches.TryGetValue(go.GetInstanceID(), out cache))
            {
                cache = new ParticlePrefabCache();
                cache.psCache.AddRange(trans.GetComponentsInChildren<ParticleSystem>(true));
                cache.renderCache.AddRange(trans.GetComponentsInChildren<Renderer>(true));
                cache.psrCache.AddRange(trans.GetComponentsInChildren<ParticleSystemRenderer>(true));
                prefabCaches.Add(go.GetInstanceID(), cache);

                for (int k = 0; k < cache.renderCache.Count; k++)
                {
                    cache.shaderNameCache.Add(cache.renderCache[k].material.shader.name);
                }
            }
            for (int k = 0; k < cache.psCache.Count; k++)
            {
                cache.psCache[k].transform.localScale = Vector3.one;
            }
            if (renderQ > 0)
            {
                //UIRoot uiRoot = NGUITools.GetRoot(gameObject).GetComponent<UIRoot>();
                //if (!uiRoot) continue;
                //for (int k = 0; k < cache.psCache.Count; k++)
                //{
                //    cache.psCache[k].transform.localScale = uiRoot.transform.localScale * scaleFactor;
                //}

                for (int j = 0; j < cache.renderCache.Count; j++)
                {
                    if (changeToClipShader)
                        cache.renderCache[j].material.shader = Shader.Find("Custom/Particle Texture Area Clip");
                    else
                        cache.renderCache[j].material.shader = Shader.Find(cache.shaderNameCache[j]);

                    cache.renderCache[j].material.renderQueue = renderQ;
                    if (gameObject.layer == LayerMask.NameToLayer("UI"))
                        cache.renderCache[j].gameObject.layer = LayerMask.NameToLayer("UI");
                    else
                        cache.renderCache[j].gameObject.layer = LayerMask.NameToLayer("UIModel");
                }

                for (int j = 0; j < cache.psrCache.Count; ++j)
                {
                    if (cache.psrCache[j].sortingOrder > 0)
                    {
                        cache.psrCache[j].sortingFudge = cache.psrCache[j].sortingOrder * -100;
                        cache.psrCache[j].sortingOrder = 0;
                    }
                }
            }
        }
        Done = true;
    }

    /// <summary>
    /// 设置特效Layer层次
    /// </summary>
    /// <param name="layerName"></param>
    public void SetEffectLayer(string layerName)
    {
        effectLayer = LayerMask.NameToLayer(layerName);
    }

    public void ResetRenderQ()
    {
        Invoke("delayCall", 0.3f); 
    }

    void delayCall()
    {
        ParticlePrefabCache cache;
        if (!prefabCaches.TryGetValue(transCache.GetInstanceID(), out cache))
        {
            cache = new ParticlePrefabCache();
            cache.renderCache.AddRange(transCache.GetComponentsInChildren<Renderer>(true));
            prefabCaches.Add(transCache.GetInstanceID(), cache);
        }
        for (int j = 0; j < cache.renderCache.Count; j++)
        {
            cache.renderCache[j].material.renderQueue = renderQ;
        }
    }

    void OnDisable()
    {
        CancelInvoke("SetPrefabInfo");
        CancelInvoke("delayCall");
        DeactivePrefab();
        Done = false;
        effectLayer = LayerMask.NameToLayer("RoleEffect");
    }

    private void OnDestroy()
    {
        spawnedGoes.Clear();
        prefabCaches.Clear();
    }
}
