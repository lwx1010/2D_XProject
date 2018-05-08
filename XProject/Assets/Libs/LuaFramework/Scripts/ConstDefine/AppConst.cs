using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AppConst {

#if UNITY_EDITOR
    public static bool DebugMode = true;                         //调试模式-用于内部测试,影响释放
#else
    public const bool DebugMode = false;                        //调试模式-用于内部测试
#endif

    /// <summary>
    /// 如果开启更新模式，前提必须启动框架自带服务器端。
    /// 否则就需要自己将StreamingAssets里面的所有内容
    /// 复制到自己的Webserver上面，并修改下面的WebUrl。
    /// </summary>
#if UNITY_EDITOR
    public static bool UpdateMode = false;                       //更新模式-默认关闭
#else
    public const bool UpdateMode = true;                       //更新模式-默认开启(非编辑器)
#endif
#if UNITY_IOS
    public const bool LuaByteMode = false;                       //Lua字节码模式-默认关闭
#else
    public const bool LuaByteMode = true;                       //Lua字节码模式-默认关闭
#endif
#if UNITY_EDITOR
    public static bool AssetBundleMode = false;                  //资源AssetBundle模式
    public static bool LuaBundleMode = false;                    //Lua代码AssetBundle模式-默认关闭
#else
    public const bool AssetBundleMode = true;                   //资源AssetBundle模式
    public const bool LuaBundleMode = true;                     //Lua代码AssetBundle模式-不是编辑器模式下默认打开
#endif


    public const int TimerInterval = 1;
    public const int GameFrameRate = 30;                       //游戏帧频

    public const string AppName = "jh2";                       //应用程序名称
    public const string LuaTempDir = "Lua/";                    //临时目录
    public const string ExtName = ".unity3d";                   //资源扩展名
    public const string AppPrefix = AppName + "_";              //应用程序前缀

    public static string UserId = string.Empty;                 //用户ID
    public static int SocketPort = 0;                           //Socket服务器端口
    public static string SocketAddress = string.Empty;          //Socket服务器地址
    /// <summary>
    /// 是否使用动态阴影,true表示使用
    /// </summary>
    public const bool IsDynamicShadow = true;
    public static string FrameworkRoot {
        get {
            return Application.dataPath + "/LuaFramework";
        }
    }

    public static string[] UISoundConfig = new[]
    {
        "button-down","panel-change","panel-open","panel-close","bonus-item","bonus-task",
        "function-unlocked","levelup","loot","mission-complete","mission-failed",
        "skill-alert","strenthen-fail","strenthen-success"
    };
}
