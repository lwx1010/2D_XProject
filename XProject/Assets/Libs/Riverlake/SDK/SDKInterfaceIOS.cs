using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

public class SDKInterfaceIOS : SDKInterface
{
    #region import methods
    [DllImport("__Internal")]
	private static extern void SDK_Setup();
	[DllImport("__Internal")]
	private static extern void SDK_Login();
	[DllImport("__Internal")]
	private static extern bool SDK_Logout();
	[DllImport("__Internal")]
	private static extern bool SDK_SDKExit();
	[DllImport("__Internal")]
	private static extern void SDK_Pay (string payParams);
	[DllImport("__Internal")]
	private static extern bool SDK_IsSupportExit();
	[DllImport("__Internal")]
	private static extern void SDK_UserCenter();
	[DllImport("__Internal")]
	private static extern bool SDK_IsSupportLogout ();
    [DllImport("__Internal")]
    private static extern void SDK_GetSDKInfo();
    #endregion
    // move init to application didFinishLaunchingWithOptions
    public override void Init()
    {
        SDK_Setup();
    }

    public override void Login()
    {
        SDK_Login();
    }

    public override bool Logout()
    {
        if (!IsSupportLogout())
        {
            Debug.LogWarning("SDK not support logout");
            if (OnLogout != null) OnLogout();
            return false;
        }

        return SDK_Logout();
    }

    public override bool SDKExit()
    {
		return SDK_SDKExit();
    }

    public override void OrderAndPay(PayParams data)
	{
        string json = encodePayParams(data);
        SDK_Pay(json);
    }

    public override bool IsSupportExit()
    {
		return SDK_IsSupportExit();
    }

    public override bool IsSupportLogout()
    {
        return SDK_IsSupportLogout();
    }

    public override void GetSDKInfo()
    {
        SDK_GetSDKInfo();
    }

    public override bool ShowAccountCenter()
    {
        return false;
    }

    //上传游戏数据
    public override void SubmitGameData(ExtraGameData data)
    {
        string json = encodeGameData(data);
    }

    public override bool IsSupportAccountCenter()
    {
        return false;
    }

    public override bool IsIdentify()
    {
        return true;
    }
    /// <summary>
    /// 是否已成年
    /// </summary>
    /// <returns></returns>
    public override bool IsAudlt()
    {
        return true;
    }

    //获取Mac地址
    public override string GetMacAddr()
    {
        string physicalAddress = string.Empty;
        NetworkInterface[] nice = NetworkInterface.GetAllNetworkInterfaces();
        for (int i = 0; i < nice.Length; ++i)
        {
            var adaper = nice[i];
            if (adaper.Description == "en0")
            {
                physicalAddress = adaper.GetPhysicalAddress().ToString();
                break;
            }
            else
            {
                physicalAddress = adaper.GetPhysicalAddress().ToString();
                if (physicalAddress != "")
                    break;
            }
        }
        return physicalAddress;
    }
    //获取Ip地址
    public override string GetIpAddr()
    {
        //Dns.GetHostName()获取本机名Dns.GetHostAddresses()根据本机名获取ip地址组
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());   
        for (int i = 0; i < ips.Length; ++i)
        {
            var ip = ips[i];
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                return ip.ToString();
        }
        return string.Empty;
    }
}