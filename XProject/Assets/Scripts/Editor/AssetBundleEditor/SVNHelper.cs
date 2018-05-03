using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;

public class SVNHelper
{
    private const string TORTOISEPROC_NAME = "TortoiseProc.exe";
    private const string PYTHONPROC_NAME = "python";


    public static void UpdateVersion()
    {
        string args = string.Format("/command:update /path:{0}", ABPackHelper.VERSION_PATH);
        Process p = Process.Start(TORTOISEPROC_NAME, args);
        p.WaitForExit();
        AssetDatabase.Refresh();
    }

    public static void UpdateAll()
    {
        string args = string.Format("/command:update /path:{0}", Application.dataPath);
        Process p = Process.Start(TORTOISEPROC_NAME, args);
        p.WaitForExit();
        AssetDatabase.Refresh();
    }

    public static void UpdateTempAssets()
    {
        string path = ABPackHelper.TEMP_ASSET_PATH + LuaConst.osDir;
        if (Directory.Exists(path))
        {
            string args = string.Format("/command:update /path:{0}", ABPackHelper.TEMP_ASSET_PATH + LuaConst.osDir);
            Process p = Process.Start(TORTOISEPROC_NAME, args);
            p.WaitForExit();
            AssetDatabase.Refresh();
        }
    }

    public static void UpdateAssets()
    {
        string path = ABPackHelper.ASSET_PATH + LuaConst.osDir;
        string args = string.Format("/command:update /path:{0}", path);
        Process p = Process.Start(TORTOISEPROC_NAME, args);
        p.WaitForExit();
        AssetDatabase.Refresh();

        var destVersionPath = ABPackHelper.VERSION_PATH + "version.txt";
        if (!File.Exists(destVersionPath)) destVersionPath = ABPackHelper.ASSET_PATH + LuaConst.osDir + "/version.txt";
        var versionPath = ABPackHelper.BUILD_PATH + LuaConst.osDir + "/version.txt";
        File.Copy(destVersionPath, versionPath, true);
        if (File.Exists(versionPath) && AssetBundleEditor.gameVersion == null)
            AssetBundleEditor.gameVersion = GameVersion.CreateVersion(File.ReadAllText(versionPath));
        AssetDatabase.Refresh();
    }

    public static void CommitPackAssets()
    {
        try
        {
            string path = ABPackHelper.ASSET_PATH + LuaConst.osDir;
            var args = string.Format("/command:commit /path:{0} /logmsg:提交打包资源", path);
            var p = Process.Start(TORTOISEPROC_NAME, args);
            p.WaitForExit();

            path = ABPackHelper.TEMP_ASSET_PATH + LuaConst.osDir;
            args = string.Format("/command:commit /path:{0} /logmsg:提交临时打包资源", path);
            p = Process.Start(TORTOISEPROC_NAME, args);
            p.WaitForExit();

            args = string.Format("/command:commit /path:{0} /logmsg:资源整理", Application.dataPath);
            p = Process.Start(TORTOISEPROC_NAME, args);
            p.WaitForExit();

            args = string.Format("/command:commit /path:{0} /logmsg:版本号更新", ABPackHelper.VERSION_PATH);
            p = Process.Start(TORTOISEPROC_NAME, args);
            p.WaitForExit();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("错误", "资源上传svn错误: " + e.Message, "OK");
        }
        finally
        {
            AssetDatabase.Refresh();
        }
    }

    public static void CommitVersion()
    {
        var args = string.Format("/command:commit /path:{0} /logmsg:版本号更新", ABPackHelper.VERSION_PATH);
        var p = Process.Start(TORTOISEPROC_NAME, args);
        p.WaitForExit();
    }

    public static void UpdateCompressedData()
    {
        string args = string.Format("/command:update /path:{0}", ABPackHelper.TEMP_ASSET_PATH + "IOSCompress");
        Process p = Process.Start(TORTOISEPROC_NAME, args);
        p.WaitForExit();
        AssetDatabase.Refresh();
    }

    public static void CommitIOSCompressedData()
    {
        if (ABPackHelper.GetBuildTarget() != BuildTarget.iOS)
            return;

        try
        {
            string path = ABPackHelper.TEMP_ASSET_PATH + "/IOSCompress/";
            var args = string.Format("/command:commit /path:{0} /logmsg:提交ios打包压缩资源", path);
            var p = Process.Start(TORTOISEPROC_NAME, args);
            p.WaitForExit();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("错误", "资源上传svn错误: " + e.Message, "OK");
        }
        finally
        {
            AssetDatabase.Refresh();
        }
    }

    public static void UpdateTo243()
    {
        string script_path = Application.dataPath.Replace("/Assets", "") + "/rsync_243_res.py";
        string args = string.Format(" {0}", script_path);
        Process p = Process.Start(PYTHONPROC_NAME, args);
        p.WaitForExit();
        AssetDatabase.Refresh();
    }

    public static void UpdateToCDN(string path, string version, string upload_script)
    {
        if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(version))
        {
            string script_path = Application.dataPath.Replace("/Assets", "") + "/" + upload_script;
            string upload_path = string.Format("update/{0}/{1}", path, version);
            //var lastVersion = GameVersion.CreateVersion(version);
            //lastVersion.VersionDecrease();
            //string last_upload_path = string.Format("update/{0}/{1}/{2}", app_name, channel_name, lastVersion.ToString());
            string local_path = (ABPackHelper.ASSET_PATH + LuaConst.osDir).Replace("\\", "/");
            string args = string.Format(" {0} {1} {2} android", script_path, upload_path, local_path);
            Process p = Process.Start(PYTHONPROC_NAME, args);
            p.WaitForExit();
            string upload_fail_path = Application.dataPath.Replace("/Assets", "") + "uploadfaild_list.txt";
            if (File.Exists(upload_fail_path))
                EditorUtility.DisplayDialog("Warning", "资源上传CDN发生错误, 请查看项目根目录下uploadfaild_list.txt文件", "OK");
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "资源上传CDN错误, 没有设置渠道名或游戏名或版本号!", "OK");
        }
    }
}
