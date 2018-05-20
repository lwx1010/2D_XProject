using UnityEngine;
using System.Collections;
using LuaFramework;
using System.IO;
using System.Collections.Generic;
using System;
using RSG;

namespace Riverlake
{
    public class BundleInfo
    {
        public AssetBundle bundle
        {
            get
            {
                _refCount++;
                lastTimeVisited = Time.realtimeSinceStartup;
                var deps = AssetBundleManager.Instance.allBundleManifest.GetAllDependencies(_bundleName);
                for (int i = 0; i < deps.Length; ++i)
                {
                    if (AssetBundleManager.Instance.bundleLoaded.ContainsKey(deps[i]))
                    {
                        AssetBundleManager.Instance.bundleLoaded[deps[i]].refCount++;
                    }
                    else if (AssetBundleManager.Instance.bundleLoadingQueue.Contains(deps[i]))
                    {
                        continue;
                    }
                    else
                    {
                        string assetBundlePath = Util.DataPath + "AssetBundle/";
                        var temp = AssetBundle.LoadFromFile(assetBundlePath + LuaConst.osDir +"/" + deps[i]);
                        AssetBundleManager.Instance.bundleLoaded.Add(deps[i], new BundleInfo(temp, deps[i]));
                    }
                }
                return _bundle;
            }
            private set
            {
                _bundle = value;
            }
        }

        private AssetBundle _bundle;
        public int refCount
        {
            get { return _refCount; }
            set
            {
                if (value > _refCount) lastTimeVisited = Time.realtimeSinceStartup;
                _refCount = value;
            }
        }

        private int _refCount;

        public float timeCreated { get; private set; }
        public float lastTimeVisited { get; private set; }
        public string _bundleName { get; private set; }

        public BundleInfo(AssetBundle bundle, string name)
        {
            this._bundle = bundle;
            this._refCount = 1;
            this._bundleName = name;
            timeCreated = Time.realtimeSinceStartup;
            lastTimeVisited = Time.realtimeSinceStartup;
        }

        public bool Unload()
        {

            if (!AssetBundleManager.Instance.preloadAssets.Contains(_bundleName))
            {
                LuaInterface.Debugger.Log(string.Format("<color=green>assetbundle unload: {0}</color>", _bundleName));
#if !UNITY_EDITOR
                if (_bundle != null) _bundle.Unload(true);
                _bundle = null;
                return true;
#else
            return false;
#endif
            }
            return false;
        }
    }

    public sealed class AssetBundleManager : Singleton<AssetBundleManager>
    {
        public AssetBundleManifest allBundleManifest { get; private set; }

        /// <summary>
        /// 正在加载的bundle资源
        /// </summary>
        public Queue<string> bundleLoadingQueue = new Queue<string>();
        /// <summary>
        /// 已经加载的bundle资源
        /// </summary>
        public Dictionary<string, BundleInfo> bundleLoaded = new Dictionary<string, BundleInfo>();
        /// <summary>
        /// asset资源map映射表
        /// </summary>
        public Dictionary<string, string> assetbundleMap = new Dictionary<string, string>();

        private Dictionary<string, HashSet<GameObject>> assetIntances = new Dictionary<string, HashSet<GameObject>>();

        public delegate void OnProgressChanged(float value);

        public OnProgressChanged onProgressChange;

        public List<string> preloadAssets { get; private set; }

        private AssetBundleCreateRequest preloadAbcr = null;

        private int curIndex = 0;

        const float MAXDELAY_CLEAR_TIME = 10 * 60;
        private float delayClearTime;

        private string assetBundlePath;

        void Awake()
        {
            delayClearTime = MAXDELAY_CLEAR_TIME;
            assetBundlePath = Util.DataPath + "AssetBundle/";
        }

        public void AssetBundleInit(Action callback)
        {
            LoadAssetbundleMap();
            LoadManifest();
            StartCoroutine(PreloadAsset(callback));
        }

        private void LoadAssetbundleMap()
        {
            var content = File.ReadAllBytes(string.Format("{0}{1}/bundlemap.ab", assetBundlePath, LuaConst.osDir));
            var decryptoStrs = Encoding.GetString(Riverlake.Crypto.Crypto.Decode(content)).Split('\n');
            preloadAssets = new List<string>();
            for (int i = 0; i < decryptoStrs.Length; ++i)
            {
                if (!string.IsNullOrEmpty(decryptoStrs[i]))
                {
                    var temp = decryptoStrs[i].Split('|');
                    Debug.Assert(temp.Length == 3);
                    if (assetbundleMap.ContainsKey(temp[0]))
                    {
                        Debug.LogError(string.Format("{0} is already exists", temp[0]));
                        throw new ArgumentException();
                    }
                    assetbundleMap.Add(temp[0], temp[1]);
                    if (temp[2] == "1" && !preloadAssets.Contains(temp[1]))
                        preloadAssets.Add(temp[1]);
                }
            }
        }

