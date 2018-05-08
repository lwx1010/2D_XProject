using System;
using UnityEngine;

namespace Riverlake.Resources
{
    /// <summary>
    /// 处理异步加载AssetBundle
    /// </summary>
    public class LoadBundleAsync : ALoadOperation
    {
        private bool isDone = false;

        private UnityEngine.Object mainAsset;
        public LoadBundleAsync(string assetName) : base(assetName)
        {

        }

        public override void OnLoad()
        {
            AssetBundleManager.Instance.LoadPrefabAsync(assetPath)
                .Then((gObj) => loadFinishCallback(gObj));
        }

        private void loadFinishCallback(GameObject gameObject)
        {
            this.mainAsset = gameObject;
            isDone = true;

            this.onFinishEvent();
        }

        public override bool IsDone()
        {
            return isDone;
        }

        public override T GetAsset<T>()
        {
            return mainAsset as T;
        }
    }
}
