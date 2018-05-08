using UnityEngine;
using System.Collections;
using System;

public class TimeConverter
{
	/// <summary>
	/// 将Unix时间戳转换为DateTime类型时间
	/// </summary>
	/// <param name="d">double 型数字</param>
	/// <returns>DateTime</returns>
	public static DateTime ConvertIntDateTime(double d)
	{
		DateTime time = DateTime.MinValue;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
#else
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#endif
        time = startTime.AddSeconds(d);
		return time;
	}
	/// <summary>
	/// 将c# DateTime时间格式转换为Unix时间戳格式
	/// </summary>
	/// <param name="time">时间</param>
	/// <returns>double</returns>
	public static double ConvertDateTimeInt(DateTime time)
	{
		double intResult = 0;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
#else
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#endif
        intResult = (time - startTime).TotalSeconds;
		return intResult;
	}

    public static long ConvertDateTimeLong(DateTime time)
    {
        long intResult = 0;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
#else
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#endif
        intResult = (time - startTime).Ticks;
        return intResult;
    }

    /// <summary>
    /// 返回mm：ss
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string CovertToString(int seconds)
    {
        int m = seconds / 60;
        int s = seconds % 60;

        return string.Format("{0:D2}:{1:D2}", m, s);
    }

    /// <summary>
    /// 返回hh:mm:ss
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string ConvertToHoursString(int seconds)
    {
        int h = seconds / 3600;
        int m = seconds % 3600 / 60;
        int s = seconds % 3600 % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
    }

    /// <summary>
    /// 转换成 2016/1/20 11:11:11
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static string ConvertToDateString(double d)
    {
        DateTime time = DateTime.MinValue;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
#else
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#endif
        time = startTime.AddSeconds(d);
        
      
        return  time.ToString("yyyy年MM月dd号H时mm分ss秒");
    }

    /// <summary>
    /// 转换成 2016/1/20 11:11:11
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static string ConvertToLogDateString(double d)
    {
        DateTime time = DateTime.MinValue;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
#else
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#endif
        time = startTime.AddSeconds(d);


        return time.ToString("yyyy年MM月dd号HH:mm");
    }

    /// <summary>
    /// 转换成11:11:11
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static string ConvertToDateString1(double d)
    {
        DateTime time = DateTime.MinValue;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
#else
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
#endif
        time = startTime.AddSeconds(d);
        return time.TimeOfDay.ToString();
    }

    /// <summary>
    /// 解析时间字符
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime Parse(string dateTime)
    {
        return DateTime.Parse(dateTime);
    }
}
