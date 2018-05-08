using UnityEngine;
using System.Collections;
using Riverlake;
using LuaFramework;
using Config;
using Riverlake.Network;

#if !UNITY_METRO && !NETFX_CORE
[System.Reflection.Obfuscation(Exclude = true)]
#endif
public class Fps : Singleton<Fps>
{
    private long mFrameCount;
    private long mLastFrameTime;
    public static long mLastFps;

    GUIStyle style = new GUIStyle();

    public new void Init()
    {
        if (Debug.isDebugBuild)
        {
            style.font = Resources.Load<Font>("Fonts/msyh.ttf");
            style.fontSize = 20;
        }
    }

    // Use this for initialization
    void Start()
    {
        mFrameCount = 0;
        mLastFrameTime = 0;
        mLastFps = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTick();
    }

    void OnGUI()
    {
        if (Debug.isDebugBuild) DrawFps();
    }

    private void DrawFps()
    {
        if (style == null)
            return;

        if (mLastFps > 25)
        {
            style.normal.textColor = new Color(0, 1, 0);
        }
        else if (mLastFps > 15)
        {
            style.normal.textColor = new Color(1, 1, 0);
        }
        else
        {
            style.normal.textColor = new Color(1, 0, 0);
        }

        GUI.Label(new Rect(Screen.width - 180, 32, 320, 240), string.Format("fps: {0}", mLastFps), style);

    }

    private void UpdateTick()
    {
        if (true)
        {
            mFrameCount++;
            long nCurTime = TickToMilliSec(System.DateTime.Now.Ticks);
            if (mLastFrameTime == 0)
            {
                mLastFrameTime = TickToMilliSec(System.DateTime.Now.Ticks);
            }

            if ((nCurTime - mLastFrameTime) >= 1000)
            {
                long fps = (long)(mFrameCount * 1.0f / ((nCurTime - mLastFrameTime) / 1000.0f));

                mLastFps = fps;

                mFrameCount = 0;

                mLastFrameTime = nCurTime;
            }
        }
    }

    public static long TickToMilliSec(long tick)
    {
        return tick / (10 * 1000);
    }

    #region 平均帧数检测
    public bool needCheckFps = false;
    public bool checkFps = true;        //弹出一次提示后，间断10分钟再开始检测
    private float stopCheckTime = 0;
    private float STOP_TIME_INTERVAL = 600.0f;

    private float CHECK_TIME_INTERVAL = 1.0f;
    private float startCheckTime = 0;
    private float lastCheckTime = 0;
    private long totalFps = 0;

    private void FixedUpdate()
    {
        if (!needCheckFps || ConnectObserver.powerSaveMode)
            return;

        if (!checkFps)
        {
            if ((Time.realtimeSinceStartup - stopCheckTime) < STOP_TIME_INTERVAL)
                return;

            checkFps = true;
            startCheckTime = Time.realtimeSinceStartup;
            lastCheckTime = Time.realtimeSinceStartup;
            totalFps = 0;
            return;
        }

        if ((Time.realtimeSinceStartup - lastCheckTime) < CHECK_TIME_INTERVAL)
            return;

        totalFps += mLastFps;
        lastCheckTime = Time.realtimeSinceStartup;

        if ((Time.realtimeSinceStartup - startCheckTime) < User_Config.fpsInterval)
            return;

        startCheckTime = Time.realtimeSinceStartup;

        //调用lua那边弹出提示
        //Util.CallMethod("COMMONCTRL", "ShowFpsSettingTip", totalFps / User_Config.fpsInterval);
        Check(totalFps / User_Config.fpsInterval);
        totalFps = 0;
    }

    private void Check(long fps)
    {
        int cnt = (int)(User_Config.playerScreenCount * User_Config.MaxPlayerScreen);
        if (fps < User_Config.fpsHigh && cnt >= 10 && User_Config.blockOtherPlayer == 0)
        {
            Util.CallMethod("COMMONCTRL", "ShowFpsSettingTip", 2);
            return;
        }

        if (fps < User_Config.fpsLow && cnt > 0 && cnt < 10
            && (User_Config.blockOtherPlayer == 0
                || User_Config.blockOtherPartner == 0
                || User_Config.blockAllianPlayer == 0
                || User_Config.blockMonster == 0
                || User_Config.blockAllTitleSpr == 0
                || User_Config.showSimpleSkillEff == 0))
        {
            Util.CallMethod("COMMONCTRL", "ShowFpsSettingTip", 1);
            return;
        }
    }

    /// <summary>
    /// 停止检测fps
    /// </summary>
    public void StopCheckFps()
    {
        checkFps = false;
        stopCheckTime = Time.realtimeSinceStartup;
    }

    public void ContinueCheckFps()
    {
        checkFps = true;
        startCheckTime = Time.realtimeSinceStartup + 3;
        lastCheckTime = Time.realtimeSinceStartup + 3;
        totalFps = 0;
    }

    public void StartCheckFps()
    {
        needCheckFps = true;
        startCheckTime = Time.realtimeSinceStartup + 15;
        lastCheckTime = Time.realtimeSinceStartup + 15;
        totalFps = 0;
    }

    public void CancelCheckFps()
    {
        needCheckFps = false;
    }
    #endregion
}
