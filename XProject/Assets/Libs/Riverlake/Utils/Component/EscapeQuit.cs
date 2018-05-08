using UnityEngine;
using System.Collections;

/// <summary>
/// 键盘Escape退出游戏
/// </summary>
public class EscapeQuit : MonoBehaviour
{
	public string lastSceneName;

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
            if (string.IsNullOrEmpty(lastSceneName))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
            }
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(lastSceneName);
		}
	}
}
