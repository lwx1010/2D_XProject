using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using LuaFramework;
using AL.Crypto;

public class Packager {
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext) {   //能不能复制
        foreach (string e in exts) {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    public static string[] Filters = new[] { "print(", "Debug.Log(", "UnityEngine.Debug.Log(" ,
                                             "Debugger.Log(", "log(" };

    /// <summary>
    /// 载入素材
    /// </summary>
    static UnityEngine.Object LoadAsset(string file) {
        if (file.EndsWith(".lua")) file += ".txt";
        return AssetDatabase.LoadMainAssetAtPath("Assets/LuaFramework/Examples/Builds/" + file);
    }

    [MenuItem("LuaFramework/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource() {
        BuildTarget target;
#if UNITY_5
        target = BuildTarget.iOS;
#else
        target = BuildTarget.iPhone;
#endif
        BuildAssetResource(target);
    }

//    [MenuItem("LuaFramework/Build Android Resource", false, 101)]
    public static void BuildAndroidResource() {
        BuildAssetResource(BuildTarget.Android);
    }

//    [MenuItem("LuaFramework/Build Windows Resource", false, 102)]
    public static void BuildWindowsResource() {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target) {
        //if (Util.DataPath.Equals(Application.dataPath + "/"))
        //{
        //    UnityEngine.Debug.LogError("请先在AppConst中设置luabundle模式为true");
        //    return;
        //}
        //if (Directory.Exists(Util.DataPath)) {
        //    Directory.Delete(Util.DataPath, true);
        //}
        //string streamPath = Application.streamingAssetsPath;
        //if (Directory.Exists(streamPath)) {
        //    Directory.Delete(streamPath, true);
        //}
        //AssetDatabase.Refresh();

#if UNITY_IOS
        HandleBundle();
        BuildFileIndex();
#else
        ReadLuaMd5FromFile();
        HandleBundle();
        CompareLuaMd5();
        CopyNoChangeLuaFiles();
        RecordLuaMd5();
        BuildFileIndex();
#endif
        AssetDatabase.Refresh();
    }

    static void HandleBundle() {
        BuildLuaBundles();
        string luaPath = AppDataPath + "/AssetBundle/" + LuaConst.osDir + "/lua/";
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua/", 
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++) {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            foreach (string f in files) {
                var cmpStr = f.ToLower();
                if (cmpStr.Contains("protocol/"))
                {
                    if (cmpStr.EndsWith(".meta") || cmpStr.EndsWith(".lua")) continue;
                    string newfile = f.Replace(luaDataPath, "");
                    string path = Path.GetDirectoryName(luaPath + newfile);
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    string destfile = path + "/" + Path.GetFileName(f);
                    File.Copy(f, destfile, true);
                }
            }
        }
    }

    static void ClearAllLuaFiles() {
        string osPath = ABPackHelper.BUILD_PATH + LuaConst.osDir;

        if (Directory.Exists(osPath)) {
            string[] files = Directory.GetFiles(osPath, "Lua*.unity3d");

            for (int i = 0; i < files.Length; i++) {
                File.Delete(files[i]);
            }
        }

        string path = osPath + "/Lua";

        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }

        path = Application.dataPath + "/Resources/Lua";

        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }

        path = Application.persistentDataPath + "/" + LuaConst.osDir + "/Lua";

        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
    }

    static void CreateStreamDir(string dir) {
        dir = ABPackHelper.BUILD_PATH + dir;

        if (!File.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
    }

    static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true) {
        if (!Directory.Exists(sourceDir)) {
            return;
        }

        string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
        int len = sourceDir.Length;

        if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\') {
            --len;
        }

        for (int i = 0; i < files.Length; i++) {
            string str = files[i].Remove(0, len);
            string dest = destDir + str;
            if (appendext) dest += ".bytes";
            string dir = Path.GetDirectoryName(dest);
            Directory.CreateDirectory(dir);

            if (AppConst.LuaByteMode) {
                Packager.EncodeLuaFile(files[i], dest);
            } else {
                var srcFile = files[i];
                if (srcFile.EndsWith(".lua"))
                    srcFile = RemoveLogInLua(srcFile, dest);
                File.Copy(srcFile, dest, true);
            }
        }
    }

    static void BuildLuaBundles() {
        ClearAllLuaFiles();
        CreateStreamDir(LuaConst.osDir + "/lua/");

        string dir = Application.persistentDataPath;
        if (!File.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }

        string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
        CopyLuaBytesFiles(CustomSettings.luaDir, streamDir);
        CopyLuaBytesFiles(CustomSettings.toluaLuaDir, streamDir);

        var currDir = Application.dataPath.Replace("Assets", "");
        Directory.SetCurrentDirectory(currDir);
        AssetDatabase.Refresh();
        string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);

        for (int i = 0; i < dirs.Length; i++) {
            string str = dirs[i].Remove(0, streamDir.Length);
            BuildLuaBundle(str);
        }

        BuildLuaBundle(null);
        Directory.Delete(streamDir, true);
        AssetDatabase.Refresh();
    }

    static void BuildLuaBundle(string dir) {
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DisableWriteTypeTree | 
            BuildAssetBundleOptions.DeterministicAssetBundle | 
            BuildAssetBundleOptions.UncompressedAssetBundle;
        string path = "Assets/" + AppConst.LuaTempDir + dir;
        string[] files = Directory.GetFiles(path, "*.lua.bytes");
        List<Object> list = new List<Object>();
        string bundleName = "lua.unity3d";
        if (dir != null) {
            dir = dir.Replace('\\', '_').Replace('/', '_');
            bundleName = "lua_" + dir.ToLower() + AppConst.ExtName;
        }
        for (int i = 0; i < files.Length; i++) {
            Object obj = AssetDatabase.LoadMainAssetAtPath(files[i]);
            list.Add(obj);
        }

        if (files.Length > 0) {
            string output = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/lua/" + bundleName;
            if (File.Exists(output)) {
                File.Delete(output);
            }
            if (BuildPipeline.BuildAssetBundle(null, list.ToArray(), output, options, EditorUserBuildSettings.activeBuildTarget)
                && !AppConst.LuaByteMode)
            {
                byte[] bytes = File.ReadAllBytes(output);
                var buffer = Crypto.Encode(bytes);
                File.Delete(output);
                File.WriteAllBytes(output, buffer);
            }
            AssetDatabase.Refresh();
        }
    }

    static void HandleExampleBundle(BuildTarget target) {
        Object mainAsset = null;        //主素材名，单个
        Object[] addis = null;     //附加素材名，多个
        string assetfile = string.Empty;  //素材文件名

        BuildAssetBundleOptions options = BuildAssetBundleOptions.UncompressedAssetBundle |
                                          BuildAssetBundleOptions.CollectDependencies |
                                          BuildAssetBundleOptions.DeterministicAssetBundle;
        string dataPath = Util.DataPath;
        if (Directory.Exists(dataPath)) {
            Directory.Delete(dataPath, true);
        }
        string assetPath = AppDataPath + "/StreamingAssets/";
        if (Directory.Exists(dataPath)) {
            Directory.Delete(assetPath, true);
        }
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        ///-----------------------------生成共享的关联性素材绑定-------------------------------------
        BuildPipeline.PushAssetDependencies();

        assetfile = assetPath + "shared" + AppConst.ExtName;
        mainAsset = LoadAsset("Shared/Atlas/Dialog.prefab");
        BuildPipeline.BuildAssetBundle(mainAsset, null, assetfile, options, target);

        ///------------------------------生成PromptPanel素材绑定-----------------------------------
        BuildPipeline.PushAssetDependencies();
        mainAsset = LoadAsset("Prompt/Prefabs/PromptPanel.prefab");
        addis = new Object[1];
        addis[0] = LoadAsset("Prompt/Prefabs/PromptItem.prefab");
        assetfile = assetPath + "prompt" + AppConst.ExtName;
        BuildPipeline.BuildAssetBundle(mainAsset, addis, assetfile, options, target);
        BuildPipeline.PopAssetDependencies();

        ///------------------------------生成MessagePanel素材绑定-----------------------------------
        BuildPipeline.PushAssetDependencies();
        mainAsset = LoadAsset("Message/Prefabs/MessagePanel.prefab");
        assetfile = assetPath + "message" + AppConst.ExtName;
        BuildPipeline.BuildAssetBundle(mainAsset, null, assetfile, options, target);
        BuildPipeline.PopAssetDependencies();

        ///-------------------------------刷新---------------------------------------
        BuildPipeline.PopAssetDependencies();
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile() {
        string luaPath = AppDataPath + "/StreamingAssets/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath)) {
            Directory.CreateDirectory(luaPath); 
        }
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua/", 
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++) {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files) {
                if (f.EndsWith(".meta")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath)) {
                    File.Delete(newpath);
                }
                if (AppConst.LuaByteMode) {
                    EncodeLuaFile(f, newpath);
                } else {
                    var srcFile = f;
                    if (f.EndsWith(".lua"))
                        srcFile = RemoveLogInLua(srcFile, newpath);
                    File.Copy(srcFile, newpath, true);
                }
                UpdateProgress(n++, files.Count, newpath);
            } 
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static void BuildFileIndex() {
        string resPath = AppDataPath + "/AssetBundle/" + LuaConst.osDir + "/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        string tempSizeFile = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/tempsizefile.txt";
        Dictionary<string, string> assetTypeDict = new Dictionary<string, string>();
        if (File.Exists(tempSizeFile))
        {
            var sizeFileContent = File.ReadAllText(tempSizeFile);
            var temps = sizeFileContent.Split('\n');
            for (int i = 0; i < temps.Length; ++i)
            {
                if (!string.IsNullOrEmpty(temps[i]))
                {
                    var temp = temps[i].Split('|');
                    if (temp.Length != 2 && temp.Length != 3) throw new System.IndexOutOfRangeException();
                    var assetType = temp[1];
                    if (temp.Length == 3) assetType += "|" + temp[2];
                    assetTypeDict.Add(temp[0], assetType);
                    UpdateProgress(i, temps.Length, temps[i]);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++) {
            string file = files[i];
            //string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store") || file.EndsWith("apk_version.txt")) continue;
            if (file.EndsWith(".manifest") || file.Contains("tempsizefile.txt") || file.Contains("luamd5.txt")) continue;

            string md5 = MD5.ComputeHashString(file);
            int size = File.ReadAllBytes(file).Length;
            string value = file.Replace(resPath, string.Empty).ToLower();
            if (assetTypeDict.ContainsKey(value))
                sw.WriteLine(value + "|" + md5 + "|" + size + "|" + assetTypeDict[value]);
            else
                sw.WriteLine(value + "|" + md5 + "|" + size);
            UpdateProgress(i, files.Count, file);

        }
        sw.Close(); fs.Close();
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path) {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names) {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs) {
            if (dir.Contains(".svn")) continue;
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc) {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    public static void EncodeLuaFile(string srcFile, string outFile) {
        if (!srcFile.ToLower().EndsWith(".lua")) {
            File.Copy(srcFile, outFile, true);
            return;
        }
        bool isWin = true; 
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        srcFile = RemoveLogInLua(srcFile, outFile);
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            isWin = false;
            luaexe = "./luajit";
            args = "-b " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit_mac/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    static string RemoveLogInLua(string srcFile, string outFile)
    {
        var newFile = outFile + ".tmp.lua";
        File.Copy(srcFile, newFile, true);
        var lines = File.ReadAllLines(newFile);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < lines.Length; ++i)
        {
            if (isFilter(lines[i])) continue;
            sb.AppendLine(lines[i]);
        }
        if (File.Exists(newFile)) File.Delete(newFile);
        File.WriteAllText(newFile, sb.ToString());
        return newFile;
    }

    protected static bool isFilter(string file)
    {
        string format = file.Trim();
        for (int i = 0; i < Filters.Length; i++)
        {
            if (format.StartsWith(Filters[i]))
                return true;
        }
        return false;
    }

//    [MenuItem("Lua/Build Protobuf-lua-gen File")]
    public static void BuildProtobufFile() {
        
        string dir = AppDataPath + "/Lua/3rd/pblua";
        paths.Clear(); files.Clear(); Recursive(dir);

        string protoc = "d:/protobuf-2.4.1/src/protoc.exe";
        string protoc_gen_dir = "\"d:/protoc-gen-lua/plugin/protoc-gen-lua.bat\"";

        foreach (string f in files) {
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f);
            if (!ext.Equals(".proto")) continue;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            Util.Log(info.FileName + " " + info.Arguments);

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }
        AssetDatabase.Refresh();
    }

#region Lua增量更新

    static Dictionary<string, Dictionary<string, string>> luaMd5Dict = new Dictionary<string, Dictionary<string, string>>();
    static HashSet<string> copyLuaFiles = new HashSet<string>();

    /// <summary>
    /// 读取lua md5配置文件
    /// </summary>
    static void ReadLuaMd5FromFile()
    {
        luaMd5Dict.Clear();
        string luaMd5File = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/luamd5.txt";
        if (File.Exists(luaMd5File))
        {
            string[] lines = File.ReadAllLines(luaMd5File);
            for (int i = 0; i < lines.Length; ++i)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    var temps = lines[i].ToLower().Split('|');
                    if (!luaMd5Dict.ContainsKey(temps[0]))
                    {
                        luaMd5Dict.Add(temps[0], new Dictionary<string, string>());
                    }
                    luaMd5Dict[temps[0]].Add(temps[1], temps[2]);
                }
                UpdateProgress(i, lines.Length, "Getting lua md5...");
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 比较文件夹下lua更新情况，用来做lua的增量更新
    /// </summary>
    static void CompareLuaMd5()
    {
        copyLuaFiles.Clear();
        int index = 0;
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua",
                              AppDataPath + "/LuaFramework/Tolua/Lua" };
        foreach (var luaMd5 in luaMd5Dict)
        {
            var key = luaMd5.Key.Replace('-', '/');
            string dir;
            if (key.EndsWith("/lua"))
                dir = AppDataPath + "/" + key.Substring(0, key.LastIndexOf("/"));
            else
                dir = AppDataPath + "/" + key;
            if (!Directory.Exists(dir)) continue;
            string[] files = Directory.GetFiles(dir, "*.lua", SearchOption.TopDirectoryOnly);
            bool needCopy = true;
            // 如果文件夹下数量有变 说明有更改
            if (luaMd5.Value.Count == files.Length)
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    var cmpStr = files[i].ToLower().Replace("\\", "/");
                    string newfile = cmpStr.Replace(AppDataPath + "/", "");
                    var curMd5 = MD5.ComputeHashString(files[i]);
                    // 如果文件夹下有新增lua文件 说明有更改
                    if (!luaMd5.Value.ContainsKey(newfile))
                        needCopy = false;
                    // 如果lua文件md5值有变动 说明有更改
                    else if (luaMd5.Value[newfile] != curMd5)
                        needCopy = false;
                    if (!needCopy) break;
                }
            }
            else
            {
                needCopy = false;
            }
            // 文件夹下没有更改的资源，将新的lua包替换回上一个版本
            if (needCopy)
            {
                for (int i = 0; i < luaPaths.Length; ++i)
                {
                    var temp = luaPaths[i].ToLower();
                    if (dir.Contains(temp))
                    {
                        var copyFile = dir.Replace(temp, "").TrimEnd('/');
                        var tempKey = "lua" + copyFile.Replace("/", "_") + AppConst.ExtName;
                        if (!copyLuaFiles.Contains(tempKey))
                            copyLuaFiles.Add(tempKey);
                        break;
                    }
                }
            }
                
            UpdateProgress(index++, luaMd5Dict.Count, "Comparing lua md5...");
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 记录lua的md5码
    /// </summary>
    static void RecordLuaMd5()
    {
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua/",
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            string temp = luaPaths[i].Replace(AppDataPath + "/", "").Replace("/", "-").TrimEnd('-').ToLower();
            Recursive(luaDataPath);
            foreach (string f in files)
            {
                var cmpStr = f.ToLower();
                if (cmpStr.EndsWith(".lua"))
                {
                    string newfile = f.Replace(AppDataPath + "/", "");
                    string luaBundleFile = f.Replace(luaDataPath, "");
                    string key;
                    if (!luaBundleFile.Contains("/"))
                    {
                        key = "lua";
                    }
                    else
                    {
                        key = luaBundleFile.Substring(0, luaBundleFile.LastIndexOf('/'));
                        key = key.Replace('/', '-').TrimEnd('-');
                    }
                    key = temp + "-" + key.ToLower(); ;
                    string md5 = MD5.ComputeHashString(cmpStr);
                    sb.Append(key);
                    sb.Append("|");
                    sb.Append(newfile);
                    sb.Append("|");
                    sb.Append(md5);
                    sb.Append("\n");
                }
            }
            UpdateProgress(i, luaPaths.Length, "Recording lua md5...");
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        string luaMd5File = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/luamd5.txt";
        if (File.Exists(luaMd5File)) File.Delete(luaMd5File);
        File.WriteAllText(luaMd5File, sb.ToString());
    }

    /// <summary>
    /// 从上一版本替换不需要增量更新的lua bundle文件
    /// </summary>
    static void CopyNoChangeLuaFiles()
    {
        if (copyLuaFiles.Count > 0)
        {
            int index = 0;
            foreach (var file in copyLuaFiles)
            {
                string fromPath = ABPackHelper.TEMP_ASSET_PATH + LuaConst.osDir + "/lua/" + file;
                string toPath = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/lua/" + file;
                if (File.Exists(fromPath)) File.Copy(fromPath, toPath, true);
                UpdateProgress(index++, luaMd5Dict.Count, "Copy no change files...");
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

#endregion

    [MenuItem("Lua/Encoding Lua files into no BOM UTF8", false, 105)]
    static void EncodingLua2UTF8()
    {
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua/",
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int index = 0;
            foreach (string f in files)
            {
                var file = f.ToLower();
                if (file.EndsWith(".lua"))
                {
                    var content = File.ReadAllText(file);
                    UTF8Encoding utf8 = new UTF8Encoding(false);
                    File.Delete(file);
                    File.WriteAllText(file, content, utf8);
                }
                UpdateProgress(index++, files.Count, "Encoding to utf8 with no bom...");
            }
            EditorUtility.ClearProgressBar();
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
}