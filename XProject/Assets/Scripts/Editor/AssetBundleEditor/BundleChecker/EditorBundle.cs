using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BundleChecker.ResoucreAttribute;
using UnityEditor;
using Object = UnityEngine.Object;

namespace BundleChecker
{
    

    public class EditorBundleBean : IComparable<EditorBundleBean>
    {
        //包含资源
        private List<ResoucresBean> containeRes = new List<ResoucresBean>(); 
        //外部依赖
        private List<EditorBundleBean> dependencies = new List<EditorBundleBean>();
        /// <summary>
        /// 被其它Bundle依赖
        /// </summary>
        private List<EditorBundleBean> beDependcies = new List<EditorBundleBean>();

        #region ---------------Public Attribute-------------------------
        public string BundleName { get; private set; }
        public string BundlePath { get; private set; }
        public long FileSize { get; private set; }
        #endregion

        public EditorBundleBean(string path)
        {
            this.BundlePath = path;
            this.BundleName = Path.GetFileName(path);
            this.FileSize = new FileInfo(path).Length / 1024;
        }
        
        /// <summary>
        /// 依赖的外部资源
        /// </summary>
        /// <returns></returns>
        public List<EditorBundleBean> GetAllDependencies()
        {
            return dependencies;
        }
        /// <summary>
        /// 其它Bundle的依赖（被依赖）
        /// </summary>
        /// <returns></returns>
        public List<EditorBundleBean> GetBedependencies()
        {
            return beDependcies;
        } 

        public List<ResoucresBean> GetAllAssets()
        {
            return containeRes;
        }
        /// <summary>
        /// 获得具体类型的资源列表
        /// </summary>
        /// <param name="resType"></param>
        /// <returns></returns>
        public List<ResoucresBean> GetAllAssets(string resType)
        {
            List<ResoucresBean> rbs = new List<ResoucresBean>();
            foreach (ResoucresBean rb in containeRes)
            {
                if (rb.ResourceType != resType) continue;
                rbs.Add(rb);
            }
            return rbs;
        } 

        public int CompareTo(EditorBundleBean other)
        {
            CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
            return cic.Compare(BundleName, other.BundleName);
        }
    }


    public sealed class EResoucresTypes
    {
        public const string ShaderType = "Shader";
        public const string MatrialType = "Matrial";
        public const string AnimationClipType = "AnimationClip";
        public const string MeshType = "Mesh";
        public const string TextureType = "Texture";
        public const string AudioType = "Audio";
        public const string Prefab = "Prefab";
        public const string UnKnow = "UnKnow";

        private const string shaderSfx = ".shader";
        private const string materialSfx = ".mat";
        private const string texturePngSfx = ".png";
        private const string textureJpgSfx = ".jpg";
        private const string textureTgaSfx = ".tga";
        private const string texturePsdSfx = ".psd";
        private const string animSfx = ".anim";
        private const string animCtrlSfx = ".controller";
        private const string meshSfx = ".fbx";
        private const string prefabSfx = ".prefab";

        public static string GetResourceType(string suffix)
        {
            suffix = suffix.ToLower();
            switch (suffix)
            {
                case shaderSfx: return ShaderType;
                case materialSfx:   return MatrialType;
                case textureJpgSfx:
                case texturePngSfx:
                case texturePsdSfx:
                case textureTgaSfx:
                    return TextureType;
                case animSfx:
                case animCtrlSfx:
                    return AnimationClipType;
                case prefabSfx: return Prefab;
                case meshSfx: return MeshType;
            }
            
            return UnKnow;
        }
    }

    public class ResoucresBean
    {

