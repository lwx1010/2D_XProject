using System.Collections;
using System.Collections.Generic;
using LuaFramework;

namespace AL.Resources
{
    /// <summary>
    /// 异步加载场景
    /// </summary>
    public class LoadStageAsync : IEnumerator
    {
        public class AsyncLoader
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

        private SceneLoader sceneLoader;

        public SceneLoader SceneLoader
        {
            get { return sceneLoader; }
        }

        public LoadStageAsync()
        {
        }

        public LoadStageAsync(string sceneName, bool sceneActivaImmediate)
        {
            sceneLoader = new SceneLoader(sceneName , sceneActivaImmediate);
            this.AddLoader(sceneLoader , UnityEngine.Random.Range(70, 90));
        }

        /// <summary>
        /// 添加场景资源加载器
        /// </summary>
        /// <param name="loader">加载器</param>
        /// <param name="weight">资源权重</param>
        public void AddLoader(ALoadOperation loader , int weight)
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
        /// 批量添加资源加载器
        /// </summary>
        /// <param name="loaders">加载器集合</param>
        /// <param name="weight">总资源权重</param>
        public void AddRangeLoader(ALoadOperation[] loaders , int weight)
        {
            int childWeight = weight/loaders.Length;
            Weight += childWeight * loaders.Length;

            for (int i = 0; i < loaders.Length; i++)
            {
                AsyncLoader asyncLoader = new AsyncLoader();
                asyncLoader.Weight = childWeight;
                asyncLoader.Loader = loaders[i];

                this.assets.Add(asyncLoader);
            }

            if (nextLoader == null)
            {
                nextLoader = this.assets[0];
                curLoader = nextLoader;
            }
        }
        /// <summary>
        /// 自定义排序规则
        /// </summary>
        public void Sort(IComparer<AsyncLoader> comparer)
        {
            this.assets.Sort(comparer);
        }

        public bool MoveNext()
        {
            if (assets.Count <= 0) return false;

            if (!nextLoader.Loader.MoveNext())
            {
                moveIndex++;

                curLoader = nextLoader;

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
        /// <summary>
        /// 立即激活场景切换
        /// </summary>
        public void OnActiveImmediate()
        {
            if (sceneLoader == null) return;

            sceneLoader.AsyncSceneLoader.allowSceneActivation = true;
        }

        /// <summary>
        /// 异步场景是否加载完毕
        /// </summary>
        /// <returns></returns>
        public bool IsSceneDone()
        {
            if (sceneLoader == null) return true;

            return sceneLoader.AsyncSceneLoader.isDone;
        }

        public object Current
        {
            get { return curLoader.Loader; }
        }

        public bool IsDone()
        {
            return moveIndex > assets.Count;
        }

        public void Reset()
        {
        }
    }
}