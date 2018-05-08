using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Riverlake;
using LuaFramework;
using Config;
using DG.Tweening;
using LuaInterface;
using System.Text;
using Riverlake.Crypto;

public sealed class StartUpController : MonoBehaviour 
{
    public static StartUpController Instance;

    public Action clickEvent;
    public Action onGetServerInfo;
    public Action startUpEvent;

    private string _serverInfo;
    private int _getCount;

    private bool alphaInc = false;

    void Awake()
    {
        DebugConsole.Instance.enabled = false;
#if UNITY_EDITOR
        if (!AutoUpdateScene.jumped)
            return;
#endif
        Instance = this;
        SetKey();
        ProgressBar.Show();
        //gameObject.AddComponent<Main>();
        if (User_Config.internal_sdk == 1)
        {
            SDKCallback.InitCallback();
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityActivity");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            SDKInterface.sdkInit = jo.Call<bool>("isFDSDKinitOk");
#else
            SDKInterface.sdkInit = true;
#endif
            SDKInterface.Instance.Init();
            StartCoroutine(WaitForSDKInit());
        }
        else
        {
            StartUp();
        }
    }

    void SetKey()
    {
        TextAsset keyAssets = Resources.Load<TextAsset>("crypto");
        string[] keys = keyAssets.text.Split('|');
        Crypto.Proxy.SetKey(MD5.ComputeHash(Riverlake.Encoding.GetBytes(keys[0])), Riverlake.Encoding.GetBytes(keys[1]));
    }

    IEnumerator WaitForSDKInit()
    {
        while (!SDKInterface.sdkInit) yield return Yielders.EndOfFrame;
        StartUp();
    }

    void StartUp()
    {
        Debug.Log("game start up");
        AppFacade.Instance.StartUp();   //启动游戏
    }

    void Start()
    {
        Debugger.Log("Enter update scene");
    }

    public void OnBackgroundClicked()
    {
        if (clickEvent != null)
            clickEvent();
        clickEvent = null;
        LuaInterface.Debugger.Log("click background");
    }

    public void CheckServerInfo()
    {
        Debug.Log("check server info");
        if (User_Config.internal_sdk == 1)
        {
            StartCoroutine(StartSDK());
        }
        else
        {
            StartCoroutine(GetServerInfoByHttp());
        }
    }

