using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class BuildUtil : Editor
{

    public static string AssetBundleOutputPath = Application.streamingAssetsPath;

    public static Dictionary<string, DependecieData> DependenciesDict = new Dictionary<string, DependecieData>();     // 深度依赖字典

    public class DependecieData
    {
        public int DependecieCount; //  依赖次数
        public Dictionary<string,string> DependecieParentList =new Dictionary<string, string>();       //依赖他的父类
        
    }

    public static bool debug = true;

    public static string m_Extension = ".unity3d";

    /// <summary>  
    /// 清除之前设置过的AssetBundleName，避免产生不必要的资源也打包  
    /// 设置了AssetBundleName的，都会进行打包，不论在什么目录下 避免乱打包
    /// </summary> 
    public static void ClearAssetBundleName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        //Debug.Log("get the bundle length " + length);
        string[] oldAssetBundleName = new string[length];

        for (int i = 0; i < length; ++i)
        {
            oldAssetBundleName[i] = AssetDatabase.GetAllAssetBundleNames()[i];
            //Debug.Log(oldAssetBundleName[i]);
        }
        for (int i = 0; i < oldAssetBundleName.Length; ++i)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleName[i], true);
        }
        length = AssetDatabase.GetAllAssetBundleNames().Length;
        if (length == 0)
        {
            Debug.Log("Clear Asset Bundle Name Succeed ");
        }
        //Debug.Log(length);
    }

    public static void SelectionFile(string source, bool dependencies)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;

        for (int i = 0; i < length; ++i)
        {
            //隐藏文件
            if ((files[i].Attributes & FileAttributes.Hidden) == FileAttributes.Hidden )//&& (files[i].Attributes & FileAttributes.System) != FileAttributes.System)
            {
                continue;
            }
            if (files[i] is DirectoryInfo)
            {
                SelectionFile(files[i].FullName, dependencies);
            }
            else
            {

                if (!files[i].Name.EndsWith(".meta"))
                {
   
                    SetAssetBundleName(files[i].FullName);
                }
            }

        }
    }
    public static void SetAssetBundleName(string source)
    {
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);
        string _assetPath2 = _source.Substring(Application.dataPath.Length + 1);

        //在代码中给资源设置AssetBundleName  
        AssetImporter assetImporter = AssetImporter.GetAtPath(_assetPath);
        string onlyID = AssetDatabase.AssetPathToGUID(_assetPath);
        string extension = Path.GetExtension(_assetPath2);
        string assetName = "";

        // 如果是调试状态，用路径名，容易看出来,方便调试
        if (debug)
        {
            assetName = _assetPath2.Substring(_assetPath2.IndexOf("/") + 1);
            assetName = assetName.Replace(Path.GetExtension(assetName), m_Extension);

        }
        else
        {
            assetName = onlyID;
            assetName = string.Format("{0}{1}{2}", assetName, extension, m_Extension);

        }

        Debug.Log (assetName +"=="+ _assetPath2);  
        assetImporter.assetBundleName = assetName;
    }

    // 遍历检测深度依赖资源
    public static void CheckDependencies(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;

        for (int i = 0; i < length; ++i)
        {
            //隐藏文件
            if ((files[i].Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)//&& (files[i].Attributes & FileAttributes.System) != FileAttributes.System)
            {
                continue;
            }
            if (files[i] is DirectoryInfo)
            {
                CheckDependencies(files[i].FullName);
            }
            else
            {

                if (!files[i].Name.EndsWith(".meta"))
                {

                    Dependencies(files[i].FullName);
                    
                }
            }

        }
    }
    public static void Dependencies(string source)
    {
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);


        string onlyID = AssetDatabase.AssetPathToGUID(_assetPath);
        string[] dependenciesList = AssetDatabase.GetDependencies(_assetPath,true);
        int j = 0;
        // 将资源的深度引用加入到字典里面
        for (int i = 0; i < dependenciesList.Length; ++i)
        {
            //忽略自身
            if(dependenciesList[i] == _assetPath)
            {
                continue;
            }
            //忽略脚本
            if(dependenciesList[i].Contains(".cs"))
            {
                continue;
            }
            //忽略shader
            if (dependenciesList[i].Contains(".shader"))
            {
                continue;
            }
            j++;
            Debug.LogError(dependenciesList[i] +"===="+ dependenciesList.Length +"==="+ _assetPath+"==="+ j);
            if (DependenciesDict.ContainsKey(dependenciesList[i]))
            {
                DependenciesDict[dependenciesList[i]].DependecieCount = DependenciesDict[dependenciesList[i]].DependecieCount + 1;
                //父类依赖列表里面已经有了
                if (DependenciesDict[dependenciesList[i]].DependecieParentList.ContainsKey(_assetPath))
                {

                }else
                {
                    DependenciesDict[dependenciesList[i]].DependecieParentList.Add(_assetPath, Path.GetExtension(_assetPath));
                }
            }
            else
            {
                DependecieData dependeData = new DependecieData();
                dependeData.DependecieCount = 1;
                dependeData.DependecieParentList.Add(_assetPath, Path.GetExtension(_assetPath));
                DependenciesDict.Add(dependenciesList[i], dependeData);
            }
        }
    }

    public static void DependenciesLog()
    {
        foreach (var item in DependenciesDict)
        {
            Debug.Log("=path=" + item.Key + "=use count=" + item.Value.DependecieCount + "=id=" + AssetDatabase.AssetPathToGUID(item.Key));
            foreach (var parent in item.Value.DependecieParentList)
            {
                Debug.Log("=path1=" + item.Key + "=path=" + parent.Key + "=Extension=" + parent.Value + "=id=" );

            }
        }
    }

    public static string GetSelectFilePath()
    {
        string path = "";
        foreach (UnityEngine.Object o in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            path = AssetDatabase.GetAssetPath(o);
            break;
        }

        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("提示", "选中的路径为空", "ok");
            return path;
        }
        return path;

    }

    // 做一下字符串小写转化，斜杆转化
    public static string Replace(string s)
    {
        //s.ToLower();
        return s.Replace("\\", "/");
    }

    public static string GetPlatformFolder(UnityEditor.BuildTarget target)
    {
        switch (target)
        {
            case UnityEditor.BuildTarget.Android:
            case UnityEditor.BuildTarget.StandaloneWindows:
            case UnityEditor.BuildTarget.StandaloneWindows64:
                return "Android";
            case UnityEditor.BuildTarget.iOS:
            case UnityEditor.BuildTarget.StandaloneOSXIntel:
            case UnityEditor.BuildTarget.StandaloneOSXIntel64:
                return "IOS";
            default:
                return "Other";
        }
    }
    public static PlatformType GetPlatformType(UnityEditor.BuildTarget target)
    {
        switch (target)
        {
            case UnityEditor.BuildTarget.StandaloneWindows:
            case UnityEditor.BuildTarget.StandaloneWindows64:
                return PlatformType.Window;
            case UnityEditor.BuildTarget.StandaloneOSXIntel:
            case UnityEditor.BuildTarget.StandaloneOSXIntel64:
                return PlatformType.Mac;
            case UnityEditor.BuildTarget.Android:
   
                return PlatformType.Android;
            case UnityEditor.BuildTarget.iOS:

                return PlatformType.IOS;
            default:
                return PlatformType.None;
        }
    }

    public static BuildTarget GetBuildTarget(PlatformType type)
    {
        BuildTarget buildTarget = BuildTarget.StandaloneWindows;

        switch (type)
        {
            case PlatformType.Window:
                if (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64
                    || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows
                    || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                {
                    buildTarget = BuildTarget.StandaloneWindows;
                }
                break;
            case PlatformType.Mac:
                if (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneOSXIntel64
                    || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneOSXIntel
                    || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                {
                    buildTarget = BuildTarget.StandaloneOSXIntel64;

                }
                break;
            case PlatformType.Android:
                buildTarget = BuildTarget.Android;
                break;
            case PlatformType.IOS:
                buildTarget = BuildTarget.iOS;
                break;
        }
        return buildTarget;
    }

    public static string GetFileHash(string filePath)
    {
        try
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            int len = (int)fs.Length;
            byte[] data = new byte[len];
            fs.Read(data, 0, len);
            fs.Close();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            string fileMD5 = "";
            foreach (byte b in result)
            {
                fileMD5 += Convert.ToString(b, 16);
            }
            return fileMD5;
        }
        catch (FileNotFoundException e)
        {
            //Console.WriteLine(e.Message);
            Debug.LogError(e.Message);
            return "";
        }
    }
    public static Dictionary<string, string> md5Dict = new Dictionary<string, string>();
    public static Dictionary<string, long> SizeDict = new Dictionary<string, long>();

    public static void GeneratorMD5(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;


        for (int i = 0; i < length; ++i)
        {
            if (files[i] is DirectoryInfo)
            {
                GeneratorMD5(files[i].FullName);
            }
            else
            {
				if (files[i].Name.EndsWith(".meta"))
				{
					continue;
				}
                string md5Str = BuildUtil.GetFileHash(files[i].FullName);
                if(md5Dict.ContainsKey(md5Str))
                {
                    Debug.LogError("have same MD5 key :" + md5Str);
                    continue;
                }
                else
				{
                    if(!files[i].FullName.Contains("updata.txt"))//不把update.txt的md5写入，因为update.txt只是用来自己看的
                    {
                        string platform = GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget);
                        int index = files[i].FullName.IndexOf(platform);
                        if (index >= 0)
                        {
                            index += platform.Length + 1;
                            string name = files[i].FullName.Substring(index);
                            name = Replace(name);
                            long size =GetFileSize(files[i].FullName);
                            //Debug.LogError(files[i].FullName+"====size=" + size + "===="+ CountSize(size));
                            SizeDict.Add(name, size);
                           md5Dict.Add(name, md5Str);
                        }
                    }
                   // md5Dict.Add(md5Str, files[i].FullName);
                }

            }

        }
    }

    public static long GetFileSize(string sFullName)
    {
        long lSize = 0;
        if (File.Exists(sFullName))
            lSize = new FileInfo(sFullName).Length;
        return lSize;
    }

    public static string CountSize(long Size)
    {
        string m_strSize = "";
        long FactSize = 0;
        FactSize = Size;
        if (FactSize < 1024.00)
            m_strSize = FactSize.ToString("F2") + " Byte";
        else if (FactSize >= 1024.00 && FactSize < 1048576)
            m_strSize = (FactSize / 1024.00).ToString("F2") + " K";
        else if (FactSize >= 1048576 && FactSize < 1073741824)
            m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " M";
        else if (FactSize >= 1073741824)
            m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
        return m_strSize;
    }

    public static void WriteFile()
    {
        string path = Path.Combine(AssetBundleOutputPath ,BuildUtil.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        path  = path + "/assetMD5.txt";

        string UpdaPath = Path.Combine(AssetBundleOutputPath, BuildUtil.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        UpdaPath = UpdaPath + "/updata.txt";

        Dictionary<string, string> md5OldDict = new Dictionary<string, string>();
        Dictionary<string, string> md5ChangDict = new Dictionary<string, string>();

        if (File.Exists(path))//如果存在就删除
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                String[] md5 = line.Split(':');
                if(!md5OldDict.ContainsKey(md5[1]))
                {
                    if(md5.Length>=3)
                    {
                        md5OldDict.Add(md5[1], md5[3]);
                    }
                }
                else
                {
                    Debug.LogError("have same key :" + md5[1]);
                }
            }
            sr.Dispose();
            sr.Close();
            //资源更新完才能删
            File.Delete(path);
        }
        FileStream fs = new FileStream(path, FileMode.Create);

        StreamWriter sw = new StreamWriter(fs);
		sw.Write("Version:" + PlayerSettings.bundleVersion+"\r\n");
        //sw.Write(System.Environment.NewLine);
		sw.Write("VersionCode:" + PlayerSettings.Android.bundleVersionCode+"\r\n");
        //sw.Write(System.Environment.NewLine);
        long size = 0;
        //开始写入
        foreach (KeyValuePair<string ,string> kvp in md5Dict)
        {
            if(!md5OldDict.ContainsKey(kvp.Key))
            {
                if (md5ChangDict.ContainsKey(kvp.Key))
                {
                    Debug.LogError("have same key :" + kvp.Key);

                }
                else
                {
                    md5ChangDict.Add(kvp.Key, kvp.Value);

                }
            }
            if(SizeDict.ContainsKey(kvp.Key))
            {
                size = SizeDict[kvp.Key];
            }
			sw.Write("Name:" + kvp.Key + ":" + "MD5:" + kvp.Value+":Size:"+size+"\r\n");
          //  sw.Write("MD5:" + kvp.Key + ":  Name:" + kvp.Value);
            //sw.Write(System.Environment.NewLine);
        }


        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
        fs.Close();

        if (File.Exists(UpdaPath))//如果存在就删除
        {
            File.Delete(UpdaPath);
        }

        fs = new FileStream(UpdaPath, FileMode.Create);

        sw = new StreamWriter(fs);

        foreach (KeyValuePair<string, string> kvp in md5ChangDict)
        {
			sw.Write("Name:" + kvp.Key + ":" + "MD5:" + kvp.Value+"\r\n");
           // sw.Write("MD5:" + kvp.Key + ":  Name:" + kvp.Value);
            //sw.Write(System.Environment.NewLine);
        }
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
        fs.Close();
    }

    public static void Reset()
    {
        DependenciesDict.Clear();
    }

    public enum PlatformType
    {
        None,
        Android,
        IOS,
        Window,
        Mac,

    }
}
