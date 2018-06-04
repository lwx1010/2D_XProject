using System;
using LuaInterface;

namespace AL.Resources
{
    /// <summary>
    /// 加载开发期的资源
    /// </summary>
    public class LoadAssetAsync : ALoadOperation
    {

        private UnityEngine.Object mainAsset;

        public LoadAssetAsync(string path)
            : base(path)
        {

        }

        public LoadAssetAsync(UnityEngine.Object asset , string path)
            : base(path)
        {
            mainAsset = asset;
        }

        public override void OnLoad()
        {
#if UNITY_EDITOR
            if(mainAsset == null)
            {
                mainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (mainAsset == null)
                    UnityEngine.Debug.LogError("Cant find Asset! " + assetPath);
            }
#endif
        }

        public override bool IsDone()
        {
            this.onFinishEvent();

            return true;
        }

        [NoToLua]
        public override T GetAsset<T>()
        {
            return (T)mainAsset;
        }
    }
}
