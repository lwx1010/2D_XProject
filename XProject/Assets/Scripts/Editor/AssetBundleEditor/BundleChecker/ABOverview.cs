using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BundleChecker
{
    /// <summary>
    /// AssetBundle资源总览
    /// </summary>
    public class ABOverview
    {
        private Vector2 scrollPos = Vector2.zero;

        private int indexRow;
        private int loadIndex;
        private float loadCount;

        private enum EView
        {
            ALLAsset , RedundancyAssets , MissingAsset
        }

        private EView curView = EView.ALLAsset;
        //冗余的bundle资源
        private Dictionary<string , EditorBundleBean> redundancyDic = new Dictionary<string, EditorBundleBean>();
        private List<EditorBundleBean> redundancyList;
        //内置资源引用
//        private 
        private static Dictionary<string , string> allBundleFiles = new Dictionary<string, string>();  //目录下的所有bundle资源包

        private string curFolder = "";

        private bool isInit = false;
        private int sortToggle = 1;

        public void Initlization()
        {
            if (isInit) return;

            isInit = true;
            curFolder = CurFolderRoot;
            if (curFolder != Application.dataPath && Directory.Exists(curFolder))
                FindAllBundles();
        }

        public void OnGUI()
        {
            
            GUILayoutHelper.DrawHeader("检测AssetBundle");
            GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Asset Bundles", "DropDown" ,GUILayout.Width(120)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select", CurFolderRoot , "");
                    CurFolderRoot = path;
                    curFolder = path;                
                }
            
                if (curFolder.StartsWith(Application.dataPath))
                {
                    curFolder = curFolder.Replace(Application.dataPath, "Assets");
                }
                GUILayout.TextField(curFolder);

                GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Go Check", GUILayout.Height(25)))
            {
                bundleArr = null;
                this.FindAllBundles();
            }
            GUI.backgroundColor = Color.white;

            //Overview
            GUILayoutHelper.DrawSeparator();
            drawOverview();

            switch (curView)
            {
                    case EView.ALLAsset:
                    drawAllAssetBundle();
                    break;
                    case EView.RedundancyAssets:
                    drawAllRedundancyAsset();
                    break;
                    case EView.MissingAsset:
                    drawMissingAsset();
                    break;
            }
        }

        /// <summary>
        /// 总览
        /// </summary>
        private void drawOverview()
        {
            ABMainChecker mainCheckr = ABMainChecker.MainChecker;
            if (mainCheckr == null || mainCheckr.BundleList == null) return;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Format("总资源数：{0}" , mainCheckr.BundleList.Count) , GUILayout.Height(50)))
            {
                scrollPos = Vector2.zero;
                curView = EView.ALLAsset;
            }

            if (GUILayout.Button(string.Format("冗余资源数：{0}", redundancyDic.Count), GUILayout.Height(50)))
            {
                scrollPos = Vector2.zero;
                redundancyList = redundancyDic.Values.ToList();
                redundancyList.Sort((x,y)=>(new CaseInsensitiveComparer()).Compare(x.BundleName , y.BundleName));

                curView = EView.RedundancyAssets;
            }

            if (GUILayout.Button(string.Format("丢失AB数：{0}", mainCheckr.MissingRes.Count), GUILayout.Height(50)))
            {
                scrollPos = Vector2.zero;
                selectAsset = "";
                mainCheckr.MissingRes.Sort((x, y) => (new CaseInsensitiveComparer()).Compare(x.Name, y.Name));
                curView = EView.MissingAsset;
            }
            
            GUILayout.EndHorizontal();
        }

        #region --------------All AssetBundle------------------

        private string searchFilter = ""; //搜索文件
        private EditorBundleBean[] bundleArr = null;
        private int sortCount = 1;
        private int sortFileSize = 1;
        private void drawAllAssetBundle()
        {
            //all assets
            GUILayoutHelper.DrawHeader("All AssetBundle");
            //Search
            GUILayout.BeginHorizontal();
            {
                searchFilter = EditorGUILayout.TextField("", searchFilter, "SearchTextField", GUILayout.Width(Screen.width - 20f));

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    searchFilter = "";
                    GUIUtility.keyboardControl = 0;
                }
            }
            GUILayout.EndHorizontal();

            if (bundleArr == null)
            {
                var bundleDic = ABMainChecker.MainChecker.BundleList;
                bundleArr = new EditorBundleBean[bundleDic.Count];
                bundleDic.Values.CopyTo(bundleArr , 0);

                Array.Sort(bundleArr);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(false, "AssetBundle 名称", "ButtonLeft", GUILayout.Width(200)))
            {
                Array.Sort(bundleArr);
            }
            if (GUILayout.Toggle(false, "依赖数量", "ButtonMid", GUILayout.Width(80)))
            {
                sortCount *= -1;
                Array.Sort(bundleArr , (x, y) => x.GetAllDependencies().Count > y.GetAllDependencies().Count ? sortCount : sortCount * -1);
            }
            if (GUILayout.Toggle(false, "文件大小", "ButtonMid", GUILayout.Width(80)))
            {
                sortFileSize *= -1;
                Array.Sort(bundleArr, (x, y) => x.FileSize > y.FileSize ? sortFileSize : sortFileSize * -1);
            }
            GUILayout.Toggle(false, "具体依赖文件", "ButtonMid");
            GUILayout.Toggle(false, "详细", "ButtonRight", GUILayout.Width(80));
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            indexRow = 0;

            foreach (EditorBundleBean bundle in bundleArr)
            {
                if (string.IsNullOrEmpty(searchFilter) || bundle.BundleName.Contains(searchFilter))
                {
                    drawRowBundle(bundle);
                }
            }
            GUILayout.EndScrollView();
        }

        private void drawRowBundle(EditorBundleBean bundle)
        {
            indexRow ++;
            GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.backgroundColor = Color.white;
            //名称
            GUILayout.Label(bundle.BundleName , GUILayout.Width(200));
            //依赖数量
            List<EditorBundleBean> dependencies = bundle.GetAllDependencies();
            GUILayout.Label(dependencies.Count.ToString() , GUILayout.Width(80));
            //文件大小
            GUILayout.Label(bundle.FileSize >= 1024 ? string.Format("{0:F}MB" , bundle.FileSize / 1024F) :
                                                      string.Format("{0}KB" , bundle.FileSize),GUILayout.Width(80));
            //具体的ab名称
            GUILayout.BeginVertical();
            int column = Mathf.Max( 1, (int)((ABMainChecker.MainChecker.Width - 380)/150));
            int endIndex = 0;
            for (int i = 0 , maxCount = dependencies.Count ; i < maxCount; i++)
            {
                EditorBundleBean depBundle = dependencies[i];
                if (i % column == 0)
                {
                    endIndex = i + column - 1;
                    GUILayout.BeginHorizontal();
                }
                if (GUILayout.Button(depBundle.BundleName, GUILayout.Width(150)))
                {
                    ABMainChecker.MainChecker.DetailBundleView.SetCurrentBundle(depBundle);
                }
                if (i == endIndex)
                {
                    endIndex = 0;
                    GUILayout.EndHorizontal();
                }
            }
            if (endIndex != 0) GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            //查询
            GUILayout.Space(15);
            if (GUILayout.Button("GO" , GUILayout.Width(50) , GUILayout.Height(25)))
            {
                ABMainChecker.MainChecker.DetailBundleView.SetCurrentBundle(bundle);
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
        }
        #endregion

        #region --------------Redundancy Asset------------------
        /// <summary>
        /// 冗余的资源
        /// </summary>
        private void drawAllRedundancyAsset()
        {
            //all assets
            GUILayoutHelper.DrawHeader("All Redundancy AssetBundle");

            
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(false, "AssetBundle 名称", "ButtonLeft", GUILayout.MinWidth(300)))
            {
                sortToggle *= -1;
                redundancyList.Sort((x,y)=>(new CaseInsensitiveComparer()).Compare(x.BundleName , y.BundleName) * sortToggle);
            }
            if (GUILayout.Toggle(false, "Mesh", "ButtonMid", GUILayout.MinWidth(100)))
            {
                sortToggle *= -1;
                redundancyList.Sort((x,y)=> comparerAssetCount(x,y,EResoucresTypes.MeshType) );
            }
            if (GUILayout.Toggle(false, "Material", "ButtonMid", GUILayout.MinWidth(100)))
            {
                sortToggle *= -1;
                redundancyList.Sort((x, y) => comparerAssetCount(x, y, EResoucresTypes.MatrialType));
            }
            if(GUILayout.Toggle(false, "Texture", "ButtonMid", GUILayout.MinWidth(100)))
            {
                sortToggle *= -1;
                redundancyList.Sort((x, y) => comparerAssetCount(x, y, EResoucresTypes.TextureType));
            }
            if (GUILayout.Toggle(false, "Shader", "ButtonMid", GUILayout.MinWidth(100)))
            {
                sortToggle *= -1;
                redundancyList.Sort((x, y) => comparerAssetCount(x, y, EResoucresTypes.ShaderType));
            }
            GUILayout.Toggle(false, "详细", "ButtonRight", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            indexRow = 0;
            
            foreach (EditorBundleBean bundle in redundancyList)
            {
                if(string.IsNullOrEmpty(searchFilter) || bundle.BundleName.Contains(searchFilter))
                    drawRowRedundancyAsset(bundle);
            }
            GUILayout.EndScrollView();
        }

        private void drawRowRedundancyAsset(EditorBundleBean bundle)
        {
            indexRow++;
            GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(25f));
            GUI.backgroundColor = Color.white;
            //名称
            GUILayout.Label(bundle.BundleName, GUILayout.MinWidth(300));
            //Mesh
            int count = getAssetDedundancyCount(bundle, EResoucresTypes.MeshType);
            GUILayout.Space(40);
            GUILayout.Label(count.ToString() , GUILayout.MinWidth(100));
            //material
            count = getAssetDedundancyCount(bundle, EResoucresTypes.MatrialType);
            GUILayout.Space(40);
            GUILayout.Label(count.ToString(), GUILayout.MinWidth(100));
            //Texture
            count = getAssetDedundancyCount(bundle, EResoucresTypes.TextureType);
            GUILayout.Space(40);
            GUILayout.Label(count.ToString(), GUILayout.MinWidth(100));
            //Shader
            count = getAssetDedundancyCount(bundle, EResoucresTypes.ShaderType);
            GUILayout.Space(40);
            GUILayout.Label(count.ToString(), GUILayout.MinWidth(100));
            //查询
            GUILayout.Space(25);
            if (GUILayout.Button("GO", GUILayout.Width(50), GUILayout.Height(25)))
            {
                ABMainChecker.MainChecker.DetailBundleView.SetCurrentBundle(bundle);
            }
            GUILayout.Space(25);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 冗余资源被反复引用的次数
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="resType"></param>
        /// <returns></returns>
        private int getAssetDedundancyCount(EditorBundleBean bundle, string resType)
        {
            int count = 0;
            List<ResoucresBean> resList = bundle.GetAllAssets();
            foreach (ResoucresBean res in resList)
            {
                if (resType == res.ResourceType && res.IncludeBundles.Count > 1)
                    count ++;
            }
            return count;
        }

        private int comparerAssetCount(EditorBundleBean x, EditorBundleBean y, string resType)
        {
            return getAssetDedundancyCount(x, resType).CompareTo(getAssetDedundancyCount(y, resType)) * sortToggle;
        }
        #endregion


        #region ----------------------丢失的Bundle资源----------------------------------------

        private string selectAsset = "";
        private void drawMissingAsset()
        {
            //all assets
            GUILayoutHelper.DrawHeader("All Missing AssetBundle");

            List<ResoucresBean> missingResList = ABMainChecker.MainChecker.MissingRes;

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(false, "Asset 名称", "ButtonLeft", GUILayout.Width(200)))
            {
                missingResList.Sort((x, y) => (new CaseInsensitiveComparer()).Compare(x.Name, y.Name));
            }
            if (GUILayout.Toggle(false, "类型", "ButtonMid", GUILayout.Width(100)))
            {
                missingResList.Sort((x , y)=> (new CaseInsensitiveComparer()).Compare(x.ResourceType , y.ResourceType));
            }
            GUILayout.Toggle(false, "路径", "ButtonRight");
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            indexRow = 0;
            
            foreach (ResoucresBean res in missingResList)
            {
                indexRow++;
                GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                bool isCheck = false;
                //名称
                GUI.color = selectAsset == res.Name ? Color.green : Color.white;
                isCheck = GUILayout.Button(res.Name, EditorStyles.label, GUILayout.Width(200));
                //type
                GUILayout.Label(res.ResourceType , GUILayout.Width(100));
                //Path
                isCheck = (GUILayout.Button(res.AssetPath , EditorStyles.label) ? true : isCheck);
                GUI.color = Color.white;
                if (isCheck)
                {
                    selectAsset = res.Name;
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(res.AssetPath);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        #endregion
        /// <summary>
        /// 查找指定目录的Bundles
        /// </summary>
        public void FindAllBundles()
        {
            string rootPath = CurFolderRoot;
            if (!Directory.Exists(rootPath)) return;

            string[] fileArr = Directory.GetFiles(rootPath, "*" + CheckerConfig.AssetBundleSuffix, SearchOption.AllDirectories);
            //记录bundle路径，用于校验
            allBundleFiles.Clear();
            foreach (string abPath in fileArr)
                allBundleFiles[Path.GetFileName(abPath)] = abPath;

            ABMainChecker.MainChecker.Clear();

            Dictionary<string , EditorBundleBean> bundleDic = ABMainChecker.MainChecker.BundleList;

            EditorUtility.DisplayProgressBar("查找中", "正在查找文件...", 1.0f / fileArr.Length);
            loadCount = (float)fileArr.Length;
            loadIndex = 0;

            for (int i = 0 , maxCount = fileArr.Length; i < maxCount; i++)
            {
                string assetPath = GetRealBundlePath(fileArr[i]);
                if (!bundleDic.ContainsKey(assetPath))
                {
                    EditorBundleBean bundleBean = new EditorBundleBean(assetPath);
                    bundleDic[assetPath] = bundleBean;
                    loadAssetBundle(bundleBean);
                }
            }
            

            Dictionary<string, ResoucresBean> resDic = ABMainChecker.MainChecker.ResourceDic;
            ResoucresBean[] resArr = new ResoucresBean[resDic.Count];
            resDic.Values.CopyTo(resArr , 0);

            redundancyDic.Clear();
            for (int i = 0 , maxCount = resArr.Length; i < maxCount; i++)
            {
                EditorUtility.DisplayProgressBar("分析中", "检测依赖资源...", (float)i / maxCount);
                resArr[i].CheckDependencies();
            }

            //再检测冗余
            int _i = 0;
            float _maxCount = resDic.Count;
            foreach (ResoucresBean res in resDic.Values)
            {
                EditorUtility.DisplayProgressBar("分析中", "分析冗余...", _i / _maxCount);
                _i++;
                if (res.IncludeBundles.Count <= 1) continue;

                foreach (EditorBundleBean bundle in res.IncludeBundles)
                {
                    redundancyDic[bundle.BundleName] = bundle;
                }
            }
            EditorUtility.ClearProgressBar();
        }


        private void loadAssetBundle(EditorBundleBean bundle)
        {
            loadIndex++;
            EditorUtility.DisplayProgressBar("分析中", "分析AssetBundle资源...", loadIndex / loadCount);

            string manifest = string.Concat(bundle.BundlePath, ".manifest");
            if (!File.Exists(manifest))
            {
                Debug.LogWarning("Cant find manifest ! " + manifest + "[" + bundle.BundleName + "]");
                return;
            }

            manifest = File.ReadAllText(manifest);
            string[] manifestInfoArr = manifest.Split('\n');

            //查找包含资源
            string[] bundInfo = getBundleInfo(manifestInfoArr, "Assets:");
            List<ResoucresBean> allAssets = bundle.GetAllAssets();

            ABMainChecker mainCheck = ABMainChecker.MainChecker;
            foreach (string assetPath in bundInfo)
            {
                //string assetName = Path.GetFileName(assetPath);
                ResoucresBean rb = null;
                if (!mainCheck.ResourceDic.TryGetValue(assetPath, out rb))
                {
                    rb = new ResoucresBean(assetPath);
                    mainCheck.ResourceDic[assetPath] = rb;
                }

                if(!rb.IncludeBundles.Contains(bundle))
                    rb.IncludeBundles.Add(bundle);

                allAssets.Add(rb);
            }

            //查找依赖
            bundInfo = getBundleInfo(manifestInfoArr, "Dependencies:");
            Dictionary<string, EditorBundleBean> bundles = ABMainChecker.MainChecker.BundleList;
            List<EditorBundleBean> depBundles = bundle.GetAllDependencies();

            foreach (string curAssetPath in bundInfo)
            {
                EditorBundleBean depBundle = null;
                string assetPath = GetRealBundlePath(curAssetPath);
                if (!bundles.TryGetValue(assetPath, out depBundle))
                {
                    depBundle = new EditorBundleBean(assetPath);
                    bundles[assetPath] = depBundle;
                    loadAssetBundle(depBundle);
                }

                //依赖记录
                if (!depBundles.Contains(depBundle))
                    depBundles.Add(depBundle);

                //被依赖
                List<EditorBundleBean> beDepBundles = depBundle.GetBedependencies();
                if (!beDepBundles.Contains(bundle))
                    beDepBundles.Add(bundle);
            }
        }


        private string[] getBundleInfo(string[] manifestArr, string key)
        {
            bool isStart = false;
            List<string> infos = new List<string>();
            foreach (string str in manifestArr)
            {

                if (isStart)
                {
                    if (!str.StartsWith("-")) break;
                    infos.Add(str.Substring(2).Trim());
                }

                if (str.StartsWith(key))
                {
                    isStart = true;
                }
            }
            return infos.ToArray();
        }

        public static string overwiewDefaultFolder =  "Assets/OverView_defultFolder";
        public string CurFolderRoot
        {
            get
            {
                return EditorPrefs.GetString(overwiewDefaultFolder, Application.dataPath);
            }
            set
            {
                EditorPrefs.SetString(overwiewDefaultFolder, value);
            }
        }


        public static string GetRelativeAssetPath(string path)
        {
            string assetPath = path.Replace("\\", "/");
            if (!assetPath.StartsWith("Assets/"))
            {
                int index = assetPath.IndexOf("Assets/");
                if(index > 0)
                    assetPath = assetPath.Substring(index).Replace("\\", "/");
            }
            return assetPath;
        }

        /// <summary>
        /// 获得真实的Bundle路径
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public static string GetRealBundlePath(string bundlePath)
        {
            string bundName = Path.GetFileName(bundlePath);
            if (allBundleFiles.ContainsKey(bundName)) return allBundleFiles[bundName];
            return bundlePath;
        }
    }



}