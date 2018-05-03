using LuaInterface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riverlake.Resources
{
    /// <summary>
    /// 场景资源加载器
    /// </summary>
    public class SceneLoader : ALoadOperation
    {

        private AsyncOperation loading;
        /// <summary>
        /// 是否加载完成就立即切换场景
        /// </summary>
        private bool activeImmediate;

        private LoadSceneMode loadSceneMode;
        public AsyncOperation AsyncSceneLoader
        {
            get { return loading; }
        }

        public SceneLoader(string path , bool sceneActivaImmediate) : base(path)
        {
            activeImmediate = sceneActivaImmediate;
            loadSceneMode = LoadSceneMode.Single;
        }

        public SceneLoader(string path, bool sceneActivaImmediate, LoadSceneMode loadMode) : base(path)
        {
            activeImmediate = sceneActivaImmediate;
            this.loadSceneMode = loadMode;
        }
        
        public override void OnLoad()
        {
            loading = SceneManager.LoadSceneAsync(this.assetPath , loadSceneMode);
            loading.allowSceneActivation = false;
        }

        public override bool MoveNext()
        {
            if (!hasLoaded)
            {
                hasLoaded = true;
                this.OnLoad();
            }
            progress = loading.progress;

            bool result = IsDone();
            if (result)  this.onFinishEvent();

            return !result;
        }

        public override bool IsDone()
        {
            if (loading.progress > 0.89f)
            {
                loading.allowSceneActivation = activeImmediate;
                return true;
            }
            return false;
        }
        
        [NoToLua]
        public override T GetAsset<T>()
        {
            return default(T);
        }
        
    }
}