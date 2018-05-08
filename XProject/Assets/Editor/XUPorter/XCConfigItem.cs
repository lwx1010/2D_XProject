using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.XCodeEditor;
using System.IO;
using System;
using System.Text;
using UnityEditor;

public class XCConfigItem
{
    class CodeEditItem
    {
        public enum EditType
        {
            WriteBelow,
            Replace,
        }
        public EditType type { get; set; }
        public string key { get; set; }
        public string value { get; set; }

        public void EditCode(XClass code)
        {
            if (type == EditType.WriteBelow)
            {
                code.WriteBelow(key, value);
            }
            else if (type == EditType.Replace)
            {
                code.Replace(key, value);
            }
        }
    }
    private string channel { get; set; }
    private string game_name { get; set; }
    private string code_sign { get; set; }
    private string dev_team { get; set; }
    private string provision_profile { get; set; }
    private string external_call { get; set; }
    public string bundleIdentifier { get; private set; }
    public string product_name { get; private set; }
    public string app_icon { get; private set; }

    private List<string> includes = new List<string>();

    private List<CodeEditItem> codeEditList = new List<CodeEditItem>();

    public static XCConfigItem ParseXCConfig(string path)
    {
        if (!path.CustomEndsWith(".json")) path += ".json";
        if (!File.Exists(path)) return null;
        Hashtable datastore = (Hashtable)XUPorterJSON.MiniJSON.jsonDecode(File.ReadAllText(path));
        XCConfigItem item = new XCConfigItem();
        ArrayList includes = datastore["includes"] as ArrayList;
        for (int i = 0; i < includes.Count; ++i)
        {
            item.includes.Add(includes[i].ToString());
        }
        item.channel = datastore["channel"].ToString();
        item.game_name = datastore["game_name"].ToString();
        item.code_sign = datastore["code_sign"].ToString();
        item.provision_profile = datastore["provision_profile"].ToString();
        item.dev_team = datastore["dev_team"].ToString();
        item.external_call = datastore["external_call"].ToString();
        item.bundleIdentifier = datastore["bundle_identifier"].ToString();
        item.product_name = datastore["product_name"].ToString();
        item.app_icon = datastore["app_icon"].ToString();
        ArrayList code_edit_items = datastore["code_edit"] as ArrayList;
        for (int i = 0; i < code_edit_items.Count; ++i)
        {
            CodeEditItem editItem = new CodeEditItem();
            Hashtable config = code_edit_items[i] as Hashtable;
            if (config != null)
            {
                string type = config["type"].ToString();
                if (type == "write")
                    editItem.type = CodeEditItem.EditType.WriteBelow;
                else if (type == "replace")
                    editItem.type = CodeEditItem.EditType.Replace;
                editItem.key = config["key"].ToString();
                ArrayList values = config["value"] as ArrayList;
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < values.Count; ++j)
                {
                    sb.Append(values[j].ToString());
                }
                editItem.value = sb.ToString();
            }
            item.codeEditList.Add(editItem);
        }
        return item;
    }

    private void EditCode(string filePath)
    {
        XClass UnityAppController = new XClass(filePath + "/Classes/UnityAppController.mm");
        for (int i = 0; i < codeEditList.Count; ++i)
        {
            codeEditList[i].EditCode(UnityAppController);
        }
    }

    private void CopySDK(string pathToBuiltProject, string copy_folder)
    {
        string path = Application.dataPath + "/Editor/XUPorter/Mods/" + copy_folder;
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        var root = pathToBuiltProject;
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);

        for (int i = 0; i < files.Length; ++i)
        {
            var file = files[i];
            if (file.CustomEndsWith(".meta")) continue;
            var tempStr = file.Replace(path, "").Replace("\\", "/").TrimStart('/');
            var newPath = Path.Combine(root, tempStr);
            string[] temps = tempStr.Split('/');
            string dir = root;
            for (int j = 0; j < temps.Length - 1; ++j)
            {
                dir += "/" + temps[j];
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            }

            File.Copy(file, newPath, true);
        }
    }

    public void EditProject(string pathToBuiltProject)
    {
        string path = Path.GetFullPath(pathToBuiltProject);
        // Create a new project object from build target
        XCProject project = new XCProject(pathToBuiltProject);

        // Find and run through all projmods files to patch the project.
        // Please pay attention that ALL projmods files in your project folder will be excuted!
        string[] files = Directory.GetFiles(Application.dataPath, "*.projmods", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            UnityEngine.Debug.Log("ProjMod File: " + file);
            if (!isFilter(file))
            {
                CopySDK(pathToBuiltProject, Path.GetFileNameWithoutExtension(file));
                project.ApplyMod(file);
            }
        }

        //TODO disable the bitcode for iOS 9
        project.overwriteBuildSetting("ENABLE_BITCODE", "NO", "Release");
        project.overwriteBuildSetting("ENABLE_BITCODE", "NO", "Debug");

        //TODO implement generic settings as a module option
        project.overwriteBuildSetting("CODE_SIGN_IDENTITY", code_sign, "Release");
        project.overwriteBuildSetting("CODE_SIGN_IDENTITY", code_sign, "Debug");
        project.overwriteBuildSetting("CODE_SIGN_STYLE", "Manual");
        project.overwriteBuildSetting("DEVELOPMENT_TEAM", dev_team);
        project.overwriteBuildSetting("PROVISIONING_PROFILE_SPECIFIER", provision_profile, "Debug");
        project.overwriteBuildSetting("PROVISIONING_PROFILE_SPECIFIER", provision_profile, "Release");

        // 编辑生成代码
        EditCode(path);

        CopyIcons(pathToBuiltProject);
        // Finally save the xcode project
        project.Save();
    }

    public void CopyConfig()
    {
        string resources_path = "Assets/Resources/config.txt";
        var lines = File.ReadAllLines(resources_path);
        StringBuilder builder = new StringBuilder();
        foreach (var line in lines)
        {
            if (line.Contains("logo_name="))
            {
                builder.Append("logo_name=" + game_name);
            }
            else if (line.Contains("resource_server="))
            {
                var str = line;
                str.TrimEnd('\n');
                str += string.Format("update/{0}/{1}/", game_name, channel);
                builder.Append(str);
            }
            else
            {
                builder.Append(line);
            }
            builder.Append("\n");
        }

        File.WriteAllText(resources_path, builder.ToString());
        AssetDatabase.Refresh();

        // 删除logo下不需要的图片
        var files = Directory.GetFiles("Assets/Resources/Logo", "*.png", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (!fileName.Equals(game_name))
                AssetDatabase.DeleteAsset(file.Replace("\\", "/"));
        }
        AssetDatabase.Refresh();
    }

    private bool isFilter(string file)
    {
        string fileName = Path.GetFileNameWithoutExtension(file);
        for (int i = 0; i < includes.Count; ++i)
        {
            if (fileName.Equals(includes[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    private void CopyIcons(string pathToBuiltProject)
    {
        string appIconPath = Application.dataPath + "/Editor/XUPorter/AppIcon/" + game_name + "/";
        string[] iconflies = null;
        if (Directory.Exists(appIconPath))
        {
            iconflies = Directory.GetFiles(appIconPath);
        }
        if (iconflies != null && iconflies.Length > 0)
        {
            appIconPath = pathToBuiltProject + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/";
            string[] icons = Directory.GetFiles(appIconPath);
            foreach (string file in icons)
            {
                Debug.Log("Delete icon:" + file);
                File.Delete(file);
            }
            foreach (string file in iconflies)
            {
                string fileName = file.Substring(file.LastIndexOf("/") + 1);
                if (fileName.CustomEndsWith(".meta")) continue;
                Debug.Log("Icon Name:" + fileName);
                File.Copy(file, Path.Combine(appIconPath, fileName));
            }
        }
    }
}
