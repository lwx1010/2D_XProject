using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Config;
using RSG;
using Riverlake;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UO = UnityEngine.Object;
using UR = UnityEngine.Resources;

namespace Riverlake.Resources
{
    public static class ResourceManager
    {
        /// <summary>
        /// 载入素材
        /// </summary>
        //public AssetBundle LoadBundle(string name) {
        //    string uri = Util.DataPath + name.ToLower() + AppConst.ExtName;
        //    AssetBundle bundle = AssetBundle.LoadFromFile(uri); //关联数据的素材绑定
        //    return bundle;
        //}

        /// <summary>
        /// 异步加载Prefab
        /// </summary>
        /// <param name="assetName">无后缀的资源路径</param>
        /// <returns></returns>
        public static ALoadOperation LoadAssetAsync(string assetName)
        {
            if (AppConst.AssetBundleMode)
            {
                return new LoadBundleAsync(assetName);
            }

            string assetPath = string.Concat("Assets/Res/", assetName, ".prefab");
            return new LoadAssetAsync(assetPath);
        }

        #region -------------------同步加载资源-------------------

        /// <summary>
        /// 加载Resources目录资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private static T LoadAssets<T>(string assetName) where T : UnityEngine.Object
        {
            string resAssetPath = assetName;
            string ext = Path.GetExtension(assetName);
            if (!string.IsNullOrEmpty(ext)) resAssetPath = assetName.Replace(ext, "");
            T res = UR.Load<T>(resAssetPath);
            if (res == null)
                Debug.LogError(string.Format("Cant find resource: ", assetName));
            return res;
        }

        /// <summary>
        /// 加载Resources目录资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static UO LoadAssets(string assetName, Type type)
        {
            string resAssetPath = assetName;
            string ext = Path.GetExtension(assetName);
            if (!string.IsNullOrEmpty(ext)) resAssetPath = assetName.Replace(ext, "");
            UO res = UR.Load(resAssetPath, type);
            if (res == null)
                Debug.LogError(string.Format("Cant find resource: ", assetName));
            return res;
        }

        /// <summary>
        /// 加载Resources目录文本资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static TextAsset LoadTextAssets(string assetName)
        {
            return LoadAssets<TextAsset>(assetName);
        }

        /// <summary>
        /// 加载Resources目录Sprite资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static Sprite LoadSpriteAssets(string assetName)
        {
            return LoadAssets<Sprite>(assetName);
        }

        /// <summary>
        /// 加载Resources目录GameObject资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static GameObject LoadGameObjectAssets(string assetName)
        {
            return LoadAssets<GameObject>(assetName);
        }

        /// <summary>
        /// 加载Prefab资源, Bundle类型资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static GameObject LoadPrefab(string assetName)
        {
            if (AppConst.AssetBundleMode)
                return AssetBundleManager.Instance.LoadPrefab(assetName);
#if UNITY_EDITOR
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("Assets/Res/{0}.prefab", assetName));
            return go;
#endif
            return null;
        }

        /// <summary>
        /// 加载二进制数据, Bundle类型资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static byte[] LoadBytes(string assetName)
        {
            if (AppConst.AssetBundleMode)
                return AssetBundleManager.Instance.LoadBytes(assetName);
#if UNITY_EDITOR
            string extension = Path.GetExtension(assetName);
            if (!string.IsNullOrEmpty(extension))
                assetName = assetName.Replace(extension, "");
            TextAsset textAss = AssetDatabase.LoadAssetAtPath<TextAsset>(string.Format("Assets/Res/{0}.bytes", assetName));
            return textAss.bytes;
#endif
            return null;
        }
        #endregion


        #region -------------------加载音频数据-------------------

        public static IPromise<AudioClip> LoadAudioClipAsync(string name, string extension)
        {
            if (AppConst.AssetBundleMode)
            {
                return AssetBundleManager.Instance.LoadAudioClipAsync(name, extension);
            }

            AudioClip clip = null;
#if UNITY_EDITOR
            string path = string.Format("Assets/Res/{0}.{1}", name, extension);
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
#endif
            return Promise<AudioClip>.Resolved(clip);
        }
        #endregion
    }
}