using UnityEngine;
using System.Collections;
using System;
using LuaFramework;
using Config;

public sealed class ScreenResolution : MonoBehaviour
{
    public static ScreenResolution Instance { get; private set; }
#if UNITY_IOS
    public int designWidth { get; private set; }
    public int designHeight { get; private set; }
#else
    public int designWidth { get; private set; }
    public int designHeight { get; private set; }
#endif
    public int defaultWidth { get; private set; }
    public int defaultHeight { get; private set; }
    public int scaleWidth { get; private set; }
    public int scaleHeight { get; private set; }

    public static ScreenResolution GetInstance()
    {
        return Instance;
    }

    void Awake()
	{
        Instance = this;
        DontDestroyOnLoad(this);
#if UNITY_IOS && !UNITY_EDITOR
        string deviceVersion = SystemInfo.deviceModel;
        if (!string.IsNullOrEmpty(deviceVersion) && (deviceVersion.Equals("iPhone10,3") || deviceVersion.Equals("iPhone10,6")))
        {
            defaultWidth = Screen.width - 132 * 2;
            defaultHeight = Screen.height;
        }
        else
        {
            defaultWidth = Screen.width;
            defaultHeight = Screen.height;
        }
#else
        defaultWidth = Screen.width;
        defaultHeight = Screen.height;
#endif
    }

    public void AdjustResolution()
    {
        if (Screen.currentResolution.width < 1280 || Screen.currentResolution.height < 720)
        {
            designWidth = Screen.currentResolution.width;
            designHeight = Screen.currentResolution.height;
        }
        else
        {
            if (User_Config.quality <= 1)
            {
                designWidth = 1280;
                designHeight = 720;
            }
            else if (User_Config.quality == 2)
            {
                designWidth = 1920;
                designHeight = 1080;
            }
            else
            {
                designWidth = 1280;
                designHeight = 720;
            }
        }
    }

	public void setDesignContentScale()
	{
        if (scaleWidth == 0 && scaleHeight == 0)
        {
            Resolution[] temp = Screen.resolutions;
#if UNITY_EDITOR
            int width = Screen.width;
            int height = Screen.height;
#else
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
#endif
            var tempWidth = designWidth;
            var tempHeight = designHeight;
            float s1 = (float)tempWidth / (float)tempHeight;
            float s2 = (float)width / (float)height;
            int int1 = (int)(s1 * 100);
            int int2 = (int)(s2 * 100);
            if (int1 < int2)
                tempWidth = (int)Mathf.FloorToInt(tempHeight * s2);
            else if (int1 > int2)
                tempHeight = (int)Mathf.FloorToInt(tempWidth / s2);
            
            scaleWidth = tempWidth;
            scaleHeight = tempHeight;
        }
        if (scaleWidth > 0 && scaleHeight > 0)
        {
            if (scaleWidth % 2 != 0)
                scaleWidth -= 1;
            Debug.Log(string.Format("Set Screen: {0}x{1}", scaleWidth, scaleHeight));
            Screen.SetResolution(scaleWidth, scaleHeight, Application.platform == RuntimePlatform.WindowsPlayer ? false : true);
        }
    }

    void OnApplicationPause(bool paused)
	{
#if UNITY_IOS
        if (paused)
        {
            Application.targetFrameRate = 10;
        }
        else
        {
            Application.targetFrameRate = AppConst.GameFrameRate;
            setDesignContentScale();
        }
#endif
    }
}
