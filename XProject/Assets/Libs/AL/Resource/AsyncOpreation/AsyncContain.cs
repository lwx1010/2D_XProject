using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;

namespace AL.Resources
{
    /// <summary>
    /// 异步加载容器,容器内部自动缓存加载器,需要手动释放
    /// 使用方法：
    ///     var async = new AsyncContain();
    ///     
    ///     async.AddLoader("xxx/press/yy.prefab");
    ///     async.AddLoader("xxx/pres/zzz.ogg" , 5);
    /// 
    ///     while(async.MoveNext())
    ///     {
    ///         if(async.CurrentLoader.IsDone()){
    ///             var loader = async.CurrentLoader;
    ///             var gameObj = GameObject.Instantiate(loader.GetAsset<GameObject>());
    ///         }
    ///         // progress
    ///         if(progressAction != null)
    ///              progressAction.Invoke(async.Progress);
    ///         yield return null;
    ///     }
    /// </summary>
    public class AsyncContain : IEnumerator
    {
        
        private class AsyncLoader
        {
            public int Weight;

            public ALoadOperation Loader;
        }
        /// <summary>
        /// 权重，占总场景资源量的比重
        /// </summary>
        public int Weight { get; private set; }

        /// <summary>
        /// 当前加载总进度
        /// </summary>
        public float Progress { get; private set; }

        private int completeWeight;

        private List<AsyncLoader> assets = new List<AsyncLoader>();

        private AsyncLoader curLoader;
        private AsyncLoader nextLoader;
        private int moveIndex;

        //缓存
        private Dictionary<string, AsyncLoader> cacheLoader;

        public AsyncContain()
        {
            cacheLoader = new Dictionary<string, AsyncLoader>();
        }
        
        /// <summary>
        /// 添加需要被加载的资源
        /// </summary>
        /// <param name="loader">异步加载器</param>
        /// <param name="weight">权重，用于计算进度</param>
        public void AddLoader(ALoadOperation loader, int weight)
        {
            Weight += weight;

            AsyncLoader asyncLoader = new AsyncLoader();
            asyncLoader.Weight = weight;
            asyncLoader.Loader = loader;

            this.assets.Add(asyncLoader);

            if (nextLoader == null)
            {
                nextLoader = asyncLoader;
                curLoader = nextLoader;
            }
                
        }

        /// <summary>
        /// 添加加载数据
        /// </summary>
        /// <param name="path">资源路径,类似"Assets/Res/XXXX.yyy"</param>
        /// <param name="weight">权重，用于计算进度</param>
        public void AddLoader(string path, int weight = 1)
        {
            this.AddLoader(ResourceManager.LoadBundleAsync(path) , weight);            
        }

        public bool MoveNext()
        {
            if (assets.Count <= 0) return false;

            bool isMoveNext = false;

            AsyncLoader temLoader = null;
            if (cacheLoader.TryGetValue(nextLoader.Loader.assetPath, out temLoader))
            {
                nextLoader.Loader.Finish(temLoader.Loader);
                isMoveNext = true;
            }else
                isMoveNext = !nextLoader.Loader.MoveNext();

            if (isMoveNext)
            {
                moveIndex++;

                curLoader = nextLoader;
                
                if (!cacheLoader.TryGetValue(curLoader.Loader.assetPath, out temLoader))
                {
                    cacheLoader[curLoader.Loader.assetPath] = curLoader;
                }
                
                if (moveIndex < assets.Count)
                {
                    completeWeight += nextLoader.Weight;
                    nextLoader = assets[moveIndex];
                }
            }

            float completedProgress = completeWeight + nextLoader.Loader.Progress * nextLoader.Weight;
            Progress = completedProgress / Weight;

            return !IsDone();
        }

        public ALoadOperation CurrentLoader
        {
            get { return curLoader.Loader; }
        }

        public object Current
        {
            get { return CurrentLoader; }
        }
        
        public bool IsDone()
        {
            return moveIndex > assets.Count;
        }

        public void Reset()
        {
            moveIndex = 0;
            assets.Clear();

            cacheLoader.Clear();

            nextLoader = null;
            curLoader = null;

            Progress = 0;
            Weight = 0;
        }
    }
}