    IEnumerator StartSDK()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            MessageBox.Show(LanguageTips.NETWORK_NOT_REACHABLE);
            yield break;
        }
        yield return Yielders.EndOfFrame;
        SDKInterface.Instance.OnGetSDKInfo = this.OnGetSDKInfo;
        Debug.Log("get sdk info");
        SDKInterface.Instance.GetSDKInfo();
    }

    private void OnGetSDKInfo(InfoResult info)
    {
        CenterServerManager.Instance.AppID = info.appID;
        CenterServerManager.Instance.ChannelID = info.channelID;
        CenterServerManager.Instance.Channel_id = info.channelID;
        // 互冠的pid与appid保持一致  这里使用logoName来判断
        CenterServerManager.Instance.Pid = User_Config.logoName.Equals("hg_jdjh") ? info.appID : info.pID;
        CenterServerManager.Instance.GetCdnData(GameManager.localVersion.ToString(), (version, updateUrl) =>
        {
            Debug.Log(string.Format("cdn version: {0}", version));
            string[] temps = version.Split(':');
            CheckVersion(temps[0]);
        });
    }

    void CheckVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            version = GameManager.localVersion.ToString();
        }
        else
        {
            GameVersion remoteVersion = GameVersion.CreateVersion(version);
            if (GameManager.localVersion < GameManager.packVersion)
            {
                if (remoteVersion < GameManager.packVersion)
                    version = GameManager.packVersion.ToString();
            }
            else
            {
                if (remoteVersion < GameManager.localVersion)
                    version = GameManager.localVersion.ToString();
            }
        }
        User_Config.SetWebServerUrl(Path.Combine(User_Config.resource_server, version));
        Debug.Log(string.Format("Set web url: {0}", User_Config.web_url));
        if (onGetServerInfo != null) onGetServerInfo();
    }

    /// <summary>
    /// 获取服务器信息
    /// </summary>
    /// <returns></returns>
    IEnumerator GetServerInfoByHttp()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            MessageBox.Show(LanguageTips.NETWORK_NOT_REACHABLE);
            yield break;
        }
        _serverInfo = string.Empty;
        _getCount += 1;
        WWW getData = new WWW(User_Config.server_url);
        yield return getData;
        if (!string.IsNullOrEmpty(getData.error))
        {
            Debug.Log(getData.error);
            if (_getCount < 3)
            {
                StartCoroutine(GetServerInfoByHttp());
                yield break;
            }
            else
            {
                getData.Dispose();
                yield break;
            }
        }
        else
        {
            _serverInfo = getData.text;
            SetServerInfo();
        }
        getData.Dispose();

        if (onGetServerInfo != null) onGetServerInfo();
    }

    bool SetServerInfo()
    {
        Hashtable tbl = MiniJSON.Json.Deserialize(_serverInfo) as Hashtable;

        if (tbl == null)
        {
            Debug.LogError(string.Format("Get server list error: {0}", _serverInfo));

#if UNITY_EDITOR
            print("------------------------------------------------------------------");
            _serverInfo = "{\"update_url\":\"http://192.168.0.243/rsync/TestServer/Resources/\",\"corp\":\"1\",\"version\":\"1.0.0\",\"recommend_no\":\"2\",\"serverlist\":[{\"server_no\":\"1\",\"server_name\":\"彭老师联调\",\"ip\":\"192.168.0.94\",\"port\":\"1221\",\"is_open\":\"1\",\"other\":\"\"},{\"server_no\":\"2\",\"server_name\":\"高士哥\",\"ip\":\"192.168.0.80\",\"port\":\"8999\",\"is_open\":\"1\",\"other\":\"\"},{\"server_no\":\"3\",\"server_name\":\"243内部\",\"ip\":\"192.168.0.243\",\"port\":\"2001\",\"is_open\":\"1\",\"other\":\"\"},{\"server_no\":\"5\",\"server_name\":\"海明\",\"ip\":\"192.168.0.185\",\"port\":\"1221\",\"is_open\":\"1\",\"other\":\"\"},{\"server_no\":\"4\",\"server_name\":\"243内部(稳)\",\"ip\":\"192.168.0.243\",\"port\":\"2002\",\"is_open\":\"1\",\"other\":\"\"}]}";
            tbl = MiniJSON.Json.Deserialize(_serverInfo) as Hashtable;
            if (tbl == null)
                return false;
#else
            return false;
#endif
        }

        var temp_url = tbl["update_url"].ToString().TrimEnd('/');
        var temps = temp_url.Split('/');
        StringBuilder update_url = new StringBuilder();
        for (int i = 0; i < temps.Length; ++i)
        {
            if (i < temps.Length - 1)
            {
                update_url.Append(temps[i]);
                update_url.Append("/");
            }
            else
            {
                if (temps[i].Split('.').Length >= 3)
                {
                    var remoteVersion = GameVersion.CreateVersion(temps[i]);
                    if (GameManager.localVersion < GameManager.packVersion)
                    {
                        if (remoteVersion < GameManager.packVersion)
                            update_url.Append(GameManager.packVersion.ToString());
                        else
                            update_url.Append(remoteVersion.ToString());
                    }
                    else
                    {
                        if (remoteVersion < GameManager.localVersion)
                            update_url.Append(GameManager.localVersion.ToString());
                        else
                            update_url.Append(remoteVersion.ToString());
                    }
                }
                else
                {
                    update_url.Append(temps[i]);
                }
                update_url.Append("/");
            }
        }

        User_Config.SetWebServerUrl(update_url.ToString());
        User_Config.SetDefaultServer(Convert.ToInt32(tbl["recommend_no"]));

        ServerInfo info;
        if (User_Config.serverList == null)
            User_Config.serverList = new System.Collections.Generic.List<ServerInfo>();

        ArrayList list = tbl["serverlist"] as ArrayList;
        Hashtable serverTbl;
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            serverTbl = list[i] as Hashtable;

            info = new ServerInfo();
            info.openTime = i;
            info.serverNo = Convert.ToInt32(serverTbl["server_no"].ToString());
            info.serverName = serverTbl["server_name"].ToString();
            info.serverIp = serverTbl["ip"].ToString();
            info.serverPort = serverTbl["port"].ToString();
            info.isOpen = Convert.ToInt32(serverTbl["is_open"].ToString());
            info.other = serverTbl["other"].ToString();
            info.hostId = serverTbl["host_id"].ToString();
            if (serverTbl.ContainsKey("corp_id"))
                info.corpId = serverTbl["corp_id"].ToString();
            else
                info.corpId = "1000";

            User_Config.serverList.Add(info);
        }

        User_Config.serverList.Sort((x , y)=>x.openTime.CompareTo(y.openTime) * -1);
        return true;
    }
    
    void OnDestroy()
    {
        Instance = null;
    }
}
