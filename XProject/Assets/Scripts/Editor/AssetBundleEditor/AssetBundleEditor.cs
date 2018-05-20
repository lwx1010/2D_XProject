using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using Riverlake.Crypto;
using System;
using System.Linq;
using ZstdNet;
using Config;

public class AssetBundleEditor : EditorWindow
{
    [MenuItem("AssetBundlePacker/打开ab编辑器")]
    static void OpenEditor()
    {
        var window = EditorWindow.GetWindow<AssetBundleEditor>();
        window.Show();
    }

    [MenuItem("AssetBundlePacker/关闭ab编辑器")]
    static void CloseEditor()
    {
        var window = EditorWindow.GetWindow<AssetBundleEditor>();
        window.Close();
    }

    public static void BuildIOSProject()
    {
        string path = string.Empty;
        bool nobundle = false;
        string key = string.Empty;
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.Contains("path:", StringComparison.OrdinalIgnoreCase))
            {
                var splitTmps = arg.Split(';');
                path = splitTmps[0].Split(':')[1];
                nobundle = splitTmps[1].Split(':')[1].Equals("1");
                key = splitTmps[2].Split(':')[1];
                break;
            }
        }
        XCodePostProcess.config_path = Application.dataPath + "/Res/Template/IOSConfig/" + path;
        AssetBundleEditor.OpenEditor();
        AssetBundleEditor.Instance.ReleaseIOS(path, nobundle, key);
        AssetBundleEditor.CloseEditor();
    }

    public static AssetBundleEditor Instance;

    public static GameVersion gameVersion { get; set; }
    public static int apkVersion { get; set; }

    //public Dictionary<string, ABData> datas { get; private set; }

    public Dictionary<string, string> abTypeMaps { get; private set; }

    private Vector2 deltaPosition;

    private string DEFAULT_CONFIG_NAME = "Assets/ab_config.xml";

    private string Apk_Save_Path;

    private bool debug = false;

    private bool development = false;

    private bool autoConnectProfile = false;

    private int lastSelectConfigIndex = 0;

    private SDKConfig sdkConfig;

    private List<SDKConfig> sdkConfigs = new List<SDKConfig>();

    private List<string> sdkConfigNames = new List<string>();

    private bool save = false;

    public enum OneKeyType
    {
        inside = 0,
        res_only = 1,
        allpack_only = 2,
        force_apk_change = 3,
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(Instance.position.width / 6);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button(ABLanguage.SET_RESOURCE_FOLDER, GUILayout.Width(330), GUILayout.Height(30)))
        {
            SetResourcesFolder();
        }
        GUILayout.Space(10);
        if (GUILayout.Button(ABLanguage.LOAD_CONFIG_INFO, GUILayout.Width(330), GUILayout.Height(30)))
        {
            LoadConfig();
        }
        GUILayout.Space(10);
        if (GUILayout.Button(ABLanguage.REFRESH_ALL, GUILayout.Width(330), GUILayout.Height(30)))
        {
            OneKeyRefresh();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        ABTools.DrawSeparator();
        EditorGUILayout.Space();
        deltaPosition = EditorGUILayout.BeginScrollView(deltaPosition);
        foreach (var val in ABData.datas.Values)
        {
            val.DisplayPanel();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (ABData.datas.Count > 0)
        {
            ABTools.DrawSeparator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.contentColor = Color.yellow;
            debug = EditorGUILayout.Toggle("Script Debugging", debug, GUILayout.Width(200));

            GUILayout.Space(10);
            development = EditorGUILayout.Toggle("Development Build", development, GUILayout.Width(200));

            GUILayout.Space(10);
            autoConnectProfile = EditorGUILayout.Toggle("Autoconnect Profile", autoConnectProfile, GUILayout.Width(200));
            GUI.contentColor = Color.white;

            //EditorGUILayout.LabelField("", GUILayout.Width(20));
            //EditorGUILayout.LabelField(string.Format("{0}: {1}", ABLanguage.RES_VERSION, gameVersion.ToString()), GUILayout.Width(150));
            //EditorGUILayout.LabelField(string.Format("{0}: {1}", ABLanguage.APK_VERSION, apkVersion.ToString()), GUILayout.Width(150));

            EditorGUILayout.LabelField(ABLanguage.PACK_CONFIG, GUILayout.Width(100));
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.red;
            lastSelectConfigIndex = ABTools.DrawPrefixList(lastSelectConfigIndex, sdkConfigNames.ToArray(), GUILayout.Width(200));
            if (sdkConfigs != null && sdkConfigs.Count > lastSelectConfigIndex)
                sdkConfig = sdkConfigs[lastSelectConfigIndex];
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(ABLanguage.EDIT_CONFIG, GUILayout.Width(100), GUILayout.Height(20)))
            {
                Debug.Log("编辑打开！！！！");
                return;
            }
            EditorGUILayout.LabelField("", GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Instance.position.width / 3 * 1.8f);
            GUILayout.Space(10);
            if (GUILayout.Button(ABLanguage.SAVE_CONFIG_INFO, GUILayout.Width(100), GUILayout.Height(25)))
            {
                SaveConfig();
                return;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(ABLanguage.CLEAR_ALL_ABNAME, GUILayout.Width(100), GUILayout.Height(25)))
            {
                ClearABName();
                return;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(ABLanguage.PACKRES_SUBPACK, GUILayout.Width(150), GUILayout.Height(25)))
            {
                PackRes();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Instance.position.width / 3 * 1.8f);
            GUILayout.Space(10);
            if (GUILayout.Button(ABLanguage.SUBPACK_RES_ONLY, GUILayout.Width(100), GUILayout.Height(25)))
            {
                CopyPackableFiles();
                return;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(ABLanguage.ALLPACK_RES_ONLY, GUILayout.Width(100), GUILayout.Height(25)))
            {
                CopyAllBundles();
                return;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(ABLanguage.SUBPACK_NOW, GUILayout.Width(150), GUILayout.Height(25)))
            {
                Pack();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
#if !UNITY_IOS
            GUILayout.Space(Instance.position.width / 6);
            GUILayout.Space(10);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(ABLanguage.RELEASE_PACKAGE, GUILayout.Width(330), GUILayout.Height(50)))
            {
                OneKeyPack(OneKeyType.allpack_only);
                return;
            }
            GUILayout.Space(20);
            if (GUILayout.Button(ABLanguage.RELEASE_RES_UPDATE, GUILayout.Width(330), GUILayout.Height(50)))
            {
                OneKeyPack(OneKeyType.res_only);
                return;
            }
            GUILayout.Space(20);
            if (GUILayout.Button(ABLanguage.RELEASE_APK_UPDATE, GUILayout.Width(330), GUILayout.Height(50)))
            {
                OneKeyPack(OneKeyType.force_apk_change);
                return;
            }
#else
            GUILayout.Space(Instance.position.width / 2.5f);
            GUILayout.Space(10);
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button(ABLanguage.RELEASE_IOS_XCODE_PORJ, GUILayout.Width(330), GUILayout.Height(50)))
            {
                XCodePostProcess.config_path = Application.dataPath + "/Res/Template/IOSConfig/HWSDK_Test.json";
                ReleaseIOS("HWTestin", false, "jdjh");
                return;
            }
#endif
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }
    }

    void OnEnable()
    {
        try
        {
            save = true;
            if (!Directory.Exists(ABPackHelper.BUILD_PATH)) Directory.CreateDirectory(ABPackHelper.BUILD_PATH);
            if (!Directory.Exists(ABPackHelper.BUILD_PATH + LuaConst.osDir)) Directory.CreateDirectory(ABPackHelper.BUILD_PATH + LuaConst.osDir);
            Instance = this;
            abTypeMaps = new Dictionary<string, string>();
            ABData.datas.Clear();
//#if !UNITY_IOS
//            SVNHelper.UpdateVersion();
//#endif
//            // 拷贝资源版本号
//#if UNITY_IOS
//            var destVersionPath = ABPackHelper.VERSION_PATH + "version_ios.txt";
//#else
//            var destVersionPath = ABPackHelper.VERSION_PATH + "version.txt";
//#endif
//            if (!File.Exists(destVersionPath)) destVersionPath = ABPackHelper.ASSET_PATH + LuaConst.osDir + "/version.txt";
//            var versionPath = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/version.txt";
//            File.Copy(destVersionPath, versionPath, true);
//            if (File.Exists(versionPath))
//                gameVersion = GameVersion.CreateVersion(File.ReadAllText(versionPath));
//            else
//                gameVersion = GameVersion.CreateVersion(Application.version);
//            // 读取游戏版本号
//            destVersionPath = ABPackHelper.VERSION_PATH + "apk_version.txt";
//            if (File.Exists(destVersionPath))
//            {
//                var ver = File.ReadAllText(destVersionPath);
//                apkVersion = Convert.ToInt32(ver);
//            }
//            else
//                apkVersion = 0;

            if (!File.Exists(DEFAULT_CONFIG_NAME))
                return;
            LoadConfig(DEFAULT_CONFIG_NAME);
            LoadSDKConfig();
            Refresh();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            save = false;
            AssetBundleEditor.CloseEditor();
        }
    }

    void OnDisable()
    {
        try
        {
            if (save) SaveConfig();
            Instance = null;
            ABData.datas.Clear();
            abTypeMaps.Clear();
            abTypeMaps = null;
            gameVersion = null;
            Apk_Save_Path = null;
        }
        catch (Exception e)
        {

        }
    }

    void SetResourcesFolder()
    {
        string path = EditorUtility.OpenFolderPanel(ABLanguage.SET_RESOURCE_FOLDER, "Assets", "");
        if (string.IsNullOrEmpty(path)) return;

        ABData.datas.Clear();
        string[] pathes = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < pathes.Length; ++i)
        {
            var abPath = "Assets" + Path.GetFullPath(pathes[i]).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
            var data = new ABData(null, abPath, "", 0, 0, 0, 0, false, false, 0, false, 0, 0, false);
            ABData.datas.Add(abPath, data);
        }
    }

    void LoadConfig()
    {
        string path = EditorUtility.OpenFilePanel(ABLanguage.LOAD_CONFIG_INFO, Application.dataPath, "xml");
        if (string.IsNullOrEmpty(path)) return;
        LoadConfig(path);
    }

    void LoadConfig(string path)
    {
        ABData.datas.Clear();
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        var root = doc.SelectSingleNode("ABConfig");
        if (root.Attributes["lastConfigIndex"] != null)
            lastSelectConfigIndex = Convert.ToInt32(root.Attributes["lastConfigIndex"].Value);
        var nodes = root.SelectNodes("data");
        ABData.datas.Clear();
        for (int i = 0; i < nodes.Count; ++i)
        {
            var abPath = nodes[i].Attributes["path"].Value;
            var data = ABData.DeserializeFromXml(nodes[i], null);
            ABData.datas.Add(abPath, data);
        }
    }

    void LoadSDKConfig()
    {
        sdkConfigs.Clear();
        sdkConfigNames.Clear();
        string root = Application.dataPath + "/Res/Template/SDKConfig/";
        string[] files = Directory.GetFiles(root, "*.json", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; ++i)
        {
            var config = SDKConfig.LoadSDKConfig(File.ReadAllText(files[i]));
            sdkConfigs.Add(config);
            sdkConfigNames.Add(config.show_name);
        }
    }

    void SaveConfig()
    {
        XmlDocument doc = new XmlDocument();
        if (File.Exists(DEFAULT_CONFIG_NAME)) File.Delete(DEFAULT_CONFIG_NAME);
        XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        doc.AppendChild(dec);
        var attr = doc.CreateElement("ABConfig");
        doc.AppendChild(attr);
        attr.SetAttribute("lastConfigIndex", lastSelectConfigIndex.ToString());
        foreach (var val in ABData.datas.Values)
        {
            val.SerializeToXml(attr);
        }
        doc.Save(DEFAULT_CONFIG_NAME);
        AssetDatabase.Refresh();
        Debug.Log("<color=#2fd95b>Save Success !</color>");
    }

    void ClearABName()
    {
        ABPackHelper.ClearAllAbName();
    }

    void OneKeyRefresh()
    {
        foreach (var val in ABData.datas.Values)
        {
            if (val.removeFlag) continue;

            val.OneKeyRefresh();
        }
    }

    void Refresh()
    {
        Dictionary<int, List<ABData>> tempDatas = new Dictionary<int, List<ABData>>();
        foreach (var val in ABData.datas.Values)
        {
            val.SortData(tempDatas);
        }
        List<int> keys = new List<int>(tempDatas.Keys);
        keys.Sort();
        EditorUtility.DisplayProgressBar("Hold on", "", 0);
        int index = 0;
        foreach (var key in keys)
        {
            for (int i = 0; i < tempDatas[key].Count; ++i)
            {
                var name = tempDatas[key][i].abName.ToLower();
                if (!abTypeMaps.ContainsKey(name))
                {
                    var value = string.Format("{0}.{1}", tempDatas[key][i].assetType, tempDatas[key][i].downloadOrder);
                    abTypeMaps.Add(name, value);
                }
            }
            index++;
            EditorUtility.DisplayProgressBar("Hold on", "", (float)index / (float)keys.Count);
        }
        EditorUtility.ClearProgressBar();
    }

    bool SetPlayerSavePath()
    {
        string ext = "";
        string locationPath = "";
        string appName = DateTime.Now.ToString("yyyyMMdd");
        switch (ABPackHelper.GetBuildTarget())
        {
            case BuildTarget.Android:
                ext = "apk";
                locationPath = Path.Combine(Application.dataPath, "../android_apk/");
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                ext = "exe";
                locationPath = Path.Combine(Application.dataPath, "../win_exe/");
                if (!Directory.Exists(locationPath)) Directory.CreateDirectory(locationPath);
                break;
            case BuildTarget.iOS:
                locationPath = Application.dataPath.Replace("jyjh/Assets", "") + "xcode_proj";
                break;
        }
        if (!Directory.Exists(locationPath)) Directory.CreateDirectory(locationPath);
        Apk_Save_Path = EditorUtility.SaveFilePanel(ABLanguage.PACK_SAVE_FOLDER, locationPath, appName, ext);
        if (string.IsNullOrEmpty(Apk_Save_Path)) return false;
        return true;
    }

    void Pack()
    {
        if (SetPlayerSavePath())
        {
            SVNHelper.UpdateAssets();
            ClearABName();
            SetAbName();
            // 开始打包
            Build();
        }
    }

    void OneKeyPack(OneKeyType type)
    {
        try
        {
            if (SetPlayerSavePath())
            {
                // 0.svn更新打包资源
                SVNHelper.UpdateAssets();
                SVNHelper.UpdateTempAssets();
                // 1.svn更新
                SVNHelper.UpdateAll();
                // 2. 清除AB名
                ClearABName();
                // 3. 设置AB名
                SetAbName();
                // 3.打包ab
                BuildAssetBundle();
                // 4.建立ab映射文件
                BuildBundleNameMapFile();
                // 5.记录压缩前文件大小和md5码,以及资源类型（是否包含于整包,是否是补丁资源）
                RecordFileRealSize();
                // 6.打包lua以及压缩资源
                BuildLuaAndCopyResources();
                // 7.svn提交打包资源
                SVNHelper.CommitPackAssets();
                // 8.复制分包资源到StreamingAssets目录下
                if (type == OneKeyType.allpack_only)
                {
                    CopyPackableFiles();
                    // 9.打包测试包
                    BuildPlayer(false, false);
                    // 10.打包整包
                    CopyAllBundles();
                    BuildPlayer(true, type == OneKeyType.force_apk_change);
                    // 11.重置config.txt
                    ResetConfig();
                }
                else if (type == OneKeyType.inside || type == OneKeyType.force_apk_change)
                {
                    CopyAllBundles();
                    // 10.打包内网整包
                    BuildPlayer(true, type == OneKeyType.force_apk_change);
                    // 11.重置config.txt
                    ResetConfig();
                }
                // 12.上传CDN
                UploadCheck();
                Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void ReleaseIOS(string save_path, bool nobundle, string key)
    {
        try
        {
            var locationPath = Application.dataPath.Replace("jyjh/Assets", "") + "xcode_proj";
            Apk_Save_Path = locationPath + "/" + (string.IsNullOrEmpty(save_path) ? DateTime.Now.ToString("yyyyMMdd") : save_path);
            IOSGenerateHelper.GenerateNewCrypto(key);
            SetKey();
            if (!nobundle)
            {
                // 2. 清除AB名
                ClearABName();
                // 3. 设置AB名
                SetAbName();
                // 3.打包ab
                BuildAssetBundle();
                // 4.建立ab映射文件
                BuildBundleNameMapFile();
                // 5.记录压缩前文件大小和md5码,以及资源类型（是否包含于整包,是否是补丁资源）
                RecordFileRealSize();
                // 6.打包lua以及压缩资源
                BuildLuaAndCopyResources();
                // 8.复制整包资源到StreamingAssets目录下
                CopyAllBundles();
            }
            // 3.拷贝config
            BuildPlayer(true, false);
            // 4.重置config.txt
            ResetConfig();

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    void SetKey()
    {
        string content = File.ReadAllText("Assets/Resources/crypto.txt");
        string[] keys = content.Split('|');
        Crypto.Proxy.SetKey(MD5.ComputeHash(Riverlake.Encoding.GetBytes(keys[0])), Riverlake.Encoding.GetBytes(keys[1]));
    }

    void PackRes()
    {
        try
        {
            SVNHelper.UpdateAssets();
            // 1.清理ab名
            ClearABName();
            // 2.设置ab名
            SetAbName();
            // 3.打包ab
            BuildAssetBundle();
            // 4.建立ab映射文件
            BuildBundleNameMapFile();
            // 5.记录压缩前文件大小和md5码,以及资源类型（是否包含于整包,是否是补丁资源）
            RecordFileRealSize();
            // 6.压缩资源
            BuildLuaAndCopyResources();
            // 7.分包处理
            CopyPackableFiles();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    void SetAbName()
    {
        string savePath = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/tempsizefile.txt";
        if (File.Exists(savePath)) File.Delete(savePath);
        AssetDatabase.Refresh();

        // 设置ab名
        Dictionary<int, List<ABData>> tempDatas = new Dictionary<int, List<ABData>>();
        foreach (var val in ABData.datas.Values)
        {
            val.SortData(tempDatas);
        }
        List<int> keys = new List<int>(tempDatas.Keys);
        keys.Sort();
        EditorUtility.DisplayProgressBar("Set AssetBundleName...", "", 0);
        int index = 0;
        int total = 0;
        foreach (var key in keys)
        {
            total += tempDatas[key].Count;
        }
        foreach (var key in keys)
        {
            for (int i = 0; i < tempDatas[key].Count; ++i)
            {
                var name = tempDatas[key][i].abName.ToLower();
                if (!abTypeMaps.ContainsKey(name))
                {
                    var value = string.Format("{0}.{1}", tempDatas[key][i].assetType, tempDatas[key][i].downloadOrder);
                    abTypeMaps.Add(name, value);
                }
                EditorUtility.DisplayProgressBar("Set AssetBundleName...", tempDatas[key][i].abPath, (float)index++ / (float)total);
                tempDatas[key][i].SetAbName();
            }
        }
        EditorUtility.DisplayProgressBar("Set AssetBundleName...", "完成", 1);
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    void Build()
    {
        try
        {
            // 1.打包ab
            BuildAssetBundle();
            // 2.建立ab映射文件
            BuildBundleNameMapFile();
            // 3.记录压缩前文件大小和md5码,以及资源类型（是否包含于整包,是否是补丁资源）
            RecordFileRealSize();
            // 4.压缩资源
            BuildLuaAndCopyResources();
            // 5.复制分包资源到StreamingAssets目录下
            CopyPackableFiles();
            // 6.打包apk
            BuildPlayer(false, false);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    void CopyPackableFiles()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
            Directory.Delete(Application.streamingAssetsPath, true);
        string targetPath = Application.streamingAssetsPath + "/" + LuaConst.osDir;
        Directory.CreateDirectory(targetPath);

        string bundlePath = ABPackHelper.BUILD_PATH + LuaConst.osDir;
        List<string> withExtensions = new List<string>() { ".ab", ".unity3d", ".txt", ".conf", ".pb" };
        string[] files = Directory.GetFiles(bundlePath, "*.*", SearchOption.AllDirectories)
            .Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        ABPackHelper.ShowProgress("", 0);
        for (int i = 0; i < files.Length; ++i)
        {
            if (Path.GetFileName(files[i]) == "tempsizefile.txt" || Path.GetFileName(files[i]) == "luamd5.txt") continue;
            ABPackHelper.ShowProgress("Copying files...", (float)i / (float)files.Length);
            var tempStr = files[i].Replace(bundlePath, "").Replace("\\", "/").TrimStart('/');
            var dirs = tempStr.Split('/');
            var tempDir = targetPath;
            for (int j = 0; j < dirs.Length - 1; ++j)
            {
                tempDir += "/" + dirs[j];
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);
            }
            var file = tempStr.ToLower();
            if (file.EndsWith(".ab"))
            {
                foreach (var key in abTypeMaps.Keys)
                {
                    var value = abTypeMaps[key.ToLower()].Split('.');
                    if ((file.Contains(key) && Convert.ToInt32(value[0]) == 0) 
                        || file.Contains(LuaConst.osDir.ToLower() + ".ab") 
                        || file.Contains("bundlemap.ab"))
                    {
                        File.Copy(files[i], targetPath + "/" + tempStr);
                        break;
                    }
                }
            }
            else
            {
                File.Copy(files[i], targetPath + "/" + tempStr);
            }
        }
        AssetDatabase.Refresh();
        ABPackHelper.ShowProgress("", 1);
        CompressWithZSTD(1024 * 1024 * 30);
    }

    void CopyAllBundles()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
            Directory.Delete(Application.streamingAssetsPath, true);
        string targetPath = Application.streamingAssetsPath + "/" + LuaConst.osDir;
        Directory.CreateDirectory(targetPath);

        string bundlePath = ABPackHelper.BUILD_PATH + LuaConst.osDir;
        List<string> withExtensions = new List<string>() { ".ab", ".unity3d", ".txt", ".conf", ".pb" };
        string[] files = Directory.GetFiles(bundlePath, "*.*", SearchOption.AllDirectories)
            .Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        ABPackHelper.ShowProgress("", 0);
        for (int i = 0; i < files.Length; ++i)
        {
            if (Path.GetFileName(files[i]) == "tempsizefile.txt" || Path.GetFileName(files[i]) == "luamd5.txt") continue;
            ABPackHelper.ShowProgress("Copying files...", (float)i / (float)files.Length);
            var tempStr = files[i].Replace(bundlePath, "").Replace("\\", "/").TrimStart('/');
            var dirs = tempStr.Split('/');
            var tempDir = targetPath;
            for (int j = 0; j < dirs.Length - 1; ++j)
            {
                tempDir += "/" + dirs[j];
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);
            }
            var file = ABPackHelper.GetRelativeAssetsPath(files[i]);
            bool needCopy = true;
            foreach (var key in abTypeMaps.Keys)
            {
                var value = abTypeMaps[key.ToLower()].Split('.');
                if (file.Contains(key) && Convert.ToInt32(value[0]) == 3)
                {
                    needCopy = false;
                    break;
                }
            }
            if (needCopy) File.Copy(files[i], targetPath + "/" + tempStr);
        }
        AssetDatabase.Refresh();
        ABPackHelper.ShowProgress("", 1);
        CompressWithZSTD(1024 * 1024 * 10);
    }

    void RemoveNotExsitBundles()
    {
        string[] files = Directory.GetFiles(ABPackHelper.BUILD_PATH, "*.ab", SearchOption.AllDirectories);
        ABPackHelper.ShowProgress("", 0);

        List<string> tempBundles = new List<string>();
        tempBundles.AddRange(abTypeMaps.Keys);
        
        for (int i = 0; i < files.Length; ++i)
        {
            var file = files[i].Replace("\\", "/");
            ABPackHelper.ShowProgress("Removing bundles...", (float)i / (float)files.Length);
            bool exist = false;
            for (int j = 0; j < tempBundles.Count; j++)
            {
                if (file.Contains(tempBundles[j]))
                {
                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                File.Delete(files[i]);
                File.Delete(files[i] + ".meta");
                File.Delete(files[i] + ".manifest");
                File.Delete(files[i] + ".manifest.meta");
            }
        }
        ABPackHelper.ShowProgress("", 1);
        AssetDatabase.Refresh();
    }

    void BuildAssetBundle()
    {
        ABPackHelper.CopyToAssetBundle();
        RemoveNotExsitBundles();
        // 更新资源版本号
        string bundlePath = ABPackHelper.BUILD_PATH + LuaConst.osDir;
        gameVersion.VersionIncrease();
        //PlayerSettings.bundleVersion = gameVersion.ToString();
        File.WriteAllText(bundlePath + "/version.txt", gameVersion.ToString());
        ABPackHelper.SaveVersion(gameVersion.ToString());
        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles(bundlePath, ABPackHelper.buildOptions, ABPackHelper.GetBuildTarget());
        var manifestFile = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/" + LuaConst.osDir;
        var saveABFile = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/" + LuaConst.osDir.ToLower();
        if (File.Exists(manifestFile))
        {
            var bytes = File.ReadAllBytes(manifestFile);
            File.Delete(manifestFile);
            File.WriteAllBytes(saveABFile + ".ab", bytes);
        }
        else
            Debug.LogError("<<BuildAssetBundle>> Cant find root manifest. ps:" + manifestFile);
        AssetDatabase.Refresh();
    }

    void BuildBundleNameMapFile()
    {
        string savePath = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/bundlemap.ab";
        StringBuilder sb = new StringBuilder();
        foreach (var val in ABData.datas.Values)
        {
            val.Format(sb);
        }
        if (File.Exists(savePath)) File.Delete(savePath);
        Debug.Log(sb.ToString());
        File.WriteAllBytes(savePath, Crypto.Encode(Riverlake.Encoding.GetBytes(sb.ToString())));
        AssetDatabase.Refresh();
    }

    void BuildPlayer(bool packAllRes, bool forceUpdate)
    {
        var option = BuildOptions.None;
        if (debug) option |= BuildOptions.AllowDebugging;
        if (development) option |= BuildOptions.Development;
        if (autoConnectProfile) option |= BuildOptions.ConnectWithProfiler;
        var temps = Apk_Save_Path.Replace("\\", "/").Split('/');
        if ((ABPackHelper.GetBuildTarget() == BuildTarget.Android 
            || ABPackHelper.GetBuildTarget() == BuildTarget.StandaloneWindows64
            || ABPackHelper.GetBuildTarget() == BuildTarget.StandaloneWindows) 
            && sdkConfig != null)
        {
            string lastChannel = string.Empty;
            for (int i = 0; i < sdkConfig.items.Count; ++i)
            {
                StringBuilder final_path = new StringBuilder();
                for (int j = 0; j < temps.Length - 1; ++j)
                {
                    final_path.Append(temps[j] + "/");
                }
                var item = sdkConfig.items[i];
                if (item.need_subpack == 0 && !packAllRes) continue;
                if (ABPackHelper.GetBuildTarget() == BuildTarget.StandaloneWindows64 || ABPackHelper.GetBuildTarget() == BuildTarget.StandaloneWindows)
                {
                    final_path.Append(DateTime.Now.ToString("yyyyMMdd") + "/");
                    if (!Directory.Exists(final_path.ToString())) Directory.CreateDirectory(final_path.ToString());
                    final_path.Append(item.game_name + "_v");
                }
                else
                {
                    if (packAllRes)
                    {
                        if (item.development == 1)
                        {
                            option |= BuildOptions.Development;
                            final_path.Append(item.game_name + DateTime.Now.ToString("yyyyMMdd") + "_allpack_dev_v");
                        }
                        else if (item.use_sdk == 1)
                            final_path.Append(item.game_name + DateTime.Now.ToString("yyyyMMdd") + "_allpack_sdk_v");
                        else
                            final_path.Append(item.game_name + DateTime.Now.ToString("yyyyMMdd") + "_allpack_test_v");
                    }
                    else
                    {
                        if (item.development == 1)
                        {
                            option |= BuildOptions.Development;
                            final_path.Append(item.game_name + DateTime.Now.ToString("yyyyMMdd") + "_subpack_dev_v");
                        }
                        else if (item.use_sdk == 1)
                            final_path.Append(item.game_name + DateTime.Now.ToString("yyyyMMdd") + "_subpack_sdk_v");
                        else
                            final_path.Append(item.game_name + DateTime.Now.ToString("yyyyMMdd") + "_subpack_test_v");
                    }
                }
                final_path.Append(gameVersion.ToString());
                if (ABPackHelper.GetBuildTarget() == BuildTarget.Android)
                {
                    final_path.Append(".apk");
                    if (File.Exists(final_path.ToString())) File.Delete(final_path.ToString());
                    // 写入并保存sdk启用配置
                    item.CopyConfig();
                    item.CopySDK();
                    item.SetPlayerSetting(sdkConfig.splash_image);
                    item.SaveSDKConfig();
                    item.SplitAssets(sdkConfig.split_assets);
                    IncreaseLEBIAN_VERCODE(forceUpdate, item.update_along);
                    if (item.update_along == 0 && forceUpdate)
                    {
                        if (Directory.Exists(Application.streamingAssetsPath)) Directory.Delete(Application.streamingAssetsPath, true);
                    }
                }
                else if (ABPackHelper.GetBuildTarget() == BuildTarget.StandaloneWindows64 || ABPackHelper.GetBuildTarget() == BuildTarget.StandaloneWindows)
                {
                    final_path.Append(".exe");
                    if (Directory.Exists(final_path.ToString())) Directory.Delete(final_path.ToString(), true);
                    item.CopyConfig();
                }
                AssetDatabase.Refresh();
                BuildPipeline.BuildPlayer(ABPackHelper.GetBuildScenes(), final_path.ToString(), ABPackHelper.GetBuildTarget(), option);
                AssetDatabase.Refresh();
                item.ClearSDK();

                SVNHelper.UpdateAll();
            }
        }
        else if (ABPackHelper.GetBuildTarget() == BuildTarget.iOS)
        {
            // 在上传目录新建一个ios_check.txt文件用于判断当前包是否出于提审状态
            string checkFile = ABPackHelper.ASSET_PATH + LuaConst.osDir + "/ios_check.txt";
            if (File.Exists(checkFile)) File.Delete(checkFile);
            File.WriteAllText(checkFile, "1");

            XCConfigItem configItem = XCConfigItem.ParseXCConfig(XCodePostProcess.config_path);
            if (configItem != null)
            {
                PlayerSettings.applicationIdentifier = configItem.bundleIdentifier;
                PlayerSettings.productName = configItem.product_name;
                configItem.CopyConfig();
            }
            IOSGenerateHelper.IOSConfusing();
            AssetDatabase.Refresh();
            BuildPipeline.BuildPlayer(ABPackHelper.GetBuildScenes(), Apk_Save_Path, ABPackHelper.GetBuildTarget(), option);
            AssetDatabase.Refresh();
        }

        Resources.UnloadUnusedAssets();
        GC.Collect();
        
        Debug.Log("<color=green>Build success!</color>");
    }

    void IncreaseLEBIAN_VERCODE(bool forceUpdate, int update_along)
    {
        var manifestPath = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";
        var lines = File.ReadAllLines(manifestPath);
        StringBuilder sb = new StringBuilder();
        foreach (var line in lines)
        {
            if (line.Contains("\"LEBIAN_VERCODE\""))
            {
                var ver_temps = line.Split('=');
                for (int j = 0; j < ver_temps.Length; ++j)
                {
                    if (j != ver_temps.Length - 1)
                    {
                        sb.Append(ver_temps[j]);
                        sb.Append('=');
                    }
                    else
                    {
                        // 读取apk版本号
                        string apkVersionPath = ABPackHelper.VERSION_PATH + "apk_version.txt";
                        if (forceUpdate)
                        {
                            apkVersion++;
                            if (File.Exists(apkVersionPath)) File.Delete(apkVersionPath);
                            File.WriteAllText(apkVersionPath, apkVersion.ToString());
                            SVNHelper.CommitVersion();
                        }
                        sb.Append('"').Append(apkVersion).Append("\"/>\n");
                    }
                }
            }
            else if (line.Contains("\"ClientChId\"") && update_along == 0)
            {
                var ver_temps = line.Split('=');
                for (int j = 0; j < ver_temps.Length; ++j)
                {
                    if (j != ver_temps.Length - 1)
                    {
                        sb.Append(ver_temps[j]);
                        sb.Append('=');
                    }
                    else
                    {
                        sb.Append("\"PATCH_V3\"/>\n");
                    }
                }
            }
            else
            {
                sb.AppendLine(line);
            }
        }
        File.Delete(manifestPath);
        sb.Replace("$bundleidentifier$", PlayerSettings.applicationIdentifier);
        File.WriteAllText(manifestPath, sb.ToString());
    }

    void ResetConfig()
    {
        string resources_path = "Assets/Resources/";
        if (File.Exists(resources_path + "config1.tmp"))
        {
            File.Delete(resources_path + "config.txt");
            File.WriteAllText(resources_path + "config.txt", File.ReadAllText(resources_path + "config1.tmp"));
            File.Delete(resources_path + "config1.tmp");
            AssetDatabase.Refresh();
        }
    }

    void BuildLuaAndCopyResources()
    {
        ABPackHelper.ShowProgress("", 1);
        AssetDatabase.Refresh();
        Packager.BuildAssetResource(ABPackHelper.GetBuildTarget());
        ABPackHelper.CopyAssets(ABPackHelper.BUILD_PATH + LuaConst.osDir);
        ABPackHelper.CopyToTempAssets();
        AssetDatabase.Refresh();
    }

    void RecordFileRealSize()
    {
        string[] abFiles = Directory.GetFiles(ABPackHelper.BUILD_PATH + LuaConst.osDir, "*.ab", SearchOption.AllDirectories);
        StringBuilder sb = new StringBuilder();
        var replaceStr = (ABPackHelper.BUILD_PATH + LuaConst.osDir).Replace("\\", "/");
        foreach (var abfile in abFiles)
        {
            var temp = abfile.Replace("\\", "/");
            var fileName = temp.Replace(replaceStr + "/", "").ToLower();
            sb.Append(fileName);
            sb.Append("|AssetType:");
            var abName = fileName.Split('.')[0];
            if (abTypeMaps.ContainsKey(abName))
            {
                var value = abTypeMaps[abName].Split('.');
                sb.Append(value[0]);
                sb.Append("|DownloadOrder:" + value[1]);
            }
            else
            {
                sb.Append("0");
            }
            sb.Append("\n");
        }
        string savePath = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/tempsizefile.txt";
        if (File.Exists(savePath)) File.Delete(savePath);
        File.WriteAllText(savePath, sb.ToString());
    }

    void CompressWithZSTD(long maxFileSize)
    {
        string outPutPath = Application.streamingAssetsPath + "/" + LuaConst.osDir;
        ABPackHelper.ShowProgress("Hold on...", 0);
        var dirInfo = new DirectoryInfo(outPutPath);
        var dirs = dirInfo.GetDirectories();
        Dictionary<int, List<string>> allFiles = new Dictionary<int, List<string>>();
        // data原始包控制在10M左右
        long curSize = 0;
        int tmpIndex = 0;
        for (int i = 0; i < dirs.Length; ++i )
        {
            if (dirs[i].Name == "lua") continue;
            var abFileInfos = dirs[i].GetFiles("*.*", SearchOption.AllDirectories);
            for (int j = 0; j < abFileInfos.Length; ++j)
            {
                if (abFileInfos[j].FullName.EndsWith(".meta")) continue;
                if (curSize >= maxFileSize)
                {
                    curSize = 0;
                    tmpIndex++;
                }
                if (curSize == 0) allFiles.Add(tmpIndex, new List<string>());
                var fileName = ABPackHelper.GetRelativeAssetsPath(abFileInfos[j].FullName);
                allFiles[tmpIndex].Add(fileName);
                curSize += File.ReadAllBytes(fileName).Length;
            }
        }
        int index = 0;
        // 合并生成的bundle文件，合成10M左右的小包(二进制)
        foreach (var key in allFiles.Keys)
        {
            var tmpName = "data" + key;
#if UNITY_IOS
            tmpName = IOSGenerateHelper.RenameResFileWithRandomCode(tmpName);
#endif
            var savePath = string.Format("{0}/{1}.tmp", outPutPath, tmpName);
            ABPackHelper.ShowProgress("Streaming data...", (float)index++ / (float)allFiles.Count);
            using (var fs = new FileStream(savePath, FileMode.CreateNew))
            {
                using (var writer = new BinaryWriter(fs))
                {
                    for (int i = 0; i < allFiles[key].Count; ++i)
                    {
                        var bytes = File.ReadAllBytes(allFiles[key][i]);
                        var abName = allFiles[key][i].Replace("Assets/StreamingAssets/" + LuaConst.osDir + "/", "");
                        writer.Write(abName);
                        writer.Write(bytes.Length);
                        writer.Write(bytes);
                    }
                }       
            }
        }
        ABPackHelper.ShowProgress("Finished...", 1);
        for (int i = 0; i < dirs.Length; ++i)
        {
            if (dirs[i].Name == "lua") continue;
            Directory.Delete(dirs[i].FullName, true);
        }
        AssetDatabase.Refresh();

        // 对合并后的文件进行压缩
        ABPackHelper.ShowProgress("Hold on...", 0);
        var pakFiles = Directory.GetFiles(outPutPath, "*.tmp", SearchOption.AllDirectories);
        for (int i = 0; i < pakFiles.Length; ++i)
        {
            var savePath = string.Format("{0}/{1}.bin", outPutPath, Path.GetFileNameWithoutExtension(pakFiles[i]));
            ABPackHelper.ShowProgress("compress with zstd...", (float)i / (float)pakFiles.Length);
            var fileName = ABPackHelper.GetRelativeAssetsPath(pakFiles[i]);
            using (var compressFs = new FileStream(savePath, FileMode.CreateNew))
            {
                using (var compressor = new Compressor(new CompressionOptions(CompressionOptions.MaxCompressionLevel)))
                {
                    var bytes = compressor.Wrap(File.ReadAllBytes(fileName));
#if UNITY_IOS
                    bytes = Crypto.Encode(bytes);
#endif
                    compressFs.Write(bytes, 0, bytes.Length);
                }
            }
            File.Delete(fileName);
        }
        ABPackHelper.ShowProgress("Finished...", 1);

        // 生成包体第一次进入游戏解压缩配置文件
        StringBuilder builder = new StringBuilder();
        string[] allfiles = Directory.GetFiles(outPutPath, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < allfiles.Length; ++i)
        {
            if (allfiles[i].EndsWith(".meta")) continue;
            if (allfiles[i].EndsWith("datamap.ab")) continue;

            var fileName = allfiles[i].Replace(outPutPath, "").Replace("\\", "/").TrimStart('/');
            builder.Append(fileName);
            builder.Append('|');
            builder.Append(MD5.ComputeHashString(allfiles[i]));
            builder.Append("\n");
        }
        var packFlistPath = outPutPath + "/packlist.txt";
        if (File.Exists(packFlistPath)) File.Delete(packFlistPath);
        File.WriteAllText(packFlistPath, builder.ToString());
        AssetDatabase.Refresh();
    }

    void UploadCheck()
    {
        if (sdkConfig.upload243 == 1)
            SVNHelper.UpdateTo243();
        else if (sdkConfig.uploadCDN == 1)
        {
            foreach (var item in sdkConfig.uploadPathes)
            {
                SVNHelper.UpdateToCDN(item.path, gameVersion.ToString(), item.script);
            }
        }
    }

#region ios 压缩特殊处理

    void CopyIOSCompressedData()
    {
        string to = ABPackHelper.TEMP_ASSET_PATH + "IOSCompress";
        if (Directory.Exists(to)) Directory.Delete(to, true);
        Directory.CreateDirectory(to);
        string from = Application.streamingAssetsPath;
        string[] files = Directory.GetFiles(from, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            if (files[i].EndsWith(".meta")) continue;
            var dest = files[i].Replace("\\", "/").Replace(Application.streamingAssetsPath + "/", "");
            var dirs = dest.Split('/');
            string path = to;
            ABPackHelper.ShowProgress("compress for ios: " + dest, (float)i / (float)files.Length);
            foreach (var dir in dirs)
            {
                if (dir.Contains('.')) continue;
                path += "/" + dir;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            File.Copy(files[i], to + "/" + dest, true);
        }
        ABPackHelper.ShowProgress("copy compressed data for ios...", 1);
        SVNHelper.CommitIOSCompressedData();
        AssetDatabase.Refresh();
    }

    void CopyCompressedData2IOSStreaming()
    {
        string to = Application.streamingAssetsPath;
        if (Directory.Exists(to)) Directory.Delete(to, true);
        Directory.CreateDirectory(to);
        string from = ABPackHelper.TEMP_ASSET_PATH + "IOSCompress";
        string[] files = Directory.GetFiles(from, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            var dest = files[i].Replace("\\", "/").Replace(ABPackHelper.TEMP_ASSET_PATH.Replace("\\", "/") + "IOSCompress/", "");
            var dirs = dest.Split('/');
            string path = to;
            ABPackHelper.ShowProgress("copy for ios..." + dest, (float)i / (float)files.Length);
            foreach (var dir in dirs)
            {
                if (dir.Contains('.')) continue;
                path += "/" + dir;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            File.Copy(files[i], to + "/" + dest, true);
        }
        ABPackHelper.ShowProgress("copy compressed data for ios...", 1);
        AssetDatabase.Refresh();
    }

#endregion
}
