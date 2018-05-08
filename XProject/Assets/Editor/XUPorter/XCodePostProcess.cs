using UnityEngine;
using System.Text;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;

public static class XCodePostProcess
{
    public static string config_path;
#if UNITY_EDITOR
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
#if UNITY_5
		if (target != BuildTarget.iOS) {
#else
        if (target != BuildTarget.iPhone) {
#endif
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}

        if (string.IsNullOrEmpty(config_path)) config_path = Application.dataPath + "/Res/Template/IOSConfig/XYFYSMYYSDK_Test";
        XCConfigItem configItem = XCConfigItem.ParseXCConfig(config_path);
        if (configItem != null)
        {
            File.WriteAllText(Application.dataPath + "/ios_pack.txt", "config_path is " + config_path);
            configItem.EditProject(pathToBuiltProject);
        }
        else
            File.WriteAllText(Application.dataPath + "/ios_error.txt", "config_path is null");
	}
#endif

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
}
