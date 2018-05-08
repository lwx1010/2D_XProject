using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Riverlake
{
    public sealed class ObjectPool : MonoBehaviour
    {
        public enum StartupPoolMode { Awake, Start, CallManually };

        [System.Serializable]
        public class StartupPool
        {
            public int size;
            public GameObject prefab;
        }

        static ObjectPool _instance;
        static List<GameObject> tempList = new List<GameObject>();

        static Vector3 outPos = new Vector3(50000, 50000, 50000);

        public const int POOL_INIT_SIZE = 5;

        public Dictionary<string, GameObject> pooledObjNames = new Dictionary<string, GameObject>();

        Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

        public StartupPoolMode startupPoolMode;
        public StartupPool[] startupPools;

        bool startupPoolsCreated;

        void Awake()
        {
            _instance = this;
            if (startupPoolMode == StartupPoolMode.Awake)
                CreateStartupPools();
        }

        void Start()
        {
            if (startupPoolMode == StartupPoolMode.Start)
                CreateStartupPools();
        }

        void OnDestroy()
        {
            tempList.Clear();
            pooledObjNames.Clear();
            pooledObjects.Clear();
            spawnedObjects.Clear();
        }

        public GameObject PushToPool(string prefabPath, int initSize, Transform parent = null)
        {
            return PushToPool(prefabPath, initSize, parent, 0, 0, 0, 0, 0, 0);
        }

        public GameObject PushToPool(string prefabPath, int initSize, Transform parent, float x, float y, float z)
        {
            return PushToPool(prefabPath, initSize, parent, x, y, z, 0, 0, 0);
        }

        public GameObject PushToPool(string prefabPath, int initSize, Transform parent, float x, float y, float z, float rx, float ry, float rz)
        {
            GameObject prefab = null;
            GameObject tempGo = null;
            if (!instance.pooledObjNames.TryGetValue(prefabPath, out tempGo))
            {
                //var go = LuaFramework.Util.LoadPrefab(prefabPath);
                //if (go != null)
                //{
                //    CreatePool(go, initSize);
                //    instance.pooledObjNames.Add(prefabPath, go);
                //    prefab = go.Spawn(parent, new Vector3(x, y, z), Quaternion.Euler(rx, ry, rz));
                //}
            }
            else
            {
                prefab = tempGo.Spawn(parent, new Vector3(x, y, z), Quaternion.Euler(rx, ry, rz));
            }
            return prefab;
        }

        public IEnumerator AsyncPushAvatarToPool(string prefabPath, Action<GameObject, string> callback)
        {
            AsyncPushToPool(prefabPath, 2, null, 0, 0, 0, callback, true);
            yield return 0;
        }

        public void AsyncPushToPool(string prefabPath, int initSize, Transform parent, float x, float y, float z, Action<GameObject, string> callback)
        {
            AsyncPushToPool(prefabPath, initSize, parent, x, y, z, callback, false);
        }

        void AsyncPushToPool(string prefabPath, int initSize, Transform parent, float x, float y, float z, Action<GameObject, string> callback, bool isAvatar)
        {
            GameObject prefab = null;
            GameObject tempGo = null;
            if (!instance.pooledObjNames.TryGetValue(prefabPath, out tempGo))
            {
                instance.pooledObjNames.Add(prefabPath, null);
                //StartCoroutine(LuaFramework.Util.LoadPrefabAsync(prefabPath, (go, path) =>
                //{
                //    if (go != null)
                //    {
                //        CreatePool(go, initSize);
                //        instance.pooledObjNames[prefabPath] = go;
                //        prefab = go.Spawn(parent, new Vector3(x, y, z));
                //        SetPrefabLayer(prefabPath, prefab, isAvatar);
                //    }
                //    else
                //    {
                //        instance.pooledObjNames.Remove(prefabPath);
                //    }
                //    if (callback != null)
                //        callback(prefab, path);
                //}));
            }
            else if (tempGo == null)
            {
                StartCoroutine(WaitForPoolComplete(prefabPath, parent, x, y, z, callback, isAvatar));
            }
            else
            {
                prefab = tempGo.Spawn(parent, new Vector3(x, y, z));
                if (callback != null)
                    callback(prefab, prefabPath);
            }
        }

        IEnumerator WaitForPoolComplete(string prefabPath, Transform parent, float x, float y, float z, Action<GameObject, string> callback, bool isAvatar)
        {
            GameObject tempGo;
            while (true)
            {
                if (!instance.pooledObjNames.TryGetValue(prefabPath, out tempGo))
                {
                    AsyncPushToPool(prefabPath, 1, parent, x, y, z, callback);
                    yield break;
                }
                else if (tempGo != null)
                    break;
                yield return Yielders.EndOfFrame;
            }

            GameObject prefab = null;
            if (tempGo != null)
                prefab = tempGo.Spawn(parent, new Vector3(x, y, z));
            if (callback != null)
                callback(prefab, prefabPath);
        }

        public static void CreateStartupPools()
        {
            if (!instance.startupPoolsCreated)
            {
                instance.startupPoolsCreated = true;
                var pools = instance.startupPools;
                if (pools != null && pools.Length > 0)
                    for (int i = 0; i < pools.Length; ++i)
                        CreatePool(pools[i].prefab, pools[i].size);
            }
        }

        public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
        {
            CreatePool(prefab.gameObject, initialPoolSize);
        }
        public static void CreatePool(GameObject prefab, int initialPoolSize)
        {
            if (prefab != null && ObjectPoolDontDestroy.CountPooled(prefab) > 0)
                return;
            if (prefab != null && !instance.pooledObjects.ContainsKey(prefab))
            {
                //if (instance.pooledObjects.Count >= 100)
                //{
                //    PopOld();
                //}
                var list = new List<GameObject>();
                instance.pooledObjects.Add(prefab, list);

                if (initialPoolSize > 0)
                {
                    bool active = prefab.activeSelf;
                    prefab.SetActive(false);
                    Transform parent = instance.transform;
                    while (list.Count < initialPoolSize)
                    {
                        var obj = (GameObject)UnityEngine.Object.Instantiate(prefab);
                        obj.transform.parent = parent;
                        list.Add(obj);
                    }
                    prefab.SetActive(active);
                }
            }
        }

        public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position) where T : Component
        {
            return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            List<GameObject> list;
            Transform trans;
            GameObject obj;
            if (instance.pooledObjects.TryGetValue(prefab, out list))
            {
                obj = null;
                if (list.Count > 0)
                {
                    while (obj == null && list.Count > 0)
                    {
                        obj = list[0];
                        list.RemoveAt(0);
                    }
                    if (obj != null)
                    {
                        trans = obj.transform;
                        trans.parent = parent;
                        trans.localPosition = position;
                        trans.localRotation = rotation;
                        
                        obj.SetActive(true);
                        instance.spawnedObjects.Add(obj, prefab);
                        return obj;
                    }
                }

                obj = (GameObject)UnityEngine.Object.Instantiate(prefab);
                trans = obj.transform;
                trans.parent = parent;
                trans.localPosition = position;
                trans.localRotation = rotation;
                instance.spawnedObjects.Add(obj, prefab);
                return obj;
            }
            else
            {
                obj = (GameObject)UnityEngine.Object.Instantiate(prefab);
                trans = obj.GetComponent<Transform>();
                trans.parent = parent;
                trans.localPosition = position;
                trans.localRotation = rotation;
                return obj;
            }
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
        {
            return Spawn(prefab, parent, position, Quaternion.identity);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, null, position, rotation);
        }
        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position)
        {
            return Spawn(prefab, null, position, Quaternion.identity);
        }
        public static GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void Recycle<T>(T obj) where T : Component
        {
            Recycle(obj.gameObject);
        }
        public static void RecycleFromLua(GameObject obj)
        {
            if (obj == null) return;
            GameObject prefab;
            if (instance.spawnedObjects.TryGetValue(obj, out prefab))
                Recycle(obj, prefab);
            else
                UnityEngine.Object.Destroy(obj);
        }
        public static void Recycle(GameObject obj)
        {
            try
            {
                if (obj == null) return;
                GameObject prefab;
                if (instance.spawnedObjects.TryGetValue(obj, out prefab))
                    Recycle(obj, prefab);
                else
                    UnityEngine.Object.DestroyImmediate(obj);
            }
            catch
            {

            }
        }
        static void Recycle(GameObject obj, GameObject prefab)
        {
            instance.pooledObjects[prefab].Add(obj);
            instance.spawnedObjects.Remove(obj);
            obj.transform.parent = null;
            obj.transform.SetParent(instance.transform);
            obj.SetActive(false);
        }

        public static void RecycleAll<T>(T prefab) where T : Component
        {
            RecycleAll(prefab.gameObject);
        }
        public static void RecycleAll(GameObject prefab)
        {
            foreach (var item in instance.spawnedObjects)
                if (item.Value == prefab)
                    tempList.Add(item.Key);
            for (int i = 0; i < tempList.Count; ++i)
                Recycle(tempList[i]);
            tempList.Clear();
        }
        public static void RecycleAll()
        {
            tempList.AddRange(instance.spawnedObjects.Keys);
            for (int i = 0; i < tempList.Count; ++i)
                Recycle(tempList[i]);
            tempList.Clear();
        }

        public static bool IsSpawned(GameObject obj)
        {
            return instance.spawnedObjects.ContainsKey(obj);
        }

        public static int CountPooled<T>(T prefab) where T : Component
        {
            return CountPooled(prefab.gameObject);
        }
        public static int CountPooled(GameObject prefab)
        {
            List<GameObject> list;
            if (instance.pooledObjects.TryGetValue(prefab, out list))
                return list.Count;
            return 0;
        }

        public static int CountSpawned<T>(T prefab) where T : Component
        {
            return CountSpawned(prefab.gameObject);
        }
        public static int CountSpawned(GameObject prefab)
        {
            int count = 0;
            foreach (var instancePrefab in instance.spawnedObjects.Values)
                if (prefab == instancePrefab)
                    ++count;
            return count;
        }

        public static int CountAllPooled()
        {
            int count = 0;
            foreach (var list in instance.pooledObjects.Values)
                count += list.Count;
            return count;
        }

        public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            List<GameObject> pooled;
            if (instance.pooledObjects.TryGetValue(prefab, out pooled))
                list.AddRange(pooled);
            return list;
        }
        public static List<T> GetPooled<T>(GameObject prefab, List<T> list, bool appendList) where T : Component
        {
            if (list == null)
                list = new List<T>();
            if (!appendList)
                list.Clear();
            List<GameObject> pooled;
            if (instance.pooledObjects.TryGetValue(prefab, out pooled))
                for (int i = 0; i < pooled.Count; ++i)
                    list.Add(pooled[i].GetComponent<T>());
            return list;
        }

        public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            foreach (var item in instance.spawnedObjects)
                if (item.Value == prefab)
                    list.Add(item.Key);
            return list;
        }
        public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if (list == null)
                list = new List<T>();
            if (!appendList)
                list.Clear();
            foreach (var item in instance.spawnedObjects)
                if (item.Value == prefab)
                    list.Add(item.Key.GetComponent<T>());
            return list;
        }

        public static void DestroyPooled(GameObject prefab)
        {
            List<GameObject> pooled;
            if (instance.pooledObjects.TryGetValue(prefab, out pooled))
            {
                for (int i = 0; i < pooled.Count; ++i)
                    GameObject.Destroy(pooled[i]);
                pooled.Clear();
            }
        }
        public static void DestroyPooled<T>(T prefab) where T : Component
        {
            DestroyPooled(prefab.gameObject);
        }

        public static void DestroyAll(GameObject prefab)
        {
            RecycleAll(prefab);
            DestroyPooled(prefab);
        }
        public static void DestroyAll<T>(T prefab) where T : Component
        {
            DestroyAll(prefab.gameObject);
        }

        public static ObjectPool instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var obj = new GameObject("ObjectPool");
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                _instance = obj.AddComponent<ObjectPool>();
                return _instance;
            }
        }
    }

    public static class ObjectPoolExtensions
    {
        public static void CreatePool<T>(this T prefab) where T : Component
        {
            ObjectPool.CreatePool(prefab, 0);
        }
        public static void CreatePool<T>(this T prefab, int initialPoolSize) where T : Component
        {
            ObjectPool.CreatePool(prefab, initialPoolSize);
        }
        public static void CreatePool(this GameObject prefab)
        {
            ObjectPool.CreatePool(prefab, 0);
        }
        public static void CreatePool(this GameObject prefab, int initialPoolSize)
        {
            ObjectPool.CreatePool(prefab, initialPoolSize);
        }

        public static T Spawn<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPool.Spawn(prefab, parent, position, rotation);
        }
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPool.Spawn(prefab, null, position, rotation);
        }
        public static T Spawn<T>(this T prefab, Transform parent, Vector3 position) where T : Component
        {
            return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
        {
            return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab, Transform parent) where T : Component
        {
            return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab) where T : Component
        {
            return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            return ObjectPool.Spawn(prefab, parent, position, rotation);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return ObjectPool.Spawn(prefab, null, position, rotation);
        }
        public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position)
        {
            return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position)
        {
            return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Transform parent)
        {
            return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab)
        {
            return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void Recycle<T>(this T obj) where T : Component
        {
            ObjectPool.Recycle(obj);
        }
        public static void Recycle(this GameObject obj)
        {
            ObjectPool.Recycle(obj);
        }

        public static void RecycleAll<T>(this T prefab) where T : Component
        {
            ObjectPool.RecycleAll(prefab);
        }
        public static void RecycleAll(this GameObject prefab)
        {
            ObjectPool.RecycleAll(prefab);
        }

        public static int CountPooled<T>(this T prefab) where T : Component
        {
            return ObjectPool.CountPooled(prefab);
        }
        public static int CountPooled(this GameObject prefab)
        {
            return ObjectPool.CountPooled(prefab);
        }

        public static int CountSpawned<T>(this T prefab) where T : Component
        {
            return ObjectPool.CountSpawned(prefab);
        }
        public static int CountSpawned(this GameObject prefab)
        {
            return ObjectPool.CountSpawned(prefab);
        }

        public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return ObjectPool.GetSpawned(prefab, list, appendList);
        }
        public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list)
        {
            return ObjectPool.GetSpawned(prefab, list, false);
        }
        public static List<GameObject> GetSpawned(this GameObject prefab)
        {
            return ObjectPool.GetSpawned(prefab, null, false);
        }
        public static List<T> GetSpawned<T>(this T prefab, List<T> list, bool appendList) where T : Component
        {
            return ObjectPool.GetSpawned(prefab, list, appendList);
        }
        public static List<T> GetSpawned<T>(this T prefab, List<T> list) where T : Component
        {
            return ObjectPool.GetSpawned(prefab, list, false);
        }
        public static List<T> GetSpawned<T>(this T prefab) where T : Component
        {
            return ObjectPool.GetSpawned(prefab, null, false);
        }

        public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return ObjectPool.GetPooled(prefab, list, appendList);
        }
        public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list)
        {
            return ObjectPool.GetPooled(prefab, list, false);
        }
        public static List<GameObject> GetPooled(this GameObject prefab)
        {
            return ObjectPool.GetPooled(prefab, null, false);
        }

        public static void DestroyPooled(this GameObject prefab)
        {
            ObjectPool.DestroyPooled(prefab);
        }
        public static void DestroyPooled<T>(this T prefab) where T : Component
        {
            ObjectPool.DestroyPooled(prefab.gameObject);
        }

        public static void DestroyAll(this GameObject prefab)
        {
            ObjectPool.DestroyAll(prefab);
        }
        public static void DestroyAll<T>(this T prefab) where T : Component
        {
            ObjectPool.DestroyAll(prefab.gameObject);
        }
    }
}



