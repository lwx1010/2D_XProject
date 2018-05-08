using UnityEngine;
using System.Collections;
using Riverlake;
using System;

public sealed class TimeManager : Singleton<TimeManager>
{
    private UInt64 serverSecSinceStarted;

    private float beginAppStartup;
    private static DateTime mStartTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970 , 1 , 1));

    public static TimeManager instance
    {
        get
        {
            return TimeManager.Instance;
        }
    } 
    /// <summary>
    /// 当前的服务器时间 
    /// </summary>
    public DateTime CurServerTime
    {
        get
        {
            ulong currentTime = serverSecSinceStarted + (ulong)(Time.realtimeSinceStartup - beginAppStartup );
            return mStartTime.AddSeconds(currentTime);
        }
    }
    /// <summary>
    /// 当前服务器时间的总秒数
    /// </summary>
    public UInt64 CurServerTotalSeconds
    {
        get
        {
            return serverSecSinceStarted + (ulong)(Time.realtimeSinceStartup - beginAppStartup);
        }
    }
    /// <summary>
    /// 设置服务器的时间
    /// </summary>
    /// <param name="millisec">毫秒</param>
    /// <param name="usec"></param>
    public void SetServerTime(string millisec, uint usec)
    {
        beginAppStartup = Time.realtimeSinceStartup;
        try
        {
            if (string.IsNullOrEmpty(millisec)) millisec = "0";
            serverSecSinceStarted = Convert.ToUInt64(millisec) / 1000 + (ulong)(usec / 1000000f); //转换为秒
        }
        catch (Exception e)
        {
            Debug.LogError("set servettime error: " + e.Message + "\nstack trace:" + e.StackTrace);
        }
    }
    /// <summary>
    /// 格式化DateTime
    /// </summary>
    /// <param name="time"></param>
    /// <param name="format">格式 eg:dd-MM-yy</param>
    /// <returns></returns>
    public static string FormatDateTime(DateTime time, string format)
    {
        return time.ToString(format);
    }

    /// <summary>
    /// 获得当前时间的总秒数
    /// </summary>
    /// <returns></returns>
    public static double TotalSecondsToCurrentTime()
    {
        DateTime curLocalTime = Instance.CurServerTime;
        return (curLocalTime - mStartTime).TotalSeconds;
    }
    /// <summary>
    /// 获得当前时间的总毫秒数
    /// </summary>
    /// <returns></returns>
    public static double TotalMilliSecondsToCurrentTime()
    {
        DateTime curLocalTime = Instance.CurServerTime;
        return (curLocalTime - mStartTime).TotalMilliseconds;
    }

    public static double TotalMilliSeconds(DateTime time)
    {
        return (time - mStartTime).TotalMilliseconds;
    }

    public static float GetRealTimeSinceStartUp()
    {
        return Time.realtimeSinceStartup;
    }
}