        private void LoadManifest()
        {
            string path = string.Format("{0}{1}/{2}", assetBundlePath, LuaConst.osDir, LuaConst.osDir+".ab");
            if (!File.Exists(path))
            {
                Debug.LogError(string.Format("no manifest file exists in {0}", path));
                return;
            }
            var allBundle = AssetBundle.LoadFromFile(path);
            if (allBundle)
            {
                allBundleManifest = allBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                allBundle.Unload(false);
                var bundles = allBundleManifest.GetAllAssetBundles();
                
                for (int i = 0; i < bundles.Length; ++i)
                {
                    if (preloadAssets.Contains(bundles[i]))
                    {
                        var deps = allBundleManifest.GetAllDependencies(bundles[i]);
                        for (int k = 0; k < deps.Length; ++k)
                        {            
                            if (!preloadAssets.Contains(deps[k])) preloadAssets.Add(deps[k]);
                        }
                        if (!preloadAssets.Contains(bundles[i])) preloadAssets.Add(bundles[i]);
                    }
                }
            }
        }

        public IEnumerator PreloadAsset(Action callback)
        {
            for (int i = 0; i < preloadAssets.Count; ++i)
            {
                curIndex = i;
                string path = string.Format("{0}/{1}", assetBundlePath + LuaConst.osDir, preloadAssets[i]);
                if (preloadAssets[i].CustomEndsWith(".ab") && File.Exists(path))
                {
                    preloadAbcr = AssetBundle.LoadFromFileAsync(path);
                    yield return preloadAbcr;
                    if (!bundleLoaded.ContainsKey(preloadAssets[i]))
                        bundleLoaded.Add(preloadAssets[i], new BundleInfo(preloadAbcr.assetBundle, preloadAssets[i]));
                    preloadAbcr = null;
                }
            }
            preloadAssets.Clear();
            if (callback != null) callback();
        }

        void Update()
        {
            if (preloadAbcr != null)
            {
                if (onProgressChange != null)
                    onProgressChange((float)(curIndex + 1 + preloadAbcr.progress) / (float)preloadAssets.Count);
            }
            delayClearTime -= Time.deltaTime;
            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                delayClearTime = MAXDELAY_CLEAR_TIME;
            }
            if (delayClearTime <= 0)
            {
                delayClearTime = MAXDELAY_CLEAR_TIME;
                GC.Collect();
            }
        }

