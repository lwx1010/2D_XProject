using System.Collections;
using UnityEngine;
using System;
using Riverlake;

/// <summary>
///  SDK 回调 Android和IOS走同样的回调接口，保证接口的统一性
/// </summary>
public sealed class SDKCallback : MonoBehaviour
{

    private static SDKCallback _instance;

    private static object _lock = new object();

    private float lastWarningTime = 0;

    //初始化回调对象
    public static SDKCallback InitCallback()
    {
        Debug.Log("Callback->InitCallback");

        lock (_lock)
        {
            if (_instance == null)
            {
                GameObject callback = GameObject.Find("(sdk_callback)");
                if (callback == null)
                {
                    callback = new GameObject("(sdk_callback)");
                    UnityEngine.Object.DontDestroyOnLoad(callback);
                    _instance = callback.AddComponent<SDKCallback>();
                }
                else
                {
                    _instance = callback.GetComponent<SDKCallback>();
                }
            }

            return _instance;
        }
    }

    public void OnGetSDKInfo(string jsonData)
    {
        Debug.Log("Callback->OnGetSDKInfo");
        InfoResult data = parseInfoResult(jsonData);
        if (data == null)
        {
            Debug.Log("The data parse error." + jsonData);
            return;
        }
        if (SDKInterface.Instance.OnGetSDKInfo != null)
        {
            SDKInterface.Instance.OnGetSDKInfo.Invoke(data);
        }
    }

    //初始化成功回调
    public void OnInitSuc(string jsonData)
    {
        //一般不需要处理
        Debug.Log("Callback->OnInitSuc");

        SDKInterface.sdkInit = true;
    }

    //登录成功回调
    public void OnLoginSuc(string jsonData)
    {
        Debug.Log("Callback->OnLoginSuc");

        LoginResult data = parseLoginResult(jsonData);
        if (data == null)
        {
            Debug.Log("The data parse error." + jsonData);
            return;
        }

        if (SDKInterface.Instance.OnLoginSuc != null)
        {
            SDKInterface.Instance.OnLoginSuc.Invoke(data);
        }
    }

    //切换帐号回调
    public void OnSwitchLogin()
    {

        UnityEngine.Debug.Log("Callback->OnSwitchLogin");

        if (SDKInterface.Instance.OnLogout != null)
        {
            SDKInterface.Instance.OnLogout.Invoke();
        }
    }

    //登出回调
    public void OnLogout()
    {
        UnityEngine.Debug.Log("Callback->OnLogout");

        if (SDKInterface.Instance.OnLogout != null)
        {
            SDKInterface.Instance.OnLogout.Invoke();
        }
    }

    //支付回调
    public void OnPaySuc(string jsonData)
    {
        PayResult data = parsePayResult(jsonData);
        if (data == null)
        {
            Debug.Log("The data parse error." + jsonData);
            return;
        }
        if (SDKInterface.Instance.OnPaySuc != null)
        {
            SDKInterface.Instance.OnPaySuc.Invoke(data);
        }
    }

    public void OnMemoryWarning()
    {
        Debug.LogWarning("Received memory warning - ios system");
        float time = Time.realtimeSinceStartup;
        if (time - lastWarningTime > 30 || lastWarningTime == 0)
        {
            lastWarningTime = time;
            AssetBundleManager.Instance.Clear();
        }
    }

    private InfoResult parseInfoResult(string str)
    {
        object jsonParsed = MiniJSON.Json.Deserialize(str);
        if (jsonParsed != null)
        {
            Hashtable jsonMap = jsonParsed as Hashtable;
            InfoResult data = new InfoResult();
            
            if (jsonMap.ContainsKey("pID"))
            {
                data.pID = int.Parse(jsonMap["pID"].ToString());
            }
            if (jsonMap.ContainsKey("channelID"))
            {
                data.channelID = int.Parse(jsonMap["channelID"].ToString());
            }
            if (jsonMap.ContainsKey("appID"))
            {
                data.appID = int.Parse(jsonMap["appID"].ToString());
            }
            return data;
        }
        return null;
    }

    private LoginResult parseLoginResult(string str)
    {
        object jsonParsed = MiniJSON.Json.Deserialize(str);
        if (jsonParsed != null)
        {
            Hashtable jsonMap = jsonParsed as Hashtable;
            LoginResult data = new LoginResult();
            if (jsonMap.ContainsKey("isSuc"))
            {
                data.isSuc = bool.Parse(jsonMap["isSuc"].ToString());
            }
            if (jsonMap.ContainsKey("isSwitchAccount"))
            {
                data.isSwitchAccount = bool.Parse(jsonMap["isSwitchAccount"].ToString());
            }
            if (jsonMap.ContainsKey("userID"))
            {
                data.userID = jsonMap["userID"].ToString();
            }
            if (jsonMap.ContainsKey("sdkUserID"))
            {
                data.sdkUserID = jsonMap["sdkUserID"].ToString();

            }
            if (jsonMap.ContainsKey("username"))
            {
                data.username = jsonMap["username"].ToString();
            }

            if (jsonMap.ContainsKey("sdkUsername"))
            {
                data.sdkUsername = jsonMap["sdkUsername"].ToString();
            }
            if (jsonMap.ContainsKey("token"))
            {
                data.token = jsonMap["token"].ToString();
            }
            if (jsonMap.ContainsKey("extension"))
            {
                data.extension = jsonMap["extension"].ToString();
            }
            return data;
        }

        return null;
    }

    private PayResult parsePayResult(string str)
    {
        object jsonParsed = MiniJSON.Json.Deserialize(str);
        if (jsonParsed != null)
        {
            Hashtable jsonMap = jsonParsed as Hashtable;
            PayResult data = new PayResult();
            if (jsonMap.ContainsKey("productID"))
            {
                data.productID = jsonMap["productID"].ToString();
            }
            if (jsonMap.ContainsKey("productName"))
            {
                data.productName = jsonMap["productName"].ToString();
            }
            if (jsonMap.ContainsKey("extension"))
            {
                data.extension = jsonMap["extension"].ToString();
            }
            return data;
        }
        return null; 
    }
}