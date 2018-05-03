#if UNITY_ANDROID

using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///  SDK Android平台接口的调用
/// </summary>
public sealed class SDKInterfaceAndroid : SDKInterface
{

    private AndroidJavaObject jo;

    public SDKInterfaceAndroid()
    {
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityActivity"))
        {
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    private T SDKCall<T>(string method, params object[] param)
    {
        try
        {
            return jo.Call<T>(method, param);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return default(T);
    }

    private void SDKCall(string method, params object[] param)
    {
        try
        {
            jo.Call(method, param);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    //这里Android中，在onCreate中直接调用了initSDK，所以这里就不用调用了
    public override void Init()
    {
        //SDKCall("initSDK");
        
    }

    public override void GetSDKInfo()
    {
        SDKCall("getSDKInfo");
    }

    public override void Login()
    {
        SDKCall("login");
    }

    /*public override void LoginCustom(string customData)
    {
        SDKCall("loginCustom", customData);
    }*/

    /*public override void SwitchLogin()
    {
        SDKCall("switchLogin");
    }*/

    public override bool Logout()
    {
        Debug.Log("Start sdk logout");
        if (!IsSupportLogout())
        {
            Debug.LogWarning("SDK not support logout");
            if (OnLogout != null) OnLogout();
            return false;
        }

        SDKCall("logout");
        return true;
    }

    public override bool ShowAccountCenter()
    {
        if (!IsSupportAccountCenter())
        {
            return false;
        }

        SDKCall("showAccountCenter");
        return true;
    }

    public override void SubmitGameData( ExtraGameData data)
    {
        string json = encodeGameData(data);
        SDKCall("submitExtraData", json);
    }

    public override bool SDKExit()
    {
        if (!IsSupportExit())
        {
            return false;
        }

        SDKCall("exit");
        return true;
    }

    /*public override void Pay( PayParams data)
    {
        string json = encodePayParams(data);
        SDKCall("pay", json);
    }*/

    public override void OrderAndPay(PayParams data)
    {
        string json = encodePayParams(data);
        SDKCall("orderAndPay", json);
    }

    public override bool IsSupportExit()
    {
        return SDKCall<bool>("isSupportExit");
    }

    public override bool IsSupportAccountCenter()
    {
        return SDKCall<bool>("isSupportAccountCenter");
    }

    public override bool IsSupportLogout()
    {
        return SDKCall<bool>("isSupportLogout");
    }

    public override bool IsIdentify()
    {
        return SDKCall<bool>("isIdentify");
    }

    public override bool IsAudlt()
    {
        return SDKCall<bool>("isAudlt");
    }

    public override string GetMacAddr()
    {
        return SDKCall<string>("getMacAddr");
    }

    public override string GetIpAddr()
    {
        return SDKCall<string>("getLocalIpAddress");
    }
}

#endif