        public AssetBundle TryToGetBundle(string name)
        {
            AssetBundle bundle = null;
            try
            {
                string key;
                if (!assetbundleMap.TryGetValue(name, out key))
                {
                    Debug.LogWarning(string.Format("{0} has no assetbundle resource", name));
                }
                else
                {
                    bundle = GetBundleFromLoaded(key);
                    if (bundle == null)
                    {
                        if (bundleLoadingQueue.Contains(key))
                        {
                            Debug.LogWarning(string.Format("资源正在异步加载中: {0}", key));
                            return null;
                        }
                        else
                        {
                            var deps = allBundleManifest.GetAllDependencies(key);
                            for (int i = 0; i < deps.Length; ++i)
                            {                               
                                var depBundle = GetBundleFromLoaded(deps[i]);
                                if (depBundle == null)
                                {
                                    if (bundleLoadingQueue.Contains(deps[i]))
                                    {
                                        Debug.LogWarning(string.Format("资源正在异步加载中: {0}", key));
                                        return null;
                                    }
                                    else
                                    {
                                        var depPath = assetBundlePath  + LuaConst.osDir + deps[i];
                                        if (File.Exists(depPath))
                                        {
                                            var temp = AssetBundle.LoadFromFile(depPath);
                                            bundleLoaded.Add(deps[i], new BundleInfo(temp, deps[i]));
                                        }
                                        else
                                        {
                                            throw new FileNotFoundException(string.Format("File {0} not exists", depPath));
                                        }
                                    }
                                }
                            }
                            var path = assetBundlePath + key;
                            if (File.Exists(path))
                            {
                                if (!bundleLoaded.ContainsKey(key))
                                {
                                    bundle = AssetBundle.LoadFromFile(path);
                                    bundleLoaded.Add(key, new BundleInfo(bundle, key));
                                }
                                else
                                {
                                    throw new ArgumentException(string.Format("key {0} is already exist in bundleLoaded", key));
                                }
                            }
                            else
                            {
                                throw new FileNotFoundException(string.Format("File {0} not exists", path));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("Get Assetbundle Exception, Name: {0}", name));
                Debug.LogException(e);
                return null;
            }
            return bundle;
        }

        public IEnumerator TryToGetBundleAsync(string name, Action<AssetBundle> callback)
        {
            AssetBundle bundle = null;
            string key;
            if (!assetbundleMap.TryGetValue(name, out key))
            {
                Debug.LogError(string.Format("{0} has no assetbundle resource", name));
            }
            else
            {
                while (bundleLoadingQueue.Contains(key))
                    yield return Yielders.EndOfFrame;
                bundle = GetBundleFromLoaded(key);
                if (bundle == null)
                {
                    bundleLoadingQueue.Enqueue(key);
                    var deps = allBundleManifest.GetAllDependencies(key);
                    for (int i = 0; i < deps.Length; ++i)
                    {
                        while (bundleLoadingQueue.Contains(deps[i]))
                            yield return Yielders.EndOfFrame;
                        var depBundle = GetBundleFromLoaded(deps[i]);
                        if (depBundle == null)
                        {
                            var depPath = assetBundlePath + deps[i];
                            if (File.Exists(depPath))
                            {
                                bundleLoadingQueue.Enqueue(deps[i]);
                                var abcr = AssetBundle.LoadFromFileAsync(depPath);
                                yield return abcr;
                                bundleLoadingQueue.Dequeue();
                                bundleLoaded.Add(deps[i], new BundleInfo(abcr.assetBundle, deps[i]));
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("{0} not exists", depPath));
                            }
                        }
                    }
                    var path = assetBundlePath + key;
                    if (File.Exists(path))
                    {
                        var mainAbcr = AssetBundle.LoadFromFileAsync(path);
                        yield return mainAbcr;
                        bundle = mainAbcr.assetBundle;
                        if (!bundleLoaded.ContainsKey(key))
                            bundleLoaded.Add(key, new BundleInfo(bundle, key));
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("{0} not exists", path));
                    }
                    bundleLoadingQueue.Dequeue();
                }
            }
            if (callback != null) callback(bundle);
        }

        AssetBundle GetBundleFromLoaded(string bundleName)
        {
            BundleInfo bundleInfo;
            AssetBundle bundle = null;
            if (bundleLoaded.TryGetValue(bundleName, out bundleInfo))
            {
                bundle = bundleInfo.bundle;
            }
            return bundle;
        }

        public IPromise<T> LoadAssetAsync<T>(string assetName, string extension)
            where T : UnityEngine.Object
        {
            return new Promise<T>((resolved, reject) =>
            {
                new Promise<AssetBundle>((s, j) =>
                {
                    StartCoroutine(TryToGetBundleAsync(assetName, s));
                }).Then((bundle) =>
                {
                    StartCoroutine(_loadBundleAsset(assetName, extension, bundle, resolved));
                }).Catch((e) =>
                {
                    reject(e);
                    Debug.LogException(e);
                });
            });
        }

        public IPromise<GameObject> LoadPrefabAsync(string assetName)
        {
            assetName = assetName.ToLower();
            return LoadAssetAsync<GameObject>(assetName, "prefab").Then((go) =>
           {
               if (go == null)
               {
                   string assetPath = string.Format("assets/res/{0}.prefab", assetName);
                   throw new Exception(string.Format("load prefab error: {0}", assetPath));
               }
               go.transform.GetOrAddComponent<AssetWatcher>().assetName = assetName;
           }).Catch(e =>
           {
               Debug.LogException(e);
           });
        }

        #region -------------同步加载----------------------
        /// <summary>
        /// 同步加载Prefab文件
        /// </summary>
        /// <returns></returns>
        public GameObject LoadPrefab(string name)
        {
            name = name.ToLower();
            var assetName = string.Format("{0}.prefab", name);
            GameObject obj = LoadAssets<GameObject>(assetName);
            if (obj != null)
            {
                obj.transform.GetOrAddComponent<AssetWatcher>().assetName = name;
            }
            else
            {
                Debug.LogWarning(string.Format("load prefab error: {0}, {1}", name, assetName));
            }
            return obj;
        }

        public Sprite LoadSprite(string spriteName, string atlasName)
        {
            var assetName = atlasName.ToLower();
            string ext = Path.GetExtension(assetName);
            if (!string.IsNullOrEmpty(ext))
                assetName = assetName.Replace(ext, "");

            AssetBundle bundle = TryToGetBundle(assetName);
            if (bundle == null) return null;
            if (!assetName.CustomStartsWith("assets/res/"))
                assetName = string.Format("assets/res/{0}", atlasName);

            var assets = bundle.LoadAllAssets();
            if (assets == null)
                Debug.LogWarning(string.Format("Cant find ab: {0}", assetName));
            foreach (var asset in assets)
            {
                if (asset is Sprite)
                {
                    if (asset.name.Equals(spriteName))
                        return asset as Sprite;
                }
            }
            return null;
        }

        /// <summary>
        /// 加载字节数组
        /// </summary>
        /// <param name="path">无.bytes后缀的路径</param>
        /// <returns></returns>
        public byte[] LoadBytes(string path)
        {
            path = path.ToLower();
            TextAsset textAss = LoadAssets<TextAsset>(string.Format("{0}.bytes", path));
            if (textAss != null)
                return textAss.bytes;
            return null;
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadAssets<T>(string path) where T : UnityEngine.Object
        {
            string assetName = path;
            string ext = Path.GetExtension(assetName);
            if (!string.IsNullOrEmpty(ext))
                assetName = assetName.Replace(ext, "");

            AssetBundle bundle = TryToGetBundle(assetName);
            if (bundle == null) return null;
            if (!assetName.CustomStartsWith("assets/res/"))
                assetName = string.Format("assets/res/{0}", path);

            var asset = bundle.LoadAsset(assetName) as T;
            if (asset == null)
                Debug.LogWarning(string.Format("Cant find ab: {0}", assetName));
            return asset;
        }
        #endregion

        public IPromise<AudioClip> LoadAudioClipBundleAsync(string assetName, string extension)
        {
            assetName = assetName.ToLower();
            return LoadAssetAsync<AudioClip>(assetName, extension);
        }

        private IEnumerator _loadBundleAsset<T>(string goName, string extension, AssetBundle bundle, Action<T> callback)
            where T : UnityEngine.Object
        {
            if (bundle != null)
            {
                var assetName = string.Format("assets/res/{0}.{1}", goName, extension);
                var abcr = bundle.LoadAssetAsync(assetName);
                yield return abcr;
                var mainAsset = (T)abcr.asset;
                callback(mainAsset);
            }
            else
                callback(default(T));
        }

        public void LoadAsset(string name, GameObject go)
        {
            HashSet<GameObject> tempList;
            if (!assetIntances.TryGetValue(name, out tempList))
            {
                var hashSet = new HashSet<GameObject>();
                hashSet.Add(go);
                assetIntances.Add(name, hashSet);
            }
            else
            {
                if (!tempList.Contains(go))
                    tempList.Add(go);
            }
        }

        public void UnloadAsset(string assetName, GameObject go)
        {
            bool unload = false;
            HashSet<GameObject> tempList;
            if (assetIntances.TryGetValue(assetName, out tempList))
            {
                tempList.Remove(go);
                unload = tempList.Count == 0;
            }
            else
                unload = true;

            if (unload)
            {
                var key = assetbundleMap[assetName];
                var deps = allBundleManifest.GetAllDependencies(key);
                for (int i = 0; i < deps.Length; ++i)
                {
                    BundleInfo info;
                    if (bundleLoaded.TryGetValue(deps[i], out info))
                        info.refCount--;
                }
                BundleInfo info1;
                if (bundleLoaded.TryGetValue(key, out info1)) info1.refCount--;

                if (tempList != null) assetIntances.Remove(assetName);
            }
        }

        public void Clear()
        {
            var keys = new List<string>();
            foreach (var key in bundleLoaded.Keys)
            {
                if (bundleLoaded[key].refCount <= 0)
                {
                    if (bundleLoaded[key].Unload())
                        keys.Add(key);
                }
            }
            for (int i = 0; i < keys.Count; ++i)
            {
                bundleLoaded.Remove(keys[i]);
            }
            Util.ClearMemory();
        }
    }
}