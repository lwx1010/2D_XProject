using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class I18NUtil : Editor
{
    //UIPrefab文件夹目录
    private static string UIPrefabPath = Application.dataPath + "/Res/Prefab/Gui";
    //脚本的文件夹目录
    private static string ScriptPath = Application.dataPath + "/Scripts";
    //lua的文件目录
    private static string LuaPath = Application.dataPath + "/LuaFramework";
    //导出的中文KEY路径
    private static string OutPath = Application.dataPath + "/Res/I18N/out.txt";

    private static List<string> Localization = null;
    private static string staticWriteText = "";
//    [MenuItem("Tools/I18N多语言/导出多语言")]
    static void ExportChinese()
    {
        Localization = new List<string>();
        staticWriteText = "";

        //提取Prefab上的中文
        staticWriteText += "----------------Prefab----------------------\n";
        LoadDiectoryPrefab(new DirectoryInfo(UIPrefabPath));

        //提取CS中的中文
        staticWriteText += "----------------Script----------------------\n";
        LoadDiectoryCS(new DirectoryInfo(ScriptPath));

        //提取CS中的中文
        staticWriteText += "----------------Lua----------------------\n";
        LoadDiectoryLua(new DirectoryInfo(LuaPath));


        //最终把提取的中文生成出来
        string textPath = OutPath;
        if (System.IO.File.Exists(textPath))
        {
            File.Delete(textPath);
        }
        using (StreamWriter writer = new StreamWriter(textPath, false, Encoding.UTF8))
        {
            writer.Write(staticWriteText);
        }
        AssetDatabase.Refresh();
    }

    //递归所有UI Prefab
    static public void LoadDiectoryPrefab(DirectoryInfo dictoryInfo)
    {
        if (!dictoryInfo.Exists) return;
        FileInfo[] fileInfos = dictoryInfo.GetFiles("*.prefab", SearchOption.AllDirectories);
        int index = 0;
        foreach (FileInfo files in fileInfos)
        {
            ABPackHelper.ShowProgress("Check CHN in prefabs", (float)index / (float)fileInfos.Length);
            string path = files.FullName.Replace("\\", "/");
            string assetPath = path.Substring(path.IndexOf("Assets/"));
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            GameObject instance = GameObject.Instantiate(prefab) as GameObject;
            SearchPrefabString(instance.transform);
            GameObject.DestroyImmediate(instance);
            index++;
        }
        ABPackHelper.ShowProgress("Check CHN in prefabs", 1);
    }

    //递归所有C#代码
    static public void LoadDiectoryCS(DirectoryInfo dictoryInfo)
    {

        if (!dictoryInfo.Exists) return;
        FileInfo[] fileInfos = dictoryInfo.GetFiles("*.cs", SearchOption.AllDirectories);
        int index = 0;
        Regex rx = new Regex("[\u4e00-\u9fa5]+");
        foreach (FileInfo files in fileInfos)
        {
            ABPackHelper.ShowProgress("Check CHN in csripts", (float)index / (float)fileInfos.Length);
            string path = files.FullName.Replace("\\", "/");
            if (path.Contains("Editor/")) continue;
            string assetPath = path.Substring(path.IndexOf("Assets/"));
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset)) as TextAsset;
            string text = textAsset.text;
            //用正则表达式把代码里面两种字符串中间的字符串提取出来。
            Regex reg = new Regex("\"[^\"]*\"");
            MatchCollection mc = reg.Matches(text);
            foreach (Match m in mc)
            {
                if (rx.IsMatch(m.Value) && (!m.Value.StartsWith("//") && !m.Value.StartsWith("/*")))
                {
                    string format = m.Value.TrimStart('"').TrimEnd('"');
                    if (!Localization.Contains(format) && !string.IsNullOrEmpty(format))
                    {
                        Localization.Add(format);
                        staticWriteText += format + "\n";
                    }
                }
            }
            index++;
        }
        ABPackHelper.ShowProgress("Check CHN in csripts", 1);
    }

    //提取lua上的中文
    static public void LoadDiectoryLua(DirectoryInfo dictoryInfo)
    {

        if (!dictoryInfo.Exists) return;
        FileInfo[] fileInfos = dictoryInfo.GetFiles("*.lua", SearchOption.AllDirectories);
        int index = 0;
        Regex rx = new Regex("[\u4e00-\u9fa5]+");
        List<string> chnLuaAssets = new List<string>();
        foreach (FileInfo files in fileInfos)
        {
            ABPackHelper.ShowProgress("Check CHN in lua", (float)index / (float)fileInfos.Length);
            string path = files.FullName.Replace("\\", "/");
            string assetPath = path.Substring(path.IndexOf("Assets/"));
            string[] lines = File.ReadAllLines(assetPath);
            if (assetPath.Contains("language/") || assetPath.Contains("xlsdata/")
                || assetPath.Contains("Setting/"))
                continue;
            //用正则表达式把代码里面两种字符串中间的字符串提取出来。
            Regex reg = new Regex("\"[^\"]*\"");
            foreach (string line in lines)
            {
                MatchCollection mc = reg.Matches(line);
                foreach (Match m in mc)
                {
                    if (rx.IsMatch(m.Value))
                    {
                        string prefix = line.Substring(0, line.Length - m.Value.Length - 1);
                        if (prefix.EndsWith("print(") || prefix.EndsWith("log(") || prefix.EndsWith("logWarning(")
                            || prefix.EndsWith("logWarn(") || prefix.EndsWith("logError(") || prefix.EndsWith("error("))
                            continue;
                        string format = m.Value.TrimStart('"').TrimEnd('"');
                        if (!Localization.Contains(format) && !string.IsNullOrEmpty(format))
                        {
                            Localization.Add(format);
                            staticWriteText += format + "\n";
                            if (!chnLuaAssets.Contains(assetPath))
                                chnLuaAssets.Add(assetPath);
                        }
                    }
                }
            }
            index++;
        }
        ABPackHelper.ShowProgress("Check CHN in lua", 1);
        string luaCHNoutPath = Application.dataPath + "/Res/I18N/lua_out.txt";
        if (File.Exists(luaCHNoutPath)) File.Delete(luaCHNoutPath);
        File.WriteAllLines(luaCHNoutPath, chnLuaAssets.ToArray());
    }

    //提取Prefab上的中文
    static public void SearchPrefabString(Transform root)
    {
        foreach (Transform chind in root)
        {
            //因为这里是写例子，所以我用的是UILabel 
            //这里应该是写你用于图文混排的脚本。
            UILabel label = chind.GetComponent<UILabel>();
            if (label != null)
            {
                string text = label.text;
                if (!Localization.Contains(text) && !string.IsNullOrEmpty(text))
                {
                    Localization.Add(text);
                    text = text.Replace("\n", @"\n");
                    staticWriteText += text + "\n";
                }
            }
            if (chind.childCount > 0)
                SearchPrefabString(chind);
        }
    }
}