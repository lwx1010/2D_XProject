using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Xml;
using System;
using System.Linq;

/// <summary>
/// AB打包数据结构
/// </summary>
public class ABData
{
    public delegate void AttributeChanged(string attrName, params object[] vals);
    public static List<AttributeChanged> attrChangeCallbacks = new List<AttributeChanged>();

    public static Dictionary<string, ABData> datas = new Dictionary<string, ABData>();

    public enum PackType
    {
        Together = 0,    // 目录下所有文件一起打包
        Seperate = 1,    // 目录下所有文件分开打包
    }

    public enum ABDataType
    {
        Folder = 0,
        File = 1,
    }

    public enum FileFilterType
    {
        Prefab = 0,
        Picture = 1,
        Scene = 2,
        Material = 3,
        Sound = 4,
        Bytes = 5,
    }

    /// <summary>
    /// 资源类型
    /// </summary>
    public enum AssetType
    {
        InPackage = 0,      // 整包资源(包含在包体中)
        OutPackage,         // 整包资源(不包含在包体中)
        PacthResources,     // 补丁资源（额外添加资源）
        NoNeedToDownload,   // 不需要下载的资源()
    }

    string[] packPopList = new string[2] { ABLanguage.ALLPACK, ABLanguage.SUBPACK };
    string[] preloadPopList = new string[2] { ABLanguage.PRELOAD, ABLanguage.NONE };
    string[] showFileOnlyPopList = new string[2] { ABLanguage.DISPLAY_FILES_ONLY, ABLanguage.DISPLAY_ALL };
    string[] assetTypePopList = new string[4] 
    {
        ABLanguage.FULLPACK_RES, ABLanguage.SUBPACK_RES, ABLanguage.PATCH_RES, ABLanguage.UNDOWNLOAD
    };
    string[] filterPopList = new string[6] 
    {
        ABLanguage.PREFAB_FILE,
        ABLanguage.PICTURE_FILE,
        ABLanguage.SCENE_FILE,
        ABLanguage.MATIRIAL_FILE,
        ABLanguage.SOUND_FILE,
        ABLanguage.Bytes_File,
    };
    const string VARIANT_V1 = "ab";

    /// <summary>
    /// 资源路径
    /// </summary>
    public string abPath { get; private set; }
    /// <summary>
    /// AssetbundleName
    /// </summary>
    public string abName { get; private set; }
    /// <summary>
    /// Gui显示层级
    /// </summary>
    public int layer { get; private set; }
    /// <summary>
    /// 打包顺序
    /// </summary>
    public int packOrder { get; private set; }
    /// <summary>
    /// 是否显示子层级
    /// </summary>
    public bool showChild { get; private set; }
    /// <summary>
    /// 是否只显示文件
    /// </summary>
    public bool showFileOnly { get; private set; }
    /// <summary>
    /// 删除标志位
    /// </summary>
    public bool removeFlag { get; private set; }
    /// <summary>
    /// 资源类型
    /// </summary>
    public int assetType { get; private set; }
    /// <summary>
    /// 是否游戏启动时加载
    /// </summary>
    public bool preload { get; private set; }
    /// <summary>
    /// 数据类型（文件或者文件夹）
    /// </summary>
    public ABDataType dataType { get; private set; }
    /// <summary>
    /// 打包类型
    /// </summary>
    public PackType packType { get; private set; }
    /// <summary>
    /// 文件过滤类型
    /// </summary>
    public FileFilterType filterType { get; private set; }
    /// <summary>
    /// 下载顺序
    /// </summary>
    public int downloadOrder { get; private set; }
    /// <summary>
    /// 是否是刷新资源
    /// </summary>
    public bool isNew { get; private set; }
    /// <summary>
    /// 子文件夹
    /// </summary>
    public Dictionary<string, ABData> folders { get; private set; }
    /// <summary>
    /// 子文件
    /// </summary>
    public Dictionary<string, ABData> files { get; private set; }

    /// <summary>
    /// 父节点
    /// </summary>
    ABData parent;

    public ABData(ABData parent, string abPath, string abName, int dataType, int packType, int layer, 
        int filterType, bool showFileOnly, bool preload, int assetType, bool removeFlag, int packOrder, int downloadOrder, bool isNew)
    {
        this.parent = parent;
        this.abPath = abPath;
        this.abName = abName.ToLower();
        this.dataType = (ABDataType)dataType;
        this.packType = (PackType)packType;
        this.layer = layer;
        this.packOrder = packOrder;
        this.filterType = (FileFilterType)filterType;
        this.showFileOnly = showFileOnly;
        this.showChild = true;
        this.preload = preload;
        this.assetType = assetType;
        this.removeFlag = removeFlag;
        this.downloadOrder = downloadOrder;
        this.isNew = isNew;
        this.folders = new Dictionary<string, ABData>();
        this.files = new Dictionary<string, ABData>();
        attrChangeCallbacks.Add(this.OnAttributesChanged);
    }

