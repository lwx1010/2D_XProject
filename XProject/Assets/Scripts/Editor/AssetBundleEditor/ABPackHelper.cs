using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Threading;
using ZstdNet;

public class ABPackHelper
{
    const string VARIANT_V1 = "ab";

    static public BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.DeterministicAssetBundle 
        | BuildAssetBundleOptions.DisableWriteTypeTree 
        | BuildAssetBundleOptions.UncompressedAssetBundle;

    public static string BUILD_PATH = Application.dataPath + "/AssetBundle/";
    public static string TEMP_ASSET_PATH = Path.GetFullPath(Path.Combine(Application.dataPath, "../../tempAssets/")).Replace("\\", "/");
    public static string ASSET_PATH = Path.GetFullPath(Path.Combine(Application.dataPath, "../../assets/")).Replace("\\", "/");
    public static string VERSION_PATH = Path.GetFullPath(Path.Combine(Application.dataPath, "../version/")).Replace("\\", "/");

    public static void ShowProgress(string msg, float value)
    {
        EditorUtility.DisplayProgressBar("Hold on", msg, value);
        if (value == 1) EditorUtility.ClearProgressBar();
    }

    public static void MoveAsset(string fromPath, string toPath)
    {
        if (!Directory.Exists(toPath)) Directory.CreateDirectory(toPath);
        var dirInfo = new DirectoryInfo(fromPath);
        foreach (var file in dirInfo.GetFiles("*.*", SearchOption.AllDirectories))
        {
            if (file.Name.EndsWith(".meta")) continue;

            string assetPath = GetRelativeAssetsPath(file.FullName);
            string subPath = assetPath.Replace(fromPath, string.Empty);
            string newPath = toPath + subPath;
            var dirs = newPath.Split('/');
            string tempAssetPath = string.Empty;
            for (int i = 0; i < dirs.Length - 1; ++i)
            {
                tempAssetPath += dirs[i] + "/";
                if (!Directory.Exists(tempAssetPath))
                    Directory.CreateDirectory(tempAssetPath);
            }
            AssetDatabase.Refresh();
            string error = AssetDatabase.MoveAsset(assetPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
                return;
            }
            else
            {
                Debug.Log("Move asset from " + assetPath + " to " + newPath);
            }
            AssetDatabase.Refresh();
        }
    }

