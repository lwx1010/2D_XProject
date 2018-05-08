#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using UnityEngine;


/// <summary>
/// 处理与本地平台相关的操作
/// </summary>
public class PlatformHelper
{
#region ----------------------------本地系统剪切板-------------------------------------------

#if UNITY_IOS
    [DllImport ("__Internal")]
    private static extern void _copyToClipboard(string text);
#endif

    /// <summary>
    /// 拷贝文本到剪切板
    /// </summary>
    /// <param name="text"></param>
    public static void CopyToClipboard(string text)
    {
        if (Application.isEditor) return;
        //Debug.Log(string.Format("CopyToClipboard: {0}", text));
#if UNITY_ANDROID
        AndroidJavaObject javaObj = new AndroidJavaObject("com.hw.jh.ClipboardHelper");
        javaObj.Call("copyToClipboard", text);
#elif UNITY_IOS
        _copyToClipboard(text);
#endif
    }

#endregion
}
