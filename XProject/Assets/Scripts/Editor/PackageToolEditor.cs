using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class PackageToolEditor : EditorWindow 
{
    private static bool ms_isDebugBuild = false;
    private static BuildTarget ms_buildTarget = BuildTarget.Android;

    private static string XCODE_PROJECT_NAME = "XCodeProject";
    //private static string BUILD_OUTPUT_ANDROID_BIN = "/Public/Update/TestServer/Bin/Android";
    private static string BUILD_OUTPUT_ANDROID_RES = "/Public/Update/TestServer/Resources/Android/resources";
    private static string BUILD_OUTPUT_ANDROID_APK_DEBUG = "/Public/Update/TestServer/Resources/Android/apk_debug";
    private static string BUILD_OUTPUT_ANDROID_APK_RELEASE = "/Public/Update/TestServer/Resources/Android/apk_release";
    //private static string BUILD_OUTPUT_IOS_BIN = "/Public/Update/TestServer/Bin/IOS";
    private static string BUILD_OUTPUT_IOS_RES = "/Public/Update/TestServer/Resources/IOS";
    private static string BUILD_OUTPUT_WINDOWS_BIN = "/Public/Update/TestServer/Bin/Windows";
    //private static string BUILD_OUTPUT_WINDOWS_RES = "/Public/Update/TestServer/Resources/Windows";

    private static string BUILD_OUTPUT_WP8_RES = "/Public/Update/TestServer/Resources/WP8/resources";
    //private static string BUILD_OUTPUT_WP8_APK = "/Public/Update/TestServer/Resources/WP8/player";

    private float startTime;

    [MenuItem("AssetBundlePacker/打包工具集合")]
    static void AddWindow()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 1000, 400);
        PackageToolEditor window = (PackageToolEditor)EditorWindow.GetWindowWithRect(typeof(PackageToolEditor), wr, true, "工具集合");
        window.Show();
    }

    void OnGUI()
    {
        // 安卓
        AndroidTool();
        // IOS
        IPhoneTool();
        // wp8
        WP8Tool();
    }

    void AndroidTool()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("安卓整包(Debug)", GUILayout.Width(140)))
        {
            ms_isDebugBuild = true;
            ms_buildTarget = BuildTarget.Android;
            BuildResources();
            CopyResources(BUILD_OUTPUT_ANDROID_RES);
            ComputePackageSize();
            Build();
        }
        else if (GUILayout.Button("安卓整包(Release)", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.Android;
            BuildResources();
            CopyResources(BUILD_OUTPUT_ANDROID_RES);
            //ComputePackageSize();
            Build();
        }
        else if (GUILayout.Button("安卓资源+单独apk", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.Android;
            BuildResources();
            CopyResources(BUILD_OUTPUT_ANDROID_RES);
            BuildSingleApk();
        }
        else if (GUILayout.Button("安卓单独apk", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.Android;
            BuildSingleApk();
        }
        else if (GUILayout.Button("安卓外网单独apk", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.Android;
            BuildSingleApk(true, false);
        }
        else if (GUILayout.Button("安卓资源", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.Android;
            BuildResources();
            CopyResources(BUILD_OUTPUT_ANDROID_RES);
        }
        else if (GUILayout.Button("安卓单独打表", GUILayout.Width(130)))
        {
            ms_buildTarget = BuildTarget.Android;
            //FileManager.ClearPath(Application.streamingAssetsPath, new List<string>(new string[2] { "ogg", "mp4" }));
            //AtMePackerEditor.BuildTables();
            //AtMePackerEditor.BytesRename(Application.streamingAssetsPath);
            //AtMePackerEditor.EncryptResources();
            CopyResources(BUILD_OUTPUT_ANDROID_RES, false);
        }
        GUILayout.EndHorizontal(); 
    }

    void WP8Tool()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("UWP整包(Debug)", GUILayout.Width(140)))
        {
            ms_isDebugBuild = true;
            ms_buildTarget = BuildTarget.WSAPlayer;
            BuildResources();
            CopyResources(BUILD_OUTPUT_WP8_RES);
            CopyFlist(BUILD_OUTPUT_WP8_RES);
            Build();
        }
        else if (GUILayout.Button("UWP整包(Release)", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.WSAPlayer;
            BuildResources();
            CopyResources(BUILD_OUTPUT_WP8_RES);
            CopyFlist(BUILD_OUTPUT_WP8_RES);
            Build();
        }
        else if (GUILayout.Button("UWP资源+单独apk", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.WSAPlayer;
            BuildResources();
            CopyResources(BUILD_OUTPUT_WP8_RES);
            BuildSingleApk();
        }
        else if (GUILayout.Button("UWP单独apk", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.WSAPlayer;
            BuildSingleApk();
        }
        else if (GUILayout.Button("UWP外网单独apk", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.WSAPlayer;
            BuildSingleApk();
        }
        else if (GUILayout.Button("UWP资源", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.WSAPlayer;
            BuildResources();
            CopyResources(BUILD_OUTPUT_WP8_RES);
        }
        else if (GUILayout.Button("UWP单独打表", GUILayout.Width(130)))
        {
            ms_buildTarget = BuildTarget.WSAPlayer;
            //FileManager.ClearPath(Application.streamingAssetsPath, new List<string>(new string[2] { "ogg", "mp4" }));
            //AtMePackerEditor.BuildTables();
            //AtMePackerEditor.BytesRename(Application.streamingAssetsPath);
            CopyResources(BUILD_OUTPUT_WP8_RES, false);
        }
        GUILayout.EndHorizontal(); 
    }

    void CopyFlist(string platformPath)
    {
        string locationResPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + platformPath;
        string flistPath = locationResPath + "/flist.txt";
        if (File.Exists(Application.dataPath + "/Resources/flist.txt"))
            File.Delete(Application.dataPath + "/Resources/flist.txt");
        File.WriteAllText(Application.dataPath + "/Resources/flist.txt", File.ReadAllText(flistPath));
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 单独打包apk
    /// </summary>
    /// <param name="increaseVersion">是否需要单独更新版本号,打外网包时,同时打包含资源的整包和不包含资源的更新包</param>
    void BuildSingleApk(bool increaseVersion = true, bool debug = true)
    {
        FileManager.ClearPath(Application.streamingAssetsPath, null);
        AssetDatabase.Refresh();
        string locationPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + (debug ? BUILD_OUTPUT_ANDROID_APK_DEBUG : BUILD_OUTPUT_ANDROID_APK_RELEASE);
        string bundleVersion = PlayerSettings.bundleVersion;
        if (!Directory.Exists(locationPath + "/" + bundleVersion))
            Directory.CreateDirectory(locationPath + "/" + bundleVersion);
        locationPath += "/" + bundleVersion + "/";
        int gameVer = 0;
        int resVer = 0;
        string locationResPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_ANDROID_RES;
        string flistPath = locationResPath + "/flist.txt";
        if (GetCurVersion(flistPath, out gameVer, out resVer) && increaseVersion)
        {
            gameVer++;
            File.Delete(flistPath);
        }
        //if (increaseVersion)
        //{
        //    // 生成更新目录flist
        //    string updateFlist = AtMePackerEditor.GenerateFlist(locationResPath, gameVer, resVer);
        //    File.WriteAllText(flistPath, updateFlist);
        //}
        //// 生成更新包内flist
        //string apkFlistPath = Application.dataPath + "/Resources/flist.txt";
        //if (File.Exists(apkFlistPath))
        //    File.Delete(apkFlistPath);
        //string apkFlist = AtMePackerEditor.GenerateFlist(Application.streamingAssetsPath, gameVer, 0);
        //File.WriteAllText(apkFlistPath, apkFlist);
        //AssetDatabase.Refresh();
        //string apkName = "cn.atme.darkfairytale_a" + gameVer + ".apk";
        //BuildPipeline.BuildPlayer(GetBuildScenes(), locationPath + "/" + apkName, ms_buildTarget, BuildOptions.None);
    }

    void IPhoneTool()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("iPhone整包(Debug)", GUILayout.Width(140)))
        {
            ms_isDebugBuild = true;
            ms_buildTarget = BuildTarget.iOS;
            BuildResources();
            CopyResources(BUILD_OUTPUT_IOS_RES);
            CopyFlist(BUILD_OUTPUT_IOS_RES);
            Build();
        }
        else if (GUILayout.Button("iPhone整包(Release)", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.iOS;
            BuildResources();
            CopyResources(BUILD_OUTPUT_IOS_RES);
            CopyFlist(BUILD_OUTPUT_IOS_RES);
            Build();
        }
        else if (GUILayout.Button("iPhone资源", GUILayout.Width(140)))
        {
            ms_isDebugBuild = false;
            ms_buildTarget = BuildTarget.iOS;
            BuildResources();
            CopyResources(BUILD_OUTPUT_IOS_RES);
        }
        GUILayout.EndHorizontal();
    }

    void Build()
    {
        try
        {
            Debug.Log("Build Player");
            BuildOptions buildOption = BuildOptions.None;
            if (ms_isDebugBuild)
            {
                buildOption |= BuildOptions.Development;
                buildOption |= BuildOptions.AllowDebugging;
                buildOption |= BuildOptions.ConnectWithProfiler;
            }
            else
            {
                buildOption |= BuildOptions.None;
            }

            string locationPathName;
            if (BuildTarget.iOS == ms_buildTarget)
            {
                locationPathName = XCODE_PROJECT_NAME;
            }
            else if (BuildTarget.StandaloneWindows == ms_buildTarget)
            {
                locationPathName = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_WINDOWS_BIN + "/";
                System.DateTime time = System.DateTime.Now;
                locationPathName += "cn.atme.darkfairytale_" + time.Month.ToString("D2") + time.Day.ToString("D2") + time.Hour.ToString("D2") + time.Minute.ToString("D2");
                if (!Directory.Exists(locationPathName))
                    Directory.CreateDirectory(locationPathName);
                locationPathName += "/cn.atme.darkfairytale.exe";
            }
            else
            {
                //int gameVer = 0;
                //int resVer = 0;
                //string locationResPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_ANDROID_RES;
                //string flistPath = locationResPath + "/flist.txt";
                //// 得到更新目录flist
                //GetCurVersion(flistPath, out gameVer, out resVer);
                //// 生成整包内flist
                //string apkFlistPath = Application.dataPath + "/Resources/flist.txt";
                //if (File.Exists(apkFlistPath))
                //    File.Delete(apkFlistPath);
                //string apkFlist = AtMePackerEditor.GenerateFlist(Application.streamingAssetsPath, gameVer, resVer);
                //File.WriteAllText(apkFlistPath, apkFlist);
                //AssetDatabase.Refresh();
                //locationPathName = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_ANDROID_BIN + "/";
                //locationPathName += "cn.atme.darkfairytale_a" + gameVer + ".apk";
            }
            //BuildPipeline.BuildPlayer(GetBuildScenes(), locationPathName, ms_buildTarget, buildOption);
            //if (ms_buildTarget == BuildTarget.Android)
            //{
            //    BuildSingleApk(false, false);
            //}

            Debug.Log("<color=red>Build finished, cost " + ((Time.realtimeSinceStartup - startTime) / 60f) + "mins</color>");
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }

    //在这里找出你当前工程所有的场景文件
    string[] GetBuildScenes()
    {
        List<string> validSceneNames = new List<string>(new string[] { "startscene", "updatescene", "loginscene", "preloadingscene", "createrolescene" });
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
            {
                string name = Path.GetFileName(e.path).ToLower().Split('.')[0];
                if (validSceneNames.Contains(name))
                    names.Add(e.path);
            }
        }
        return names.ToArray();
    }

    void BuildResources()
    {
        try
        {
            startTime = Time.realtimeSinceStartup;
            //AtMePackerEditor.SwitchAndBuildResources(ms_buildTarget);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }

    bool GetCurVersion(string path, out int gameVer, out int resVer)
    {
        gameVer = 0;
        resVer = 0;
        if (File.Exists(path))
        {
            foreach (var line in File.ReadAllLines(path))
            {
                string[] vers = line.Split('|')[0].Split('.');
                gameVer = Convert.ToInt32(vers[2].TrimStart('g'));
                resVer = Convert.ToInt32(vers[3].TrimStart('r'));
                Debug.Log("game ver:" + gameVer + " res ver: " + resVer);
                break;
            }
            return true;
        }
        return false;
    }

    void CopyResources(string platformPath, bool clearPath = true)
    {
        DirectoryInfo rootInfo = new DirectoryInfo(Application.streamingAssetsPath);
        string locationResPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + platformPath;
        // 得到资源更新目录下的版本号，将其+1
        string flistPath = locationResPath + "/flist.txt";
        int resVer = 0;
        int gameVer = 0;
        if (GetCurVersion(flistPath, out gameVer, out resVer))
        {
            resVer++;
            File.Delete(flistPath);
        }
        // 清空资源目录
        if (clearPath)
            FileManager.ClearPath(locationResPath, null);
        // 拷贝资源文件
        foreach (var dirInfo in rootInfo.GetDirectories())
        {
            string path = locationResPath + "/" + dirInfo.Name;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                if (fileInfo.Name.EndsWith(".meta"))
                    continue;
                FileManager.BinaryCopyFile(fileInfo.FullName, path + "/" + fileInfo.Name);
            }
        }
        foreach (var fileInfo in rootInfo.GetFiles())
        {
            if (fileInfo.Name.EndsWith(".meta"))
                continue;
            FileManager.BinaryCopyFile(fileInfo.FullName, locationResPath + "/" + fileInfo.Name);
        }
        // 生成flist
        //string flist = AtMePackerEditor.GenerateFlist(locationResPath, gameVer, resVer);
        //File.WriteAllText(flistPath, flist);
        //if (Time.realtimeSinceStartup > startTime)
        //    Debug.Log("<color=red>Build finished, cost " + ((Time.realtimeSinceStartup - startTime) / 60f) + "mins</color>");
    }

    void ComputePackageSize()
    {
        int totalSize = 1024 * 1024 * 100;
        int startSize = 1024 * 1024 * 45;
        DirectoryInfo dirInfo = new DirectoryInfo(Application.streamingAssetsPath);
        List<string> saveName = new List<string>();
        FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.Name.EndsWith(".meta"))
                continue;

            byte[] data = File.ReadAllBytes(file.FullName);
            if (startSize + data.Length > totalSize)
                break;
            startSize += data.Length;
            saveName.Add(file.Name);
        }

        foreach (var file in files)
        {
            if (file.Name.EndsWith(".meta") || saveName.Contains(file.Name))
                continue;

            string assetPath = file.FullName.Substring(file.FullName.IndexOf("Assets")).Replace('\\', '/');
            AssetDatabase.DeleteAsset(assetPath);
        }
        AssetDatabase.Refresh();

        string locationResPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_ANDROID_RES;
        string flistPath = locationResPath + "/flist.txt";
        int gameVer = 0;
        int resVer = 0;
        if (File.Exists(flistPath))
        {
            foreach (var line in File.ReadAllLines(flistPath))
            {
                string[] vers = line.Split('|')[0].Split('.');
                gameVer = Convert.ToInt32(vers[2].TrimStart('g'));
                resVer = Convert.ToInt32(vers[3].TrimStart('r'));
                Debug.Log("game ver:" + gameVer + " res ver: " + resVer);
                break;
            }
        }
        //string rootDir = Application.streamingAssetsPath;
        //string flist = AtMePackerEditor.GenerateFlist(rootDir, gameVer, resVer);
        //if (string.IsNullOrEmpty(flist)) return;
        //if (File.Exists(Application.dataPath + "/Resources/flist.txt"))
        //    File.Delete(Application.dataPath + "/Resources/flist.txt");
        //FileManager.WriteFile(Application.dataPath + "/Resources/flist.txt", flist);
    }
}
