using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

class CommandBuild
{

    private static bool ms_isDebugBuild = false;
    private static BuildTarget ms_buildTarget = BuildTarget.Android;

    private static string XCODE_PROJECT_NAME = "XCodeProject";
    private static string BUILD_OUTPUT_ANDROID_BIN = "/Public/Update/TestServer/Bin/Android/";
    //private static string BUILD_OUTPUT_ANDROID_RES = "/Public/Update/TestServer/Resources/Android/";
    private static string BUILD_OUTPUT_WINDOWS_BIN = "/Public/Update/TestServer/Bin/Windows/";
    //private static string BUILD_OUTPUT_WINDOWS_RES = "/Public/Update/TestServer/Resources/Windows/";

    public static void Build()
    {
        try
        {
            Debug.Log("Build Player");
            UpdateBuildTarget();

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
                locationPathName = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_WINDOWS_BIN;
                System.DateTime time = System.DateTime.Now;
                locationPathName += "cn.atme.darkfairytale_" + time.Month.ToString("D2") + time.Day.ToString("D2") + time.Hour.ToString("D2") + time.Minute.ToString("D2");
                if (!Directory.Exists(locationPathName))
                    Directory.CreateDirectory(locationPathName);
                locationPathName += "/cn.atme.darkfairytale.exe";
            }
            else
            {
                locationPathName = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + BUILD_OUTPUT_ANDROID_BIN;
                System.DateTime time = System.DateTime.Now;
                locationPathName += "cn.atme.darkfairytale_" + time.Month.ToString("D2") + time.Day.ToString("D2") +
                    "_" + time.Hour.ToString("D2") + time.Minute.ToString("D2") + ".apk";
            }
            BuildPipeline.BuildPlayer(GetBuildScenes(), locationPathName, ms_buildTarget, buildOption);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            EditorApplication.Exit(1);
        }
    }

    public static void BuildResources()
    {
        try
        {
            UpdateBuildTarget();
            //AtMePackerEditor.SwitchAndBuildResources(ms_buildTarget);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            EditorApplication.Exit(1);
        }   
    }

    public static void PreBuild()
    {
        try
        {
            Debug.Log("PreBuild");
            UpdateBuildFlag();
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            EditorApplication.Exit(1);
        }
    }

    //在这里找出你当前工程所有的场景文件
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    private static void UpdateBuildFlag()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string oneArg in args)
        {
            if (oneArg != null && oneArg.Length > 0)
            {
                if (oneArg.ToLower().Contains("-debug"))
                {
                    Debug.Log("\"-debug\" is detected, switch to debug build.");
                    ms_isDebugBuild = true;
                    return;
                }
                else if (oneArg.ToLower().Contains("-release"))
                {
                    Debug.Log("\"-release\" is detected, switch to release build.");
                    ms_isDebugBuild = false;
                    return;
                }
            }
        }

        if (ms_isDebugBuild)
        {
            Debug.Log("neither \"-debug\" nor \"-release\" is detected, current is to debug build.");
        }
        else
        {
            Debug.Log("neither \"-debug\" nor \"-release\" is detected, current is to release build.");
        }
    }

    private static void UpdateBuildTarget()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string oneArg in args)
        {
            if (oneArg != null && oneArg.Length > 0)
            {
                if (oneArg.ToLower().Contains("-android"))
                {
                    Debug.Log("\"-android\" is detected, switch build target to android.");
                    ms_buildTarget = BuildTarget.Android;
                    return;
                }
                else if (oneArg.ToLower().Contains("-iphone"))
                {
                    Debug.Log("\"-iphone\" is detected, switch build target to iphone.");
                    ms_buildTarget = BuildTarget.iOS;
                    return;
                }
                else if (oneArg.ToLower().Contains("-ios"))
                {
                    Debug.Log("\"-ios\" is detected, switch build target to iphone.");
                    ms_buildTarget = BuildTarget.iOS;
                    return;
                }
                else if (oneArg.ToLower().Contains("-windows"))
                {
                    Debug.Log("\"-windows\" is detected, switch build target to StandaloneWindows.");
                    ms_buildTarget = BuildTarget.StandaloneWindows;
                    return;
                }
            }
        }

        Debug.Log("neither \"-android\", \"-ios\", \"-iphone\" nor \"-windows\" is detected, current build target is: " + ms_buildTarget);
    }

    public static void PostBuild()
    {
        Debug.Log("PostBuild");
    }
}