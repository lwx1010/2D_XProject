using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class SDKConfigItem
{
    public int development { get; private set; }
    public int use_sdk { get; private set; }
    public int need_subpack { get; private set; }
    public string sdk_name { get; private set; }
    public string company_name { get; private set; }
    public string game_name { get; private set; }
    public string protuct_name { get; private set; }
    public string app_icon { get; private set; }
    public string channel { get; private set; }
    public string config_path { get; private set; }
    public string sdk_path { get; private set; }
    public string keystore_name { get; private set; }
    public string apk_server { get; private set; }
    public int update_along { get; private set; }

    public static SDKConfigItem CreateItem(Hashtable jsonTable)
    {
        if (jsonTable != null)
        {
            SDKConfigItem item = new SDKConfigItem();
            item.use_sdk = Convert.ToInt32(jsonTable["use_sdk"]);
            item.development = Convert.ToInt32(jsonTable["development"]);
            item.need_subpack = Convert.ToInt32(jsonTable["subpack"]);
            item.sdk_name = jsonTable["sdk_name"].ToString();
            item.company_name = jsonTable["company_name"].ToString();
            item.game_name = jsonTable["game_name"].ToString();
            item.protuct_name = jsonTable["cn_name"].ToString();
            item.app_icon = jsonTable["app_icon"].ToString();
            item.channel = jsonTable["channel"].ToString();
            item.config_path = jsonTable["config_path"].ToString();
            item.sdk_path = jsonTable["sdk_path"].ToString();
            item.keystore_name = jsonTable["keystore"].ToString();
            if (jsonTable.ContainsKey("update_along")) item.update_along = Convert.ToInt32(jsonTable["update_along"]);
            else item.update_along = 0;
            return item;
        }
        return null;
    }

    public void SetPlayerSetting(string splash_image)
    {
        if (ABPackHelper.GetBuildTarget() == BuildTarget.Android || ABPackHelper.GetBuildTarget() == BuildTarget.iOS)
        {
            PlayerSettings.applicationIdentifier = "com." + company_name + "." + keystore_name.Replace('_', '.');
            PlayerSettings.companyName = company_name;
            PlayerSettings.productName = protuct_name;
            Texture2D splashImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SplashImg/" + splash_image + ".jpg");
            PlayerSettings.virtualRealitySplashScreen = splashImg;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AppIcon/" + app_icon + ".png");
            BuildTargetGroup targetGroup = BuildTargetGroup.Android;
            if (ABPackHelper.GetBuildTarget() == BuildTarget.iOS)
                targetGroup = BuildTargetGroup.iOS;

            PlayerSettings.SetIconsForTargetGroup(targetGroup, new Texture2D[1] { tex });
            if (ABPackHelper.GetBuildTarget() == BuildTarget.Android)
            {
                string keyStorePath = Application.dataPath.Replace("/Assets", "") + "/Public/KeyStore/password.txt";
                var passwords = File.ReadAllLines(keyStorePath);
                string password = string.Empty;
                string aliasName = string.Empty;
                foreach (var pass in passwords)
                {
                    var strs = pass.Split(':');
                    if (strs[0] == keystore_name)
                    {
                        password = strs[1].TrimEnd('\n');
                        aliasName = strs[2].TrimEnd('\n');
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(password))
                {
                    string storeName = Application.dataPath.Replace("/Assets", "") + "/Public/KeyStore/" + keystore_name + ".keystore";
                    PlayerSettings.Android.keystoreName = storeName.Replace("\\", "/");
                    PlayerSettings.Android.keystorePass = password;
                    PlayerSettings.Android.keyaliasName = aliasName;
                    PlayerSettings.Android.keyaliasPass = password;
                }
                else
                {
                    PlayerSettings.Android.keyaliasName = "Unsigned";
                }
            }
            AssetDatabase.Refresh();
        }
    }

    public void CopyConfig()
    {
        string template_path = "Assets/Res/Template/";
        string resources_path = "Assets/Resources/";
        if (File.Exists(resources_path + "config.txt") && !File.Exists(resources_path + "config1.tmp"))
            File.WriteAllText(resources_path + "config1.tmp", File.ReadAllText(resources_path + "config.txt"));
        File.Copy(template_path + config_path, resources_path + "config.txt", true);
        //if (use_sdk)
        //{
        //    File.Copy(template_path + config_path, resources_path + "config.txt", true);
        //}
        //else
        //{
        //    File.Copy(template_path + out_path, resources_path + "config.txt", true);
        //}
        var lines = File.ReadAllLines(resources_path + "config.txt");
        StringBuilder builder = new StringBuilder();
        foreach (var line in lines)
        {
            if (line.Contains("logo_name="))
            {
                builder.Append("logo_name=" + game_name);
            }
            else if (use_sdk == 1 && line.Contains("resource_server="))
            {
                var str = line;
                str.TrimEnd('\n');
                str += string.Format("update/{0}/{1}/", game_name, channel);
                builder.Append(str);
            }
            else if (line.Contains("apk_server="))
            {
                var str = line;
                str.TrimEnd('\n');
                str += string.Format("{0}/{1}/", game_name, channel);
                apk_server = str.Split('=')[1];
                builder.Append(str);
            }
            else
            {
                builder.Append(line);
            }
            builder.Append("\n");
        }

        File.WriteAllText(resources_path + "config.txt", builder.ToString());
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

    public void CopySDK()
    {
        try
        {
            string fromPath = Application.dataPath.Replace("/Assets", "") +"/Public/" + sdk_path;
            string toPath = Application.dataPath + "/Plugins/Android/";
            if (Directory.Exists(toPath)) Directory.Delete(toPath, true);
            Directory.CreateDirectory(toPath);
            string[] files = Directory.GetFiles(fromPath, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                var dest = files[i].Replace("\\", "/").Replace(fromPath + "/", "");
                var dirs = dest.Split('/');
                string path = toPath;
                ABPackHelper.ShowProgress("copy sdk: " + dest, (float)i / (float)files.Length);
                foreach (var dir in dirs)
                {
                    if (dir.Contains(".")) continue;
                    path += "/" + dir;
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                }
                File.Copy(files[i], toPath + "/" + dest, true);
            }
            ABPackHelper.ShowProgress("finished...", 1);
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorUtility.ClearProgressBar();
        }
    }

    public void ClearSDK()
    {
        string path = Application.dataPath + "/Plugins/Android/";
        if (Directory.Exists(path)) Directory.Delete(path, true);
        Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }

    public void SaveSDKConfig()
    {
        string rootPath = Application.streamingAssetsPath + "/" + LuaConst.osDir;
        if (!Directory.Exists(rootPath)) return;
        string path = rootPath + "/sdkname";
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, sdk_name);
        AssetDatabase.Refresh();
    }

    public void SplitAssets(bool split)
    {
        if (split && Directory.Exists(Application.streamingAssetsPath))
        {
            var root = Application.streamingAssetsPath + "/" + LuaConst.osDir;
            string[] files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
            string[] filters = new string[] { "android.ab", "bundlemap.ab", "files.txt", "sdkname", "version.txt" };
            for (int j = 0; j < files.Length; ++j)
            {
                var file = files[j];
                bool delete = true;
                for (int k = 0; k < filters.Length; ++k)
                {
                    if (Path.GetFileName(file).Equals(filters[k], StringComparison.OrdinalIgnoreCase))
                    {
                        delete = false;
                        break;
                    }
                }
                if (delete)
                {
                    if (File.Exists(file)) File.Delete(file);
                }
            }
            var split_file = root + "/split.txt";
            if (File.Exists(split_file)) File.Delete(split_file);
            File.WriteAllText(split_file, "1");
            AssetDatabase.Refresh();
        }
    }
}

public class SDKConfig
{
    public class uploadItem
    {
        public string path { get; set; }
        public string script { get; set; }
    }
    public string show_name { get; private set; }
    public int uploadCDN { get; private set; }
    public int upload243 { get; private set; }
    public List<SDKConfigItem> items { get; private set; }
    public List<uploadItem> uploadPathes { get; private set; }
    public string splash_image { get; private set; }
    public bool split_assets { get; private set; } 

    public static SDKConfig LoadSDKConfig(string json)
    {
        object jsonParsed = MiniJSON.Json.Deserialize(json);
        if (jsonParsed != null)
        {
            Hashtable jsonMap = jsonParsed as Hashtable;
            SDKConfig config = new SDKConfig();
            config.show_name = jsonMap["editor_name"].ToString();
            config.uploadCDN = Convert.ToInt32(jsonMap["uploadCDN"]);
            config.upload243 = Convert.ToInt32(jsonMap["upload243"]);
            config.splash_image = jsonMap["splash_img"].ToString();
            config.split_assets = Convert.ToInt32(jsonMap["split_assets"]) == 1;
            ArrayList pathes = jsonMap["upload_pathes"] as ArrayList;
            if (config.uploadPathes == null) config.uploadPathes = new List<uploadItem>();
            foreach (var item in pathes)
            {
                Hashtable table = item as Hashtable;
                uploadItem temp = new uploadItem();
                temp.path = table["path"].ToString();
                temp.script = table["upload_script"].ToString();
                config.uploadPathes.Add(temp);
            }
            ArrayList items = jsonMap["items"] as ArrayList;
            if (config.items == null) config.items = new List<SDKConfigItem>();
            foreach (var item in items)
            {
                config.items.Add(SDKConfigItem.CreateItem(item as Hashtable));
            }
            return config;
        }
        return null;
    }
}
