using UnityEngine;
using System.Collections;
using LuaFramework;

/// <summary>
/// 编辑器下自动跳转到UpdateScene，方便开发
/// </summary>
public class AutoUpdateScene : MonoBehaviour
{
	static public bool jumped = false;

    void Awake()
    {
        Util.AutoAdjustCameraRect(Camera.main);
        AutoJump();
    }

    void AutoJump()
	{
#if UNITY_EDITOR
        if (jumped)
			return;
		jumped = true;
		var current = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name);
        string startScene = "StartScene";
        bool exist = false;
        foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
        {
            if (System.IO.File.Exists(scene.path) && scene.path.Contains(startScene))
            {
                exist = true;
                break;
            }
        }
        if (!current.Equals(startScene) && exist)
		{
            Debug.Log(string.Format("Auto jump: {0} -> {1}", current, startScene));
            UnityEngine.SceneManagement.SceneManager.LoadScene(startScene);
		}
#endif
    }
}
