using UnityEngine;

namespace BundleChecker
{
    /// <summary>
    /// Unity内置资源检测
    /// </summary>
    public class BuiltinChecker
    {
        public const string UNITY_BUILTIN_EXTRA = "unity_builtin_extra";
        public const string LIBRARY = "Library";
        /// <summary>
        /// 提取内置资源
        /// </summary>
        public void ExtractBuildinAssets()
        {
            
        }

        /// <summary>
        /// 是否为Resources 内置资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static bool IsExtraRes(string assetPath)
        {
            return assetPath.Contains(UNITY_BUILTIN_EXTRA) ; //|| assetPath.Contains(LIBRARY);
        }


        public static bool IsLibraryRes(string assetPath)
        {
            return assetPath.Contains(LIBRARY);
        }

        public static string GetBuiltinAssetPath(Object obj)
        {
            string suffix = obj.GetType().Name;
            if (obj is Texture2D) suffix = "png";
            else if (obj is Shader) suffix = "shader";
            else if (obj is Material) suffix = "mat";
            else if (obj is Mesh) suffix = "fbx";
            else if (obj is AnimationClip) suffix = "anim";
            else if (obj is AudioClip) suffix = "ogg";

            return string.Format("{0}.{1}", obj.name, suffix);
        }
    }
}