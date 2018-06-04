/************************************************************
* @Author: LiangZG
* @Time : 2017-12-09
************************************************************/
using UnityEngine;
using System.Collections;

/// <summary>
/// 本地SDK扩展补充，比如推送，分享
/// </summary>
public sealed class NativeSDK {


    /// <summary>
    /// 启动注册推送绑定
    /// </summary>
    public static void StartPushService(string pushText)
    {
#if UNITY_EDITOR
        //Debug.Log(pushText);
        return;
#elif UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.hw.push.PushManager");
        AndroidJavaObject pushMgr = javaClass.CallStatic<AndroidJavaObject>("getInstance");
        pushMgr.Call("startPushServices", pushText);
#elif UNITY_IOS

#endif
    }
}
