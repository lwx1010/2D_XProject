/********************************************************************************
** Author： LiangZG
** Email :  game.liangzg@foxmail.com
*********************************************************************************/
using PathologicalGames;
using UnityEngine;

namespace AL.PoolCache
{
    /// <summary>
    /// 异步缓存池
    /// </summary>
    public class CachePoolAsync
    {
        private SpawnPool pool;

        private int limitCount = 100;

        private string poolName;
        public bool DebugLog
        {
            set { selfPool.logMessages = value; }
        }

        public CachePoolAsync(string poolName)
        {
            this.poolName = poolName;
        }


        private SpawnPool selfPool
        {
            get
            {
                if (pool == null)
                {
                    pool = PoolManager.Pools.Create(poolName);
                    pool.group.localPosition = Vector3.one * 10000;
                    pool.dontDestroyOnLoad = true;
                }
                return pool;
            }
        }

        public CachePoolAsync(string poolName, int limitCount)
            : this(poolName)
        {
            this.limitCount = limitCount;
        }

        /// <summary>
        /// 获得缓存的Prefab
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public GameObject GetCachePrefab(string prefabName)
        {
            PrefabPool prefabPool = null;
            if (selfPool.prefabPools.TryGetValue(prefabName, out prefabPool))
            {
                return prefabPool.prefabGO;
            }
            return null;
        }

        /// <summary>
        /// 获得缓存的Component对应，主要针对AudioSource,ParticalSystem
        /// </summary>
        /// <param name="prefabName">资源的PrefabName</param>
        /// <returns></returns>
        public T GetCacheComponent<T>(string prefabName) where T : Component
        {
            PrefabPool prefabPool = null;
            if (selfPool.prefabPools.TryGetValue(prefabName, out prefabPool))
            {
                return prefabPool.prefabGO.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 获取已存在的Prefab Pools对应
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public PrefabPool GetPrefabPool(string prefabName)
        {
            PrefabPool prefabPool = null;
            if (!selfPool.prefabPools.TryGetValue(prefabName, out prefabPool))
            {
                return null;
            }
            return prefabPool;
        }

        /// <summary>
        /// 获得一个PrefabPool , 如果不存在，则创建
        /// </summary>
        /// <param name="prefab">对应缓存的原始Prefab对象</param>
        /// <returns></returns>
        public PrefabPool SpawnPrefabPool(GameObject prefab)
        {
            PrefabPool prefabPool = null;
            if (!selfPool.prefabPools.TryGetValue(prefab.name, out prefabPool))
            {
                PrefabPool newPrefabPool = new PrefabPool(prefab.transform);
                newPrefabPool.limitFIFO = true;
                newPrefabPool.limitInstances = true;
                newPrefabPool.limitAmount = limitCount;

                newPrefabPool.cullDespawned = true;
                newPrefabPool.cullAbove = limitCount/2;

                selfPool.CreatePrefabPool(newPrefabPool);

                prefabPool = newPrefabPool;
            }
            return prefabPool;
        }

        public Transform Spawn(GameObject prefab, Transform parent)
        {
            PrefabPool prefabPool = SpawnPrefabPool(prefab);
            return selfPool.Spawn(prefabPool.prefab, parent);
        }


        public AudioSource Spawn(AudioSource audio, Transform parent)
        {
            Transform instance = Spawn(audio.gameObject, parent);
            if (instance != null)
                return instance.GetComponent<AudioSource>();
            return null;
        }

        public ParticleSystem Spawn(ParticleSystem partical, Transform parent)
        {
            Transform instance = Spawn(partical.gameObject, parent);
            if (instance != null)
                return instance.GetComponent<ParticleSystem>();
            return null;
        }

        public void Despawn(Transform instance)
        {
            if (pool == null) return;
            pool.Despawn(instance, pool.transform);
        }

        public void Despawn(AudioSource instance)
        {
            Despawn(instance.transform);
        }

        public void Despawn(ParticleSystem instance)
        {
            Despawn(instance.transform);
        }

        public void Clear()
        {
            if (pool == null) return;
            pool.DespawnAll();
        }

        public void Destroy()
        {
            if(pool != null)
                PoolManager.Pools.Destroy(this.pool.poolName);
            pool = null;
        }

    }
}