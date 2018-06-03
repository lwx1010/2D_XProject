using System;
using System.Collections;

namespace AL.Resources
{
    public abstract class ALoadOperation : IEnumerator
    {

        protected float progress;

        /// <summary>
        /// 加载进度
        /// </summary>
        public float Progress { get { return progress; } }
        /// <summary>
        /// 加载完成时的回调
        /// </summary>
        public Action<ALoadOperation> OnFinish;
        /// <summary>
        /// 资源路径
        /// </summary>
        public string assetPath { get; private set; }

        protected bool hasLoaded = false;

        public ALoadOperation(string path)
        {
            this.assetPath = path;
        }

        public virtual bool MoveNext()
        {
            if (!hasLoaded)
            {
                hasLoaded = true;
                this.OnLoad();
            }

            return !IsDone();
        }

        public virtual void Reset()
        {
        }

        public virtual object Current { get { return null; } }

        public abstract void OnLoad();

        public abstract bool IsDone();

        public abstract T GetAsset<T>() where T : UnityEngine.Object;

        protected virtual void onFinishEvent()
        {
            this.Finish(this);
        }

        public void Finish(ALoadOperation loader)
        {
            progress = 1.0f;

            if (OnFinish != null)
            {
                OnFinish.Invoke(loader);
                OnFinish = null;
            }
        }
    }
}