    public static string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }

    public static BuildTarget GetBuildTarget()
    {
        return EditorUserBuildSettings.activeBuildTarget;
    }

    public static void ClearAllAbName()
    {
        List<string> withExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".ogg", ".wav", ".jpg", ".png", ".bytes" };
        string[] files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories)
            .Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        var allfiles = AssetDatabase.GetDependencies(files);
        ShowProgress("", 0);
        for (int i = 0; i < allfiles.Length; ++i)
        {
            string file = allfiles[i];

            ShowProgress(file, (float)i / (float)allfiles.Length);
            if (file.EndsWith(".shader") || file.EndsWith(".cs") || file.EndsWith(".ttf")) continue;
            AssetImporter importer = AssetImporter.GetAtPath(GetRelativeAssetsPath(file));
            if (importer != null) importer.assetBundleName = string.Empty;
        }
        ShowProgress("", 1);
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }

    public static void ClearSceneAbName()
    {
        string[] files = Directory.GetFiles("Assets/Scenes/Level", "*.unity", SearchOption.TopDirectoryOnly);
        ShowProgress("", 0);
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i];

            ShowProgress(file, (float)i / (float)files.Length);
            AssetImporter importer = AssetImporter.GetAtPath(GetRelativeAssetsPath(file));
            if (importer != null) importer.assetBundleName = string.Empty;
        }
        ShowProgress("", 1);
        AssetDatabase.Refresh();
    }

    public static void ClearExceptSceneAbName()
    {
        List<string> withExtensions = new List<string>() { ".prefab", ".mat", ".asset", ".ogg", ".wav", ".jpg", ".png" };
        string[] files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories)
            .Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        var allfiles = AssetDatabase.GetDependencies(files);
        ShowProgress("", 0);
        for (int i = 0; i < allfiles.Length; ++i)
        {
            string file = allfiles[i];

            ShowProgress(file, (float)i / (float)allfiles.Length);
            if (file.EndsWith(".shader") || file.EndsWith(".cs") || file.EndsWith(".ttf")) continue;
            AssetImporter importer = AssetImporter.GetAtPath(GetRelativeAssetsPath(file));
            if (importer != null) importer.assetBundleName = string.Empty;
        }
        ShowProgress("", 1);
        AssetDatabase.Refresh();
    }

    public static Dictionary<string, List<string>> GetAllABDeps()
    {
        List<string> withExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".ogg", ".wav", ".jpg", ".png" };
        string[] files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories)
            .Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        var allfiles = AssetDatabase.GetDependencies(files);
        ShowProgress("", 0);
        Dictionary<string, List<string>> allDict = new Dictionary<string, List<string>>();
        for (int i = 0; i < allfiles.Length; ++i)
        {
            string file = allfiles[i];
            ShowProgress(file, (float)i / (float)allfiles.Length);
            if (file.EndsWith(".shader") || file.EndsWith(".cs") || file.EndsWith(".ttf")) continue;
            AssetImporter importer = AssetImporter.GetAtPath(GetRelativeAssetsPath(file));
            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                var key = importer.assetBundleName + "." + importer.assetBundleVariant;
                if (!allDict.ContainsKey(key))
                {
                    List<string> assets = new List<string>();
                    assets.Add(importer.assetPath);
                    allDict.Add(key, assets);
                }
                else
                {
                    allDict[key].Add(importer.assetPath);
                }
            }
        }
        ShowProgress("", 1);
        return allDict;
    }

    public static void CopyToTempAssets()
    {
        string fromPath = (BUILD_PATH + LuaConst.osDir).Replace("\\", "/");
        string toPath = TEMP_ASSET_PATH + LuaConst.osDir;
        if (Directory.Exists(toPath)) Directory.Delete(toPath, true);
        Directory.CreateDirectory(toPath);
        var dirInfo = new DirectoryInfo(fromPath);
        ShowProgress("", 0);
        int index = 0;
        FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            index++;
            ShowProgress("Copying to temp assets...", (float)index / (float)files.Length);
            if (file.Name.EndsWith(".meta") || file.Name.Contains("config.txt"))
                continue;

            string relativePath = file.FullName.Replace("\\", "/");
            string to = toPath.TrimEnd('/') + "/" + relativePath.Replace(fromPath, string.Empty);
            var dirs = to.Split('/');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dirs.Length - 1; ++i)
            {
                sb.Append(dirs[i]);
                sb.Append('/');
                if (!Directory.Exists(sb.ToString())) Directory.CreateDirectory(sb.ToString());
            }
            File.Copy(relativePath, to, true);
        }
        ShowProgress("", 1);
        AssetDatabase.Refresh();
    }

    public static void CopyToAssetBundle()
    {
        string fromPath = (TEMP_ASSET_PATH + LuaConst.osDir).Replace("\\", "/");
        string toPath = BUILD_PATH + LuaConst.osDir;
        if (Directory.Exists(toPath)) Directory.Delete(toPath, true);
        Directory.CreateDirectory(toPath);
        var dirInfo = new DirectoryInfo(fromPath);
        ShowProgress("", 0);
        int index = 0;
        FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            index++;
            ShowProgress("Copying to AssetBundle folder...", (float)index / (float)files.Length);
            if (file.Name.EndsWith(".meta") || file.Name.Contains("config.txt"))
                continue;

            string relativePath = file.FullName.Replace("\\", "/");
            string to = toPath.TrimEnd('/') + "/" + relativePath.Replace(fromPath, string.Empty);
            var dirs = to.Split('/');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dirs.Length - 1; ++i)
            {
                sb.Append(dirs[i]);
                sb.Append('/');
                if (!Directory.Exists(sb.ToString())) Directory.CreateDirectory(sb.ToString());
            }
            File.Copy(relativePath, to, true);
        }
        ShowProgress("", 1);
        AssetDatabase.Refresh();
    }

    public static void CopyAssets(string fromPath)
    {
        string toPath = ASSET_PATH + LuaConst.osDir;
        if (Directory.Exists(toPath)) Directory.Delete(toPath, true);
        Directory.CreateDirectory(toPath);
        var dirInfo = new DirectoryInfo(fromPath);
        ShowProgress("", 0);
        int index = 0;
        FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            index++;
            ShowProgress("Copying assets...", (float)index / (float)files.Length);
            if (file.Name.EndsWith(".meta") || file.Name.EndsWith(".manifest") || file.Name.Contains("config.txt"))
                continue;

            string relativePath = file.FullName.Replace("\\", "/");
            string to = toPath + "/" + relativePath.Replace(fromPath, string.Empty);
            var dirs = to.Split('/');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dirs.Length - 1; ++i)
            {
                sb.Append(dirs[i]);
                sb.Append('/');
                if (!Directory.Exists(sb.ToString())) Directory.CreateDirectory(sb.ToString());
            }
            if (relativePath.EndsWith(".ab"))
            {
                using (var compressor = new Compressor(new CompressionOptions(CompressionOptions.MaxCompressionLevel)))
                {
                    var buffer = compressor.Wrap(File.ReadAllBytes(relativePath));
                    File.WriteAllBytes(to, buffer);
                }
            }
            else File.Copy(relativePath, to, true);
        }
        ResetFlist();
        ShowProgress("", 1);
        AssetDatabase.Refresh();
    }

    static void ResetFlist()
    {
        string root = ASSET_PATH + LuaConst.osDir;
        string flistPath = root + "/files.txt";
        string[] fs = File.ReadAllLines(flistPath);
        List<string> list = new List<string>();
        for (int i = 0; i < fs.Length; ++i)
        {
            string[] elements = fs[i].Split('|');
            Debug.Assert(elements.Length >= 3);
            StringBuilder builder = new StringBuilder();
            for (int j = 0; j < elements.Length; ++j)
            {
                if (j == 2)
                    builder.Append(File.ReadAllBytes(root + "/" + elements[0]).Length);
                else
                    builder.Append(elements[j]);
                if (j != elements.Length - 1) builder.Append('|');
            }
            list.Add(builder.ToString());
        }
        File.WriteAllLines(flistPath, list.ToArray());
    }


    public static string[] GetBuildScenes()
    {
        List<string> validSceneNames = new List<string>(new string[] { "startscene", "updatescene", "loginscene", "preloadingscene", "createrolescene", "preloginscene" });
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
            {
                string name = Path.GetFileNameWithoutExtension(e.path).ToLower();
                if (validSceneNames.Contains(name))
                    names.Add(e.path);
            }
        }
        return names.ToArray();
    }

    public static void SaveVersion(string resVersion)
    {
#if UNITY_IOS
        var resVersionPath = ABPackHelper.VERSION_PATH + "version_ios.txt";
#else
        var resVersionPath = ABPackHelper.VERSION_PATH + "version.txt";
#endif
        if (File.Exists(resVersionPath)) File.Delete(resVersionPath);
        File.WriteAllText(resVersionPath, resVersion.ToString());
    }
}

