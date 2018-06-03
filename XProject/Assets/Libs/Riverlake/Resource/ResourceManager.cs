using UnityEngine;
using System.IO;
using System;
using LuaInterface;
using RSG;
using AL.PoolCache;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UO = UnityEngine.Object;
using UR = UnityEngine.Resources;

namespace AL.Resources
{
    public static class ResourceManager
    {
        #region 异步加载Bundle资源

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
        public static ALoadOperation LoadBundleAsync(string assetName,string extension = ".prefab")
        {
            if (AppConst.AssetBundleMode)
            {
                return new LoadBundleAsync(assetName);
            }
            string assetPath = string.Concat("Assets/Res/", assetName, extension);

            return new LoadAssetAsync(assetPath);
        }

        /// <summary>
        /// 从指定缓存池（poolA）中加载资源,如果缓存池中不存在资源，就从磁盘加载
        /// <para>[[重要]]:加载完成后，需要使用指定的缓存池（poolA）进行实例（spawn）</para>
        /// </summary>
        /// <param name="cachePool">CachePoolAsync缓存池</param>
        /// <param name="assetName">无后缀的资源路径</param>
        /// <param name="extension">文件后缀</param>
        /// <returns></returns>
        public static ALoadOperation LoadCacheBundleAsync(CachePoolAsync cachePool ,string assetName, string extension = ".prefab")
        {
            string prefabName = assetName;
            string[] tempArr = assetName.Split('/');
            if(tempArr.Length > 0)
                prefabName = tempArr[tempArr.Length - 1];
            GameObject prefab = cachePool.GetCachePrefab(prefabName);
            if (prefab != null)
            {
                string assetPath = string.Concat("Assets/Res/", assetName, extension);
                return new LoadAssetAsync(prefab , assetPath);
            }
            return LoadBundleAsync(assetName , extension);
        }

        #endregion

        #region -------------------同步加载资源-------------------

        /// <summary>
        /// 加载Resources目录资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private static T loadResources<T>(string assetName) where T : UnityEngine.Object
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
        public static UO LoadResource(string assetName, Type type)
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
            return loadResources<TextAsset>(assetName);
        }

        /// <summary>
        /// 加载Resources目录Sprite资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static Sprite LoadSpriteAssets(string assetName)
        {
            return loadResources<Sprite>(assetName);
        }

        /// <summary>
        /// 加载Resources目录GameObject资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static GameObject LoadGameObjectAssets(string assetName)
        {
            return loadResources<GameObject>(assetName);
        }



        #region Bundle加载
        /// <summary>
        /// 加载Prefab资源, Bundle类型资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static GameObject LoadPrefabBundle(string assetName)
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
        public static byte[] LoadBytesBundle(string assetName)
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
        /// <summary>
        /// 加载Scriptable类型的.asset文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        [NoToLua]
        public static T LoadAssetBundle<T>(string assetName) where T : ScriptableObject
        {
            if (AppConst.AssetBundleMode)
                return AssetBundleManager.Instance.LoadAssets<T>(assetName);
#if UNITY_EDITOR
            string extension = Path.GetExtension(assetName);
            if (!string.IsNullOrEmpty(extension))
                assetName = assetName.Replace(extension, "");
            T obj = AssetDatabase.LoadAssetAtPath<T>(string.Format("Assets/Res/{0}.asset", assetName));
            return obj;
#endif
            return null;
        }

        /// <summary>
        /// 从Atlas集合中加载Sprite对象
        /// </summary>
        /// <param name="spriteName">精灵图片名称</param>
        /// <param name="atlasName">图集名称:Atlas/common/common-0</param>
        /// <returns></returns>
        public static Sprite LoadSpriteFromAtlasBundle(string spriteName, string atlasName)
        {
            if (AppConst.AssetBundleMode)
                return AssetBundleManager.Instance.LoadSprite(spriteName, atlasName);
#if UNITY_EDITOR
            string extension = Path.GetExtension(spriteName);
            if (!string.IsNullOrEmpty(extension))
                spriteName = spriteName.Replace(extension, "");
            var assets = AssetDatabase.LoadAllAssetsAtPath(string.Format("Assets/Res/{0}.png", atlasName));
            foreach (var asset in assets)
            {
                if (asset is Sprite)
                {
                    if (asset.name.Equals(spriteName))
                        return asset as Sprite;
                }
            }
#endif
            return null;
        }

        #endregion

        #endregion


        #region -------------------加载音频数据-------------------

        public static IPromise<AudioClip> LoadAudioClipBundleAsync(string name, string extension)
        {
            if (AppConst.AssetBundleMode)
            {
                return AssetBundleManager.Instance.LoadAudioClipBundleAsync(name, extension);
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