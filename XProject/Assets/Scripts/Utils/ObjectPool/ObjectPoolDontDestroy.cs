using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Riverlake
{
    public sealed class ObjectPoolDontDestroy : MonoBehaviour
    {
        [System.Serializable]
        public class StartupPool
        {
            public int size;
            public GameObject prefab;
        }

        static ObjectPoolDontDestroy _instance;
        static List<GameObject> tempList = new List<GameObject>();

        public const int POOL_INIT_SIZE = 5;

        public Dictionary<string, GameObject> pooledObjNames = new Dictionary<string, GameObject>();

        Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

        public StartupPool[] pools;

        void Awake()
        {
            _instance = this;
            _instance.transform.SetSiblingIndex(0);
            DontDestroyOnLoad(gameObject);
            StartCoroutine(CreateStartupPools());
        }

        IEnumerator CreateStartupPools()
        {
            if (pools != null && pools.Length > 0)
                for (int i = 0; i < pools.Length; ++i)
                {
                    CreatePool(pools[i].prefab, pools[i].size);
                    yield return 0;
                }
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
                //    prefab = go.SpawnDontDestroy(parent, new Vector3(x, y, z), Quaternion.Euler(rx, ry, rz));
                //}
            }
            else
            {
                prefab = tempGo.SpawnDontDestroy(parent, new Vector3(x, y, z), Quaternion.Euler(rx, ry, rz));
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
                //        prefab = go.SpawnDontDestroy(parent, new Vector3(x, y, z));
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
                prefab = tempGo.SpawnDontDestroy(parent, new Vector3(x, y, z));
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
                prefab = tempGo.SpawnDontDestroy(parent, new Vector3(x, y, z));
            if (callback != null)
                callback(prefab, prefabPath);
        }

        public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
        {
            CreatePool(prefab.gameObject, initialPoolSize);
        }
        public static void CreatePool(GameObject prefab, int initialPoolSize)
        {
            if (prefab != null && !instance.pooledObjects.ContainsKey(prefab))
            {
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

        public static T SpawnDontDestroy<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return SpawnDontDestroy(prefab.gameObject, parent, position, rotation).GetComponent<T>();
        }
        public static T SpawnDontDestroy<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return SpawnDontDestroy(prefab.gameObject, null, position, rotation).GetComponent<T>();
        }
        public static T SpawnDontDestroy<T>(T prefab, Transform parent, Vector3 position) where T : Component
        {
            return SpawnDontDestroy(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
        }
        public static T SpawnDontDestroy<T>(T prefab, Vector3 position) where T : Component
        {
            return SpawnDontDestroy(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
        }
        public static T SpawnDontDestroy<T>(T prefab, Transform parent) where T : Component
        {
            return SpawnDontDestroy(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }
        public static T SpawnDontDestroy<T>(T prefab) where T : Component
        {
            return SpawnDontDestroy(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }
        public static GameObject SpawnDontDestroy(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
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
                        obj.transform.GetOrAddComponent<ObjectPoolDontDestroyDetector>();
                        instance.spawnedObjects.Add(obj, prefab);
                        return obj;
                    }
                }
                obj = (GameObject)UnityEngine.Object.Instantiate(prefab);
                trans = obj.transform;
                trans.parent = parent;
                trans.localPosition = position;
                trans.localRotation = rotation;
                obj.transform.GetOrAddComponent<ObjectPoolDontDestroyDetector>();
                instance.spawnedObjects.Add(obj, prefab);
                return obj;
            }
            else
            {
                return prefab.Spawn();
            }
        }
        public static GameObject SpawnDontDestroy(GameObject prefab, Transform parent, Vector3 position)
        {
            return SpawnDontDestroy(prefab, parent, position, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return SpawnDontDestroy(prefab, null, position, rotation);
        }
        public static GameObject SpawnDontDestroy(GameObject prefab, Transform parent)
        {
            return SpawnDontDestroy(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(GameObject prefab, Vector3 position)
        {
            return SpawnDontDestroy(prefab, null, position, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(GameObject prefab)
        {
            return SpawnDontDestroy(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void RecycleDontDestroy<T>(T obj) where T : Component
        {
            RecycleDontDestroy(obj.gameObject);
        }

        public static void RecycleDontDestroy(GameObject obj)
        {
            try
            {
                if (obj == null) return;
                GameObject prefab;
                if (instance.spawnedObjects.TryGetValue(obj, out prefab))
                    RecycleDontDestroy(obj, prefab);
                else
                    obj.Recycle();
            }
            catch
            {

            }
        }
        static void RecycleDontDestroy(GameObject obj, GameObject prefab)
        {
            instance.pooledObjects[prefab].Add(obj);
            instance.spawnedObjects.Remove(obj);
            obj.transform.parent = null;
            obj.transform.SetParent(instance.transform);
            obj.SetActive(false);
        }

        public static void RecycleDontDestroyAll<T>(T prefab) where T : Component
        {
            RecycleDontDestroyAll(prefab.gameObject);
        }
        public static void RecycleDontDestroyAll(GameObject prefab)
        {
            foreach (var item in instance.spawnedObjects)
                if (item.Value == prefab)
                    tempList.Add(item.Key);
            for (int i = 0; i < tempList.Count; ++i)
                RecycleDontDestroy(tempList[i]);
            tempList.Clear();
        }

        public static void RecycleDontDestroyAll()
        {
            tempList.AddRange(instance.spawnedObjects.Keys);
            for (int i = 0; i < tempList.Count; ++i)
                RecycleDontDestroy(tempList[i]);
            tempList.Clear();
        }

        public static void RemoveSpawned(GameObject obj)
        {
            if (instance.spawnedObjects.ContainsKey(obj))
                instance.spawnedObjects.Remove(obj);
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
            RecycleDontDestroyAll(prefab);
            DestroyPooled(prefab);
        }
        public static void DestroyAll<T>(T prefab) where T : Component
        {
            DestroyAll(prefab.gameObject);
        }

        public static ObjectPoolDontDestroy instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var obj = new GameObject("ObjectPool(DontDestroy)");
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                _instance = obj.AddComponent<ObjectPoolDontDestroy>();
                return _instance;
            }
        }

        void OnDestroy()
        {
            pooledObjNames.Clear();
            pooledObjects.Clear();
            spawnedObjects.Clear();
        }
    }

    public static class ObjectPoolDontDestroyExtensions
    {
        public static void CreateDontDestroyPool<T>(this T prefab) where T : Component
        {
            ObjectPoolDontDestroy.CreatePool(prefab, 0);
        }
        public static void CreateDontDestroyPool<T>(this T prefab, int initialPoolSize) where T : Component
        {
            ObjectPoolDontDestroy.CreatePool(prefab, initialPoolSize);
        }
        public static void CreateDontDestroyPool(this GameObject prefab)
        {
            ObjectPoolDontDestroy.CreatePool(prefab, 0);
        }
        public static void CreateDontDestroyPool(this GameObject prefab, int initialPoolSize)
        {
            ObjectPool.CreatePool(prefab, initialPoolSize);
        }

        public static T SpawnDontDestroy<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, parent, position, rotation);
        }
        public static T SpawnDontDestroy<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, null, position, rotation);
        }
        public static T SpawnDontDestroy<T>(this T prefab, Transform parent, Vector3 position) where T : Component
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, parent, position, Quaternion.identity);
        }
        public static T SpawnDontDestroy<T>(this T prefab, Vector3 position) where T : Component
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, null, position, Quaternion.identity);
        }
        public static T SpawnDontDestroy<T>(this T prefab, Transform parent) where T : Component
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static T SpawnDontDestroy<T>(this T prefab) where T : Component
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, null, Vector3.zero, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, parent, position, rotation);
        }
        public static GameObject SpawnDontDestroy(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, null, position, rotation);
        }
        public static GameObject SpawnDontDestroy(this GameObject prefab, Transform parent, Vector3 position)
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, parent, position, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(this GameObject prefab, Vector3 position)
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, null, position, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(this GameObject prefab, Transform parent)
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static GameObject SpawnDontDestroy(this GameObject prefab)
        {
            return ObjectPoolDontDestroy.SpawnDontDestroy(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void RecycleDontDestroy<T>(this T obj) where T : Component
        {
            ObjectPoolDontDestroy.RecycleDontDestroy(obj);
        }
        public static void RecycleDontDestroy(this GameObject obj)
        {
            ObjectPoolDontDestroy.RecycleDontDestroy(obj);
        }

        public static void RecycleDontDestroyAll<T>(this T prefab) where T : Component
        {
            ObjectPoolDontDestroy.RecycleDontDestroyAll(prefab);
        }
        public static void RecycleDontDestroyAll(this GameObject prefab)
        {
            ObjectPoolDontDestroy.RecycleDontDestroyAll(prefab);
        }

        public static int CountDontDestroyPooled<T>(this T prefab) where T : Component
        {
            return ObjectPoolDontDestroy.CountPooled(prefab);
        }
        public static int CountDontDestroyPooled(this GameObject prefab)
        {
            return ObjectPoolDontDestroy.CountPooled(prefab);
        }

        public static int CountSpawnDontDestroyed<T>(this T prefab) where T : Component
        {
            return ObjectPoolDontDestroy.CountSpawned(prefab);
        }
        public static int CountSpawnDontDestroyed(this GameObject prefab)
        {
            return ObjectPoolDontDestroy.CountSpawned(prefab);
        }

        public static List<GameObject> GetSpawnDontDestroyed(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return ObjectPoolDontDestroy.GetSpawned(prefab, list, appendList);
        }
        public static List<GameObject> GetSpawnDontDestroyed(this GameObject prefab, List<GameObject> list)
        {
            return ObjectPoolDontDestroy.GetSpawned(prefab, list, false);
        }
        public static List<GameObject> GetSpawnDontDestroyed(this GameObject prefab)
        {
            return ObjectPoolDontDestroy.GetSpawned(prefab, null, false);
        }
        public static List<T> GetSpawnDontDestroyed<T>(this T prefab, List<T> list, bool appendList) where T : Component
        {
            return ObjectPoolDontDestroy.GetSpawned(prefab, list, appendList);
        }
        public static List<T> GetSpawnDontDestroyed<T>(this T prefab, List<T> list) where T : Component
        {
            return ObjectPoolDontDestroy.GetSpawned(prefab, list, false);
        }
        public static List<T> GetSpawnDontDestroyed<T>(this T prefab) where T : Component
        {
            return ObjectPoolDontDestroy.GetSpawned(prefab, null, false);
        }

        public static List<GameObject> GetDontDestroyPooled(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return ObjectPoolDontDestroy.GetPooled(prefab, list, appendList);
        }
        public static List<GameObject> GetDontDestroyPooled(this GameObject prefab, List<GameObject> list)
        {
            return ObjectPoolDontDestroy.GetPooled(prefab, list, false);
        }
        public static List<GameObject> GetDontDestroyPooled(this GameObject prefab)
        {
            return ObjectPoolDontDestroy.GetPooled(prefab, null, false);
        }

        public static void DestroyDontDestroyPooled(this GameObject prefab)
        {
            ObjectPoolDontDestroy.DestroyPooled(prefab);
        }
        public static void DestroyDontDestroyPooled<T>(this T prefab) where T : Component
        {
            ObjectPoolDontDestroy.DestroyPooled(prefab.gameObject);
        }

        public static void DestroyDontDestroyAll(this GameObject prefab)
        {
            ObjectPoolDontDestroy.DestroyAll(prefab);
        }
        public static void DestroyDontDestroyAll<T>(this T prefab) where T : Component
        {
            ObjectPoolDontDestroy.DestroyAll(prefab.gameObject);
        }

        public static void RemoveDontDestoySpawned(this GameObject prefab)
        {
            ObjectPoolDontDestroy.RemoveSpawned(prefab);
        }
    }
}