        #region -------------------Public Attribute-----------------------------
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; private set; }
        /// <summary>
        /// 资源类型
        /// </summary>
        public string ResourceType { get; private set; }
        /// <summary>
        /// 是否丢失
        /// </summary>
        public bool IsMissing { get; set; }
        /// <summary>
        /// 是否是内置资源
        /// </summary>
        public bool IsBuiltinExa { get; set; }
        /// <summary>
        /// 资源的原始数据
        /// </summary>
        public ABaseResource RawRes { get; private set; }
        #endregion
        /// <summary>
        /// 所属Bundle
        /// </summary>
        public List<EditorBundleBean>  IncludeBundles = new List<EditorBundleBean>();
        /// <summary>
        /// 依赖的资源
        /// </summary>
        public Dictionary<string , ResoucresBean> Dependencies = new Dictionary<string, ResoucresBean>();

        public Object[] mainObjs { get; private set; }
        public ResoucresBean(string path)
        {
            this.AssetPath = path;
            this.Name = Path.GetFileName(path);
            this.ResourceType = EResoucresTypes.GetResourceType(Path.GetExtension(path));
        }
        
        public void LoadRawAsset()
        {
            if (this.RawRes != null || !File.Exists(this.AssetPath))    return;

                switch (this.ResourceType)
            {
                case EResoucresTypes.TextureType:
                    RawRes = new TextureAttribute(this);
                    break;
                case EResoucresTypes.MatrialType:
                    RawRes = new MaterialAttribute(this);
                    break;
                case EResoucresTypes.ShaderType:
                    RawRes = new ShaderAttribute(this);
                    break;
                case EResoucresTypes.MeshType:
                    RawRes = new MeshAttribute(this);
                    break;
            }
        }


        /// <summary>
        /// 检测依赖资源
        /// </summary>
        public void CheckDependencies()
        {           
            mainObjs = new [] {AssetDatabase.LoadAssetAtPath<Object>(this.AssetPath)};
            Object[] depArr = EditorUtility.CollectDependencies(mainObjs);
           
            Dictionary<string, ResoucresBean> resDic = ABMainChecker.MainChecker.ResourceDic;

            for (int i = 0 , max = depArr.Length; i < max; i++)
            {
                Object depAsset = depArr[i];
                ResoucresBean rb = null;
                string depAssetPath = AssetDatabase.GetAssetPath(depAsset);
                //Library asset
                if(BuiltinChecker.IsLibraryRes(depAssetPath))   continue;

                if (BuiltinChecker.IsExtraRes(depAssetPath))
                {
                    depAssetPath = BuiltinChecker.GetBuiltinAssetPath(depAsset);
                    if (!resDic.TryGetValue(depAssetPath, out rb))
                    {
                        rb = new ResoucresBean(depAssetPath);
                        rb.IsBuiltinExa = true;
                        resDic[depAssetPath] = rb;
                    }
                    //如果是内置资源，将会重复打包到所属bundle内
                    foreach (EditorBundleBean bundle in this.IncludeBundles)
                    {
                        if(rb.IncludeBundles.Contains(bundle))  continue;
                        rb.IncludeBundles.Add(bundle);
                    }
                    
                    //bundle包含
                    foreach (EditorBundleBean bundle in IncludeBundles)
                    {
                        List<ResoucresBean> allAsset = bundle.GetAllAssets();
                        if(allAsset.Contains(rb))   continue;
                        allAsset.Add(rb);
                    }
                }
                else
                {
                    string depAssetName = Path.GetFileName(depAssetPath);
                    if(depAssetName == Name)    continue;

                    //排除不打包的文件，比如.cs
                    string suffix = Path.GetExtension(depAssetPath);
                    if(CheckerConfig.ExcludeFiles.Contains(suffix) || CheckerConfig.IsExcludeFolder(depAssetPath)) continue;
                
                    if (!resDic.TryGetValue(depAssetPath, out rb))
                    {
                        rb = new ResoucresBean(depAssetPath);
                        rb.IsMissing = true;
                        resDic[depAssetPath] = rb;

                        ABMainChecker.MainChecker.MissingRes.Add(rb);
                    }                    
                }

                Dependencies[depAssetPath] = rb;
            }       
        }
    }
}