    #region ABData GUI Display
    public void DisplayPanel()
    {
        if (removeFlag) return;

        if (dataType == ABDataType.Folder)
        {
            DisplayFolder();
        }
        else if (dataType == ABDataType.File)
        {
            DisplayFile();
        }
    }

    void DisplayFolder()
    {
        if (dataType != ABDataType.Folder) return;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(layer * 10 + 10);

        if (packType == PackType.Seperate)
        {
            GUILayout.Space(10);
            GUI.backgroundColor = Color.green;
            if (!showChild)
            {
                if (GUILayout.Button("+", GUILayout.Width(20))) showChild = true;
            }
            else
            {
                if (GUILayout.Button("-", GUILayout.Width(20))) showChild = false;
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            showChild = true;
            if (folders.Count > 0) folders.Clear();
            if (files.Count > 0) files.Clear();
            GUILayout.Space(34);
        }

        if (isNew) GUI.contentColor = Color.red;
        abPath = abPath.Substring(abPath.IndexOf("Assets")).Replace("\\", "/");
        EditorGUILayout.TextField(abPath, GUILayout.Width(260));
        if (isNew) GUI.contentColor = Color.white;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.cyan;
        if (!File.Exists(abPath))
            packType = (PackType)ABTools.DrawPrefixList((int)packType, packPopList, GUILayout.Width(80));
        else
            packType = (PackType)ABTools.DrawPrefixList(1, packPopList, GUILayout.Width(80));
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(ABLanguage.AB_NAME, GUILayout.Width(50));
        if (packType == PackType.Together && string.IsNullOrEmpty(abName))
        {
            var temps = abPath.Split('/');
            abName = temps[temps.Length - 1].ToLower();
        }
        GUI.contentColor = Color.green;
        abName = EditorGUILayout.TextField(abName, GUILayout.Width(150)).ToLower();
        GUI.contentColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(ABLanguage.PACK_ORDER, GUILayout.Width(65));
        var lastOrder = packOrder;
        packOrder = Convert.ToInt32(EditorGUILayout.TextField(packOrder.ToString(), GUILayout.Width(50)));
        if (lastOrder != packOrder)
        {
            for (int i = 0; i < attrChangeCallbacks.Count; ++i)
                attrChangeCallbacks[i]("packOrder", packOrder, abName);
        }

        GUILayout.Space(10);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("delete", GUILayout.Width(80)))
        {
            RemovePath();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(ABLanguage.FILE_FILTER, GUILayout.Width(75));
        GUI.backgroundColor = Color.cyan;
        filterType = (FileFilterType)ABTools.DrawPrefixList((int)filterType, filterPopList, GUILayout.Width(60));
        GUI.backgroundColor = Color.white;

        if (packType == PackType.Seperate)
        {
            GUILayout.Space(10);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("refresh", GUILayout.Width(80)))
            {
                RefreshFolder(true);
                RefreshFile(true);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUI.backgroundColor = Color.yellow;
            var index1 = ABTools.DrawPrefixList(showFileOnly ? 0 : 1, showFileOnlyPopList, GUILayout.Width(80));
            showFileOnly = index1 == 0;
            GUI.backgroundColor = Color.white;
        }

        GUILayout.Space(10);
        GUI.backgroundColor = Color.yellow;
        var lastIndex2 = preload ? 0 : 1;
        var index2 = ABTools.DrawPrefixList(preload ? 0 : 1, preloadPopList, GUILayout.Width(80));
        preload = index2 == 0;
        if (lastIndex2 != index2)
        {
            for (int i = 0; i < attrChangeCallbacks.Count; ++i)
                attrChangeCallbacks[i]("preload", preload, abName);
        }

        GUILayout.Space(10);
        var lastIndex3 = assetType;
        assetType = ABTools.DrawPrefixList(assetType, assetTypePopList, GUILayout.Width(110));
        if (lastIndex3 != assetType)
        {
            for (int i = 0; i < attrChangeCallbacks.Count; ++i)
                attrChangeCallbacks[i]("assetType", assetType, abName);
        }
        if (assetType == 1)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(ABLanguage.DOWNLOAD_ORDER, GUILayout.Width(100));
            downloadOrder = Convert.ToInt32(EditorGUILayout.TextField(downloadOrder.ToString(), GUILayout.Width(20)));
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal(); 

        if (showChild && packType == PackType.Seperate)
        {
            DisplaySubFolderPanel();
        }
    }

    void DisplayFile()
    {
        if (dataType != ABDataType.File) return;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(layer * 10 + 50);

        if (isNew) GUI.contentColor = Color.red;
        abPath = abPath.Substring(abPath.IndexOf("Assets"));
        EditorGUILayout.TextField(abPath, GUILayout.Width(350));
        if (isNew) GUI.contentColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(ABLanguage.AB_NAME, GUILayout.Width(50));
        GUI.contentColor = Color.green;
        abName = EditorGUILayout.TextField(abName, GUILayout.Width(280)).ToLower();
        GUI.contentColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(ABLanguage.PACK_ORDER, GUILayout.Width(65));
        var lastOrder = packOrder;
        packOrder = Convert.ToInt32(EditorGUILayout.TextField(packOrder.ToString(), GUILayout.Width(50)));
        if (lastOrder != packOrder)
        {
            for (int i = 0; i < attrChangeCallbacks.Count; ++i)
                attrChangeCallbacks[i]("packOrder", packOrder, abName);
        }

        GUILayout.Space(10);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("delete", GUILayout.Width(80)))
        {
            RemovePath();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.yellow;
        var lastIndex2 = preload ? 0 : 1;
        var index2 = ABTools.DrawPrefixList(preload ? 0 : 1, preloadPopList, GUILayout.Width(80));
        preload = index2 == 0;
        if (lastIndex2 != index2)
        {
            for (int i = 0; i < attrChangeCallbacks.Count; ++i)
                attrChangeCallbacks[i]("preload", preload, abName);
        }

        GUILayout.Space(10);
        var lastIndex3 = assetType;
        assetType = ABTools.DrawPrefixList(assetType, assetTypePopList, GUILayout.Width(110));
        if (lastIndex3 != assetType)
        {
            for (int i = 0; i < attrChangeCallbacks.Count; ++i)
                attrChangeCallbacks[i]("assetType", assetType, abName);
        }
        if (assetType == 1)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(ABLanguage.DOWNLOAD_ORDER, GUILayout.Width(100));
            downloadOrder = Convert.ToInt32(EditorGUILayout.TextField(downloadOrder.ToString(), GUILayout.Width(20)));
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    void DisplaySubFolderPanel()
    {
        if (!File.Exists(abPath))
        {
            if (showFileOnly)
            {
                folders.Clear();
            }
            else
            {
                if (folders.Count == 0) RefreshFolder();
                foreach (var key in folders.Keys)
                {
                    folders[key].DisplayPanel();
                }
            }
            if (files.Count == 0) RefreshFile();
            foreach (var key in files.Keys)
            {
                files[key].DisplayPanel();
            }
        }
    }
    #endregion

    #region Element data Process
    public void RefreshFolder(bool showNewFile = false)
    {
        try
        {
            string[] dirs = Directory.GetDirectories(abPath, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dirs.Length; ++i)
            {
                var path = dirs[i].Replace('\\', '/');
                if (path.Contains("Effect/") && !path.Contains("Effect/Body_effect"))
                    continue;
                if (path.Contains("Resources/"))
                {
                    if (path.Contains("Resources/Font") || path.Contains("Resources/Logo") || path.Contains("Resources/Other"))
                        continue;
                }
                if (path.Contains("Scenes/") && !path.Contains("Scenes/Level"))
                    continue;

                if (!folders.ContainsKey(path))
                {
                    var child = new ABData(this, path, abName, 0, 0, layer + 1, (int)filterType, showFileOnly, preload,
                        showNewFile ? 2 : 0, false, packOrder, downloadOrder, showNewFile);
                    folders.Add(path, child);
                }
                else
                {
                    folders[path].removeFlag = false;
                }
            }
            var temp_dict = folders.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            folders.Clear();
            folders = temp_dict;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void RefreshFile(bool showNewFile = false)
    {
        //if (!File.Exists(abPath)) return;
        string[] temps = Directory.GetFiles(abPath, "*.*", SearchOption.TopDirectoryOnly).
            Where(s => GetFileFilter().Contains(Path.GetExtension(s).ToLower())).ToArray();
        for (int i = 0; i < temps.Length; ++i)
        {
            var path = temps[i].Replace('\\', '/');
            if (!files.ContainsKey(path))
            {
                var abName = this.abName + "/" + Path.GetFileNameWithoutExtension(path).ToLower();
                int assetType = showNewFile ? 2 : 0;
                if (abName.Contains("gui/") || abName.Contains("effect/") || abName.Contains("cutscene/"))
                    assetType = 0;
                else if (abName.Contains("models/") || abName.Contains("scenes/") || abName.Contains("sound/"))
                    assetType = 3;
                var child = new ABData(this, path, abName, 1, 0, layer + 1, (int)filterType, 
                    showFileOnly, preload, assetType, false, packOrder, downloadOrder, showNewFile);
                files.Add(path, child);
            }
            else
            {
                files[path].removeFlag = false;
            }
        }
        var temp_dict = files.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        files.Clear();
        files = temp_dict;
    }

    List<string> GetFileFilter()
    {
        List<string> filters = new List<string>();
        switch (filterType)
        {
            case FileFilterType.Prefab:
                filters.Add(".prefab");
                break;
            case FileFilterType.Picture:
                filters.Add(".jpg");
                filters.Add(".png");
                break;
            case FileFilterType.Scene:
                filters.Add(".unity");
                break;
            case FileFilterType.Material:
                filters.Add(".mat");
                break;
            case FileFilterType.Sound:
                filters.Add(".ogg");
                filters.Add(".wav");
                break;
            case FileFilterType.Bytes:
                filters.Add(".bytes");
                break;
        }
        return filters;
    }

    void RemovePath()
    {
        if (parent != null)
        {
            parent.RemovePath(abPath);
        }
        else
        {
            if (datas.ContainsKey(abPath))
            {
                if (File.Exists(abPath))
                    datas.Remove(abPath);
                else
                    datas[abPath].removeFlag = true;
            }
        }
    }

    public void RemovePath(string abPath)
    {
        if (folders.ContainsKey(abPath))
            folders[abPath].removeFlag = true;
        else if (files.ContainsKey(abPath))
            files.Remove(abPath);
    }

    public void OnAttributesChanged(string attrName, params object[] values)
    {
        if (attrName.Equals("preload") && this.abName == (string)values[1])
        {
            this.preload = (bool)values[0];
        }
        else if (attrName.Equals("packOrder") && this.abName == (string)values[1])
        {
            this.packOrder = (int)values[0];
        }
        else if (attrName.Equals("assetType") && this.abName == (string)values[1])
        {
            this.assetType = (int)values[0];
        }
    }

    public void OneKeyRefresh()
    {
        foreach (var val in folders.Values)
        {
            if (val.removeFlag) continue;

            val.RefreshFolder(true);
            val.RefreshFile(true);
            val.OneKeyRefresh();
        }
    }
    #endregion

    #region Other Tools
    string SearchPathPanel(string title, string folder, string defaultName)
    {
        return EditorUtility.SaveFolderPanel(title, folder, defaultName);
    }

    public void SortData(Dictionary<int, List<ABData>> sortedDatas)
    {
        if (sortedDatas == null)
            sortedDatas = new Dictionary<int, List<ABData>>();

        if (packType == PackType.Seperate)
        {
            foreach (var val in folders.Values)
            {
                val.SortData(sortedDatas);
            }
            foreach (var val in files.Values)
            {
                val.SortData(sortedDatas);
            }
        }
        else
        {
            if (removeFlag) return;
            List<ABData> temp = null;
            if (!sortedDatas.TryGetValue(packOrder, out temp))
            {
                temp = new List<ABData>();
                sortedDatas.Add(packOrder, temp);
            }
            sortedDatas[packOrder].Add(this);
        }
    }

    public void Format(StringBuilder sb)
    {
        if (packType == PackType.Seperate)
        {
            foreach (var val in folders.Values)
            {
                val.Format(sb);
            }
            foreach (var val in files.Values)
            {
                val.Format(sb);
            }
        }
        else
        {
            if (removeFlag) return;
            string resourcsPattern = "Assets/Res/";
            string scenesPattern = "Assets/Scenes/Level/";
            if (dataType == ABDataType.File)
            {
                if (abPath.Contains(resourcsPattern))
                {
                    var assetName = abPath.Replace(resourcsPattern, "");
                    var str = string.Format("{0}|{1}.{2}|{3}", assetName.Split('.')[0].ToLower(), abName, VARIANT_V1, preload ? 1 : 0);
                    sb.Append(str);
                    sb.Append("\n");
                }
                else if (abPath.Contains(scenesPattern))
                {
                    var assetName = abPath.Replace(scenesPattern, "");
                    var str = string.Format("{0}|{1}.{2}|{3}", assetName.Split('.')[0].ToLower(), abName, VARIANT_V1, preload ? 1 : 0);
                    sb.Append(str);
                    sb.Append("\n");
                }
            }
            else
            {
                string[] temps = Directory.GetFiles(abPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => GetFileFilter().Contains(Path.GetExtension(s).ToLower())).ToArray();
                for (int i = 0; i < temps.Length; ++i)
                {
                    var name = temps[i].Replace("\\", "/");
                    if (name.Contains(resourcsPattern))
                    {
                        var assetName = name.Replace(resourcsPattern, "");
                        var str = string.Format("{0}|{1}.{2}|{3}", assetName.Split('.')[0].ToLower(), abName, VARIANT_V1, preload ? 1 : 0);
                        sb.Append(str);
                        sb.Append("\n");
                    }
                    else if (name.Contains(scenesPattern))
                    {
                        var assetName = name.Replace(scenesPattern, "");
                        var str = string.Format("{0}|{1}.{2}|{3}", assetName.Split('.')[0].ToLower(), abName, VARIANT_V1, preload ? 1 : 0);
                        sb.Append(str);
                        sb.Append("\n");
                    }
                }
            }
        }
    }

    public void SetAbName()
    {
        if (packType == PackType.Together)
        {
            if (removeFlag) return;

            if (!string.IsNullOrEmpty(Path.GetExtension(abPath)))
            {
                if (abPath.EndsWith(".unity"))
                {
                    var deps = AssetDatabase.GetDependencies(abPath);
                    foreach (var dep in deps)
                    {
                        if (dep.EndsWith(".cs") || dep.EndsWith(".ttf") || dep.EndsWith(".shader")) continue;
                        if (dep.Contains("Assets/Effect/") || dep.Contains("Assets/EasyWater"))
                        {
                            var effectImporter = AssetImporter.GetAtPath(dep);
                            effectImporter.assetBundleName = "effect/public_effect";
                            effectImporter.assetBundleVariant = VARIANT_V1;
                        }
                    }
                    var importer = AssetImporter.GetAtPath(abPath);
                    if (string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        importer.assetBundleName = abName;
                        importer.assetBundleVariant = VARIANT_V1;
                    }
                }
                else
                {
                    var deps = AssetDatabase.GetDependencies(abPath);
                    foreach (var dep in deps)
                    {
                        if (dep.EndsWith(".cs") || dep.EndsWith(".ttf") || dep.EndsWith(".shader")) continue;
                        var importer = AssetImporter.GetAtPath(dep);
                        if (importer != null)
                        {
                            if (dep.Contains("Assets/Effect/", StringComparison.OrdinalIgnoreCase) 
                                && !importer.assetBundleName.Contains("effect/", StringComparison.OrdinalIgnoreCase))
                            {
                                if (abPath.Contains("Assets/Res/Prefab/Gui", StringComparison.OrdinalIgnoreCase) 
                                    || abPath.Contains("Assets/Res/Prefab/UIEffect", StringComparison.OrdinalIgnoreCase))
                                {
                                    importer.assetBundleName = "effect/gui_effect";
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                                else if (abName.Equals("effect/role_skill", StringComparison.OrdinalIgnoreCase))
                                {
                                    importer.assetBundleName = abName;
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                                else if (abName.Contains("models/", StringComparison.OrdinalIgnoreCase))
                                {
                                    importer.assetBundleName = "effect/model_effect";
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                                else
                                {
                                    importer.assetBundleName = "effect/other_effect";
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                            }
                            else if (string.IsNullOrEmpty(importer.assetBundleName))
                            {
                                importer.assetBundleName = abName;
                                importer.assetBundleVariant = VARIANT_V1;
                            }
                        }
                        else
                            Debug.LogWarning("Cant find Asset! Path is " + dep);
                    }
                }
            }
            else
            {
                if (!Directory.Exists(abPath))
                {
                    removeFlag = true;
                }
                else
                {
                    string[] temps = Directory.GetFiles(abPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => GetFileFilter().Contains(Path.GetExtension(s).ToLower())).ToArray();
                    for (int i = 0; i < temps.Length; ++i)
                    {
                        var deps = AssetDatabase.GetDependencies(temps[i]);
                        foreach (var dep in deps)
                        {
                            if (dep.EndsWith(".cs") || dep.EndsWith(".ttf") || dep.EndsWith(".shader")) continue;
                            var importer = AssetImporter.GetAtPath(dep);
                            if (dep.Contains("Assets/Effect/", StringComparison.OrdinalIgnoreCase)
                                && !importer.assetBundleName.Contains("effect/", StringComparison.OrdinalIgnoreCase))
                            {
                                if (abPath.Contains("Assets/Res/Prefab/Gui", StringComparison.OrdinalIgnoreCase)
                                    || abPath.Contains("Assets/Res/Prefab/UIEffect", StringComparison.OrdinalIgnoreCase))
                                {
                                    importer.assetBundleName = "effect/gui_effect";
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                                else if (abName.Equals("effect/role_skill", StringComparison.OrdinalIgnoreCase))
                                {
                                    importer.assetBundleName = abName;
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                                else if (abName.Contains("models/", StringComparison.OrdinalIgnoreCase))
                                {
                                    importer.assetBundleName = "effect/model_effect";
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                                else
                                {
                                    importer.assetBundleName = "effect/other_effect";
                                    importer.assetBundleVariant = VARIANT_V1;
                                }
                            }
                            else if (string.IsNullOrEmpty(importer.assetBundleName))
                            {
                                importer.assetBundleName = abName;
                                importer.assetBundleVariant = VARIANT_V1;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            foreach (var val in folders.Values)
                val.SetAbName();
            foreach (var val in files.Values)
                val.SetAbName();
        }
    }
    #endregion

    #region Serialize & Deserialize with XML
    public void SerializeToXml(XmlNode node)
    {
        if (node == null)
        {
            Debug.LogError("xml doc is null");
            return;
        }

        var data = node.OwnerDocument.CreateElement("data");
        data.SetAttribute("path", abPath);
        data.SetAttribute("abName", abName);
        data.SetAttribute("assetType", assetType.ToString());
        data.SetAttribute("dataType", ((int)dataType).ToString());
        data.SetAttribute("packType", ((int)packType).ToString());
        data.SetAttribute("filterType", ((int)filterType).ToString());
        data.SetAttribute("layer", layer.ToString());
        data.SetAttribute("packOrder", packOrder.ToString());
        data.SetAttribute("showFileOnly", showFileOnly.ToString());
        data.SetAttribute("preload", preload.ToString());
        data.SetAttribute("removeFlag", removeFlag.ToString());
        data.SetAttribute("downloadOrder", downloadOrder.ToString());
        foreach (var val in folders.Values)
        {
            val.SerializeToXml(data);
        }
        foreach (var val in files.Values)
        {
            val.SerializeToXml(data);
        }
        node.AppendChild(data);
    }

    public void DeserializeFolderAndFile(XmlNode node, ABData parent)
    {
        for (int i = 0; i < node.ChildNodes.Count; ++i)
        {
            var data = DeserializeFromXml(node.ChildNodes[i], parent);
            if (string.IsNullOrEmpty(Path.GetExtension(data.abPath)))
                folders.Add(data.abPath, data);
            else
                files.Add(data.abPath, data);
        }
    }

    public static ABData DeserializeFromXml(XmlNode node, ABData parent)
    {
        var abPath = node.Attributes["path"].Value;
        var abName = node.Attributes["abName"].Value;
        var dataType = Convert.ToInt32(node.Attributes["dataType"].Value);
        var packType = Convert.ToInt32(node.Attributes["packType"].Value);
        var filterType = Convert.ToInt32(node.Attributes["filterType"].Value);
        var layer = Convert.ToInt32(node.Attributes["layer"].Value);
        var packOrder = Convert.ToInt32(node.Attributes["packOrder"].Value);
        var showFileOnly = Convert.ToBoolean(node.Attributes["showFileOnly"].Value);
        var preload = Convert.ToBoolean(node.Attributes["preload"].Value);
        var assetType = Convert.ToInt32(node.Attributes["assetType"].Value);
        var removeFlag = Convert.ToBoolean(node.Attributes["removeFlag"].Value);
        var downloadOrder = Convert.ToInt32(node.Attributes["downloadOrder"].Value);
        ABData data = new ABData(parent, abPath, abName, dataType, packType, layer, filterType, 
            showFileOnly, preload, assetType, removeFlag, packOrder, downloadOrder, false);
        data.DeserializeFolderAndFile(node, data);
        return data;
    }
    #endregion
}
