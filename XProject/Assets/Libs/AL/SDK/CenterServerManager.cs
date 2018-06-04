using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;
using Config;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using LuaInterface;
using UnityEngine.Networking;
using System.Net;
using System.IO;
using System.Threading;

/***********************
* Author: 柯明余
* Time:   2017-04-06
************************/

/// <summary>
/// 中央服管理类
/// </summary>
public sealed class CenterServerManager : MonoBehaviour
{
    private static CenterServerManager instance;

    private Queue<HttpRequestEvent> requestQueue = new Queue<HttpRequestEvent>();

    private object lockObj = new object();
    private class HttpRequestEvent
    {
        public string PostDatas;
        public Action<string> OnComplete;
    }
    //是否正在请求
    private bool isRequesting;
    public static CenterServerManager Instance
    {
        get {
            if (instance == null)
            {
                GameObject callback = GameObject.Find("(sdk_callback)");
                if (callback == null)
                {
                    callback = new GameObject("(sdk_callback)");
                    DontDestroyOnLoad(callback);
                    instance = callback.GetComponent<CenterServerManager>();
                    if (instance == null)
                        instance = callback.AddComponent<CenterServerManager>();
                }
                else
                {
                    instance = callback.GetComponent<CenterServerManager>();
                    if (instance == null)
                        instance = callback.AddComponent<CenterServerManager>();
                }
                instance.Init();
            }
            return instance;
        }
    }

    private void Init()
    {
        if (!string.IsNullOrEmpty(User_Config.sdk_server_url))
            instance.url = User_Config.sdk_server_url;
        if (!string.IsNullOrEmpty(User_Config.cdn_server_url))
            instance.cdnUrl = User_Config.cdn_server_url;
    }

    private void HttpRequest(string data, Action<string> callback , bool isThread = false)
    {
        if (isThread)
        {
            HttpRequestEvent hre = new HttpRequestEvent();
            hre.PostDatas = data;
            hre.OnComplete = callback;
            lock (lockObj)
            {
                requestQueue.Enqueue(hre);
            }

            if (!isRequesting)
            {
                //PanelManager panelManager = AppFacade.Instance.GetManager<PanelManager>();
//                StartCoroutine(preRequestPost());
                preRequestPostThread();
            }
        }
        else
        {
            HttpPostAsync(url, data, callback);
        }
    }

    /// <summary>
    /// POST请求与获取结果
    /// </summary>
    private void HttpPostAsync(string Url, string postDataStr, Action<string> callBack)
    {
        //PanelManager panelManager = AppFacade.Instance.GetManager<PanelManager>();
        StartCoroutine(requestPostAsync(Url, postDataStr, callBack));
    }

    /// <summary>
    /// 逐个请求
    /// </summary>
    /// <returns></returns>
    private IEnumerator preRequestPost()
    {
        if(requestQueue.Count <= 0) yield break;

        HttpRequestEvent[] hteArr = requestQueue.ToArray();
        requestQueue.Clear();
        isRequesting = true;
        //PanelManager panelManager = AppFacade.Instance.GetManager<PanelManager>();

        for (int i = 0; i < hteArr.Length; i++)
        {
            HttpRequestEvent req = hteArr[i];

            
            yield return StartCoroutine(requestPostAsync(url, req.PostDatas, req.OnComplete));
        }

        isRequesting = false;
        yield return StartCoroutine(preRequestPost());
    }



    private void preRequestPostThread()
    {
        isRequesting = false;
        if (requestQueue.Count <= 0) return;

        HttpRequestEvent[] hteArr = null;
        lock (lockObj)
        {
           hteArr = requestQueue.ToArray();
            requestQueue.Clear();            
        }
        isRequesting = true;

        ThreadManager threadMgr = AppFacade.Instance.GetManager<ThreadManager>();
        Loom.StartMultithreadedWorkloadExecution((hre) =>HttpRequestGet(hre.PostDatas),
                                                 hteArr, (hre) => preRequestPostThread() , null, 1 , threadMgr.PoolScheduler);
    }
    
    private IEnumerator requestPostAsync(string url, string postDataStr, Action<string> callBack, int restartTime = 0)
    {
        var finalUrl = string.Format("{0}?{1}", url, postDataStr);
        using (var www = new WWW(finalUrl))
        {
            Debugger.Log("post to {0}", finalUrl);
            Debugger.Log("try time: {0}", restartTime);
            bool timeout = true;
            float endTime = UnityEngine.Time.realtimeSinceStartup + 5;

            while (UnityEngine.Time.realtimeSinceStartup < endTime)
            {
                if (www.isDone)
                {
                    timeout = false;
                    break;
                }
                yield return 0;
            }

            if (timeout)
            {
                if (restartTime > 3)
                {
                    Debugger.LogError("post request timeout！");
                    if (callBack != null) callBack("TIMEOUT");
                }
                else
                {
                    StartCoroutine(requestPostAsync(url, postDataStr, callBack, ++restartTime));
                }
                yield break;
            }

            if (www.error != null)
            {
                Debugger.LogError(www.error);
                if (callBack != null) callBack("ERROR");
                yield break;
            }

            if (callBack != null) callBack(www.text);
        }
    }

    public void HttpRequestSync(string url, string body, Action<string> callback = null, int syncTimes = 0)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
        request.ProtocolVersion = HttpVersion.Version11;
        request.AutomaticDecompression = DecompressionMethods.None;
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.Timeout = 5 * 1000;
        request.ReadWriteTimeout = 5 * 1000;
        request.KeepAlive = false;
        request.Proxy = null;
        try
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    if (syncTimes >= 3)
                        Loom.DispatchToMainThread(() => MessageBox.Show(LanguageTips.HTTP_ERROR));
                    else
                        HttpRequestSync(url, body, callback, ++syncTimes);
                    return;
                }
                StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
                string result = sr.ReadToEnd().Trim();
                Loom.DispatchToMainThread(() =>
                {
                    if (callback != null) callback(result);
                });
                sr.Close();
                responseStream.Close();
            }
        }
        catch (Exception e)
        {
            Loom.DispatchToMainThread(()=> Debug.LogException(e));
            if (syncTimes >= 3)
                Loom.DispatchToMainThread(() => MessageBox.Show(LanguageTips.HTTP_ERROR));
            else
                HttpRequestSync(url, body, callback, ++syncTimes);
        }
    }


    public void HttpRequestGet(string body, Action<string> callback = null)
    {
        string newUrl = string.Concat(url, "?", body);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(newUrl));
        request.Method = "GET";
        request.ContentType = "application/x-www-form-urlencoded";
        request.Timeout = 5 * 1000;
        request.ReadWriteTimeout = 5 * 1000;

        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    Loom.DispatchToMainThread(() =>
                    {
                        if (callback != null) callback("");
                    });
                    return;
                }
                StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
                string result = sr.ReadToEnd().Trim();
                Loom.DispatchToMainThread(() =>
                {
                    if (callback != null) callback(result);
                });
                sr.Close();
                responseStream.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 内测地址
    /// </summary>
    private string url = "http://xyfyapi.firedg.com/api.php";
    /// <summary>
    /// cdn访问地址
    /// </summary>
    private string cdnUrl = "http://xyfyapi.firedg.com/cdnApi.php";

    private StringBuilder sb = new StringBuilder();
    /******************************************************************************************/
    #region private
    /// <summary>
    /// 游戏ID
    /// </summary>
    private int appID = 74;
    /// <summary>
    /// 子渠道ID
    /// </summary>
    private int channelID = 0;
    /// <summary>
    /// 平台帐号
    /// </summary>
    private string userID = string.Empty;
    /// <summary>
    /// token
    /// </summary>
    private string token = string.Empty;
    /// <summary>
    /// 手机设备号（获取不到就为0）
    /// </summary>
    private string imeiOrIdfa = string.Empty;
    /// <summary>
    /// login
    /// </summary>
    private string method = string.Empty;
    /// <summary>
    /// 渠道ID
    /// </summary>
    private int pid = 0;
    /// <summary>
    /// 签名，详见首页签名规则
    /// sign组装格式：MD5(pid + method + "firedg!@#51000-+Bylelong")
    /// </summary>
    private string sign = string.Empty;
    /// <summary>
    /// 时间戳
    /// </summary>
    private string time = string.Empty;
    /// <summary>
    /// 平台帐号
    /// </summary>
    private string accountName = string.Empty;
    /// <summary>
    /// 最近登录的服务器，最多记录5个
    /// </summary>
    private ArrayList lastServer;
    /// <summary>
    /// 服务器列表
    /// </summary>
    private ArrayList serverList;
    /// <summary>
    /// 玩家信息列表
    /// </summary>
    private ArrayList roleInfoList;
    /// <summary>
    /// 是否显示account,默认为true
    /// </summary>
    private bool showAccount = true;
    /// <summary>
    /// 是否显示完整版本号,默认为true
    /// </summary>
    private bool versionView = true;
    /// <summary>
    /// 备用，coolpad渠道支付需要的授权令牌参数
    /// </summary>
    private string access_token = string.Empty;
    /// <summary>
    /// 平台帐号
    /// </summary>
    private string accName;
    /// <summary>
    /// 渠道ID
    /// </summary>
    private int channel_id;
    /// <summary>
    /// 服务器ID
    /// </summary>
    private int sid = 0;
    /// <summary>
    /// 服务器名称
    /// </summary>
    private string serverName = string.Empty;
    /// <summary>
    /// 物理地址
    /// </summary>
    private string mac;
    /// <summary>
    /// 版本号
    /// </summary>
    private string version = string.Empty;
    /// <summary>
    /// cdn资源更新地址
    /// </summary>
    private string packageUrl = string.Empty;
    /// <summary>
    /// ip
    /// </summary>
    private string ip = string.Empty;

    /// <summary>
    /// 操作系统
    /// </summary>
    private string os {
        get
        {
            string value = string.Empty;
#if UNITY_IOS
            value = "ios";
#elif UNITY_ANDROID
            value = "android";
#endif
            return value;
        }
    }
    /// <summary>
    /// 官网按钮状态
    /// </summary>
    private int guanwangBtnState = 1;
    /// <summary>
    ///
    /// </summary>
    private int kefuBtnState = 1;
    /// <summary>
    /// 是否白名单用户
    /// </summary>
    private bool isWhite = true;
    /// <summary>
    /// 游戏签名，小7SDK支付用到
    /// </summary>
    private string gameSign = string.Empty;
    /// <summary>
    /// 游戏ID，小7SDK支付用到
    /// </summary>
    private string gameGuid = string.Empty;
        
    #endregion

    /******************************************************************************************/
    #region public
    /// <summary>
    /// 游戏ID
    /// </summary>
    public int AppID {
        get{ return appID; }
        set{ appID = value; }
    }
    /// <summary>
    /// 平台ID
    /// </summary>
    public int ChannelID
    {
        get{return channelID;}
        set{channelID = value;}
    }
    /// <summary>
    /// 平台帐号
    /// </summary>
    public string UserID
    {
        get{return userID;}
        set{userID = value;}
    }
    /// <summary>
    /// token
    /// </summary>
    public string Token
    {
        get{return token;}

        set{ token = value;}
    }
    /// <summary>
    /// 手机设备号（获取不到就为0）
    /// </summary>
    public string ImeiOrIdfa{
        get{
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
    /// <summary>
    /// login
    /// </summary>
    public string Method
    {
        get{return method;}
        set{method = value;}
    }
    /// <summary>
    /// 平台ID
    /// </summary>
    public int Pid
    {
        get{ return pid;}
        set{pid = value;}
    }
    /// <summary>
    /// 签名，详见首页签名规则
    /// sign组装格式：MD5(pid + method + "firedg!@#51000-+Bylelong")
    /// </summary>
    public string Sign{
        get{
            return StringToMD5(string.Format("{0}{1}firedg!@#51000-+Bylelong", Pid, Method));
        }
    }
    /// <summary>
    /// IOS专用 用于判断当前包是否出于提审状态 0-不是，1-是
    /// </summary>
    public int Checking
    {
        get; set;
    }

    /// <summary>
    /// 时间戳
    /// </summary>
    public string Time
    {
        get{return time;}
        set{ time = value;}
    }
    /// <summary>
    /// 平台帐号
    /// </summary>
    public string AccountName
    {
        get{return accountName;}
        set{accountName = value;}
    }
    /// <summary>
    /// 最近登录的服务器，最多记录5个
    /// </summary>
    public ArrayList LastServer
    {
        get{return lastServer;}
        set{lastServer = value;}
    }
    /// <summary>
    /// 服务器列表
    /// </summary>
    public ArrayList ServerList
    {
        get{ return serverList;}
        set{serverList = value;}
    }
    /// <summary>
    /// 玩家信息列表
    /// </summary>
    public ArrayList RoleInfoList
    {
        get { return roleInfoList; }
        set { roleInfoList = value; }
    }
    /// <summary>
    /// 是否显示account,默认为true
    /// </summary>
    public bool ShowAccount
    {
        get{return showAccount;}
        set{showAccount = value;}
    }
    /// <summary>
    /// 是否显示完整版本号,默认为true
    /// </summary>
    public bool VersionView
    {
        get{return versionView;}
        set{versionView = value;}
    }
    /// <summary>
    /// 备用，coolpad渠道支付需要的授权令牌参数
    /// </summary>
    public string Access_token
    {
        get{return access_token;}
        set{access_token = value;}
    }
    /// <summary>
    /// 平台账号
    /// </summary>
    public string AccName
    {
        get { return accName; }
        set { accName = value; }
    }
    /// <summary>
    /// 渠道ID
    /// </summary>
    public int Channel_id
    {
        get { return channel_id; }
        set { channel_id = value; }
    }
    /// <summary>
    /// 服务器ID
    /// </summary>
    public int Sid
    {
        get { return sid; }
        set { sid = value; }
    }

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName
    {
        get { return serverName; }
        set { serverName = value; }
    }
    /// <summary>
    /// 物理地址
    /// </summary>
    public string Mac{
        get { return mac; }
        set { mac = value; }
    }
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version
    {
        get { return version; }
        set { version = value; }
    }
    /// <summary>
    /// cdn资源更新地址
    /// </summary>
    public string PackageUrl {
        get { return packageUrl; }
        set { packageUrl = value; }
    }
    /// <summary>
    /// Ip
    /// </summary>
    public string Ip
    {
        get { return ip; }
        set { ip = value; }
    }
    /// <summary>
    /// 官网按钮状态
    /// </summary>
    public int GuanwangBtnState
    {
        get { return guanwangBtnState; }
        set { guanwangBtnState = value; }
    }
    /// <summary>
    /// 客服按钮状态
    /// </summary>
    public int KefuBtnState
    {
        get { return kefuBtnState; }
        set { kefuBtnState = value; }
    }
    /// <summary>
    /// 是否白名单用户
    /// </summary>
    public bool IsWhite
    {
        get { return isWhite; }
        set
        {
            isWhite = value;
#if !UNITY_IOS
            Debugger.useLog = value;
#endif
        }
    }
    /// <summary>
    /// 游戏签名，小7SDK支付用到
    /// </summary>
    public string GameSign
    {
        get { return gameSign; }
        set { gameSign = value; }
    }

    /// <summary>
    /// 游戏ID，小7SDK支付用到
    /// </summary>
    public string GameGuid
    {
        get { return gameGuid; }
        set { gameGuid = value; }
    }

#endregion
    /// <summary>
    /// 字符串转MD5加密
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string StringToMD5(string str)
    {
        byte[] data = Encoding.UTF8.GetBytes(str);
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] outBytes = md5.ComputeHash(data);
        string outString = "";
        for (int i = 0; i < outBytes.Length; i++)
        {
            outString += outBytes[i].ToString("x2");
        }
        return outString;
    }

    /// <summary>
    /// 用户登录接口
    /// </summary>
    public void LoginRequest(Action callback)
    {
        sb.Clear();
        sb.AppendFormat("appID={0}&", appID);
        sb.AppendFormat("channelID={0}&", ChannelID);
        sb.AppendFormat("userID={0}&", UserID);
        sb.AppendFormat("token={0}&", Token);
        sb.AppendFormat("imeiOrIdfa={0}&", ImeiOrIdfa);
        sb.Append("method=login&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}&", Sign);
#if UNITY_IOS
        sb.AppendFormat("isIOS=1&");
#else
        sb.AppendFormat("isIOS=0&");
#endif
        sb.AppendFormat("checking={0}", Checking);

        string paramStr = sb.ToString();
        Debugger.Log("用户登录接口: {0}", paramStr);
        HttpRequest(paramStr, (result) => {
            LoginCallback(result);
            callback();
        });
    }

    /// <summary>
    /// 用户登录接口回调
    /// </summary>
    /// <param name="result"></param>
    private void LoginCallback(string result)
    {
        if (result == "TIMEOUT" || result == "ERROR")
        {
            MessageBox.Show("访问网络超时或出错");
            return;
        }
        object jsonParsed = MiniJSON.Json.Deserialize(result);
        if (jsonParsed != null)
        {
            Hashtable jsonMap = jsonParsed as Hashtable;
            if (jsonMap.ContainsKey("errorCode"))
            {
                int errorCode = Int32.Parse(jsonMap["errorCode"].ToString());
                if (errorCode != 0)
                {
                    Debugger.LogError("errorCode: {0}", errorCode);
                    MessageBox.Show(ErrorCodeText(errorCode));
                    return;
                }
            }
            if (jsonMap.ContainsKey("data"))
            {
                Hashtable dataDict = jsonMap["data"] as Hashtable;
                if (dataDict.ContainsKey("time"))
                {
                    time = dataDict["time"].ToString();
                }
                if (dataDict.ContainsKey("token"))
                {
                    token = dataDict["token"].ToString();
                }
                if (dataDict.ContainsKey("accountName"))
                {
                    accountName = dataDict["accountName"].ToString();
                }
                if (dataDict.ContainsKey("roleInfoList"))
                {
                    if (User_Config.roleInfoList != null)
                        User_Config.roleInfoList.Clear();
                    roleInfoList = dataDict["roleInfoList"] as ArrayList;
                    User_Config.SetRoleInfoList(roleInfoList);
                }
                if (dataDict.ContainsKey("lastServer"))
                {
                    if (User_Config.lastServerList != null)
                        User_Config.lastServerList.Clear();
                    lastServer = dataDict["lastServer"] as ArrayList;
                    User_Config.SetLastServerList(lastServer);
                    User_Config.SetServerListLevel(User_Config.lastServerList);
                }
                if (dataDict.ContainsKey("serverList"))
                {
                    if (User_Config.serverList != null)
                        User_Config.serverList.Clear();
                    serverList = dataDict["serverList"] as ArrayList;
                    User_Config.SetServerList(serverList);
                    User_Config.SetServerListLevel(User_Config.serverList);
                }
                if (dataDict.ContainsKey("showAccount"))
                {
                    showAccount = bool.Parse(dataDict["showAccount"].ToString());
                }
                if (dataDict.ContainsKey("versionView"))
                {
                    versionView = bool.Parse(dataDict["versionView"].ToString());
                }
                if (dataDict.ContainsKey("access_token"))
                {
                    access_token = dataDict["access_token"].ToString();
                }
                if (dataDict.ContainsKey("isWhite"))
                {
                    IsWhite = bool.Parse(dataDict["isWhite"].ToString());
                }
            }
            Debugger.Log("用户登录接口回调: {0}", result);
        }
        else
        {
            Debugger.LogError("Login callback json parse error!");
        }
    }

    /// <summary>
    /// 登录打点接口
    /// </summary>
    /// <param name="stepFlag">打点标识(1-注册账号 2-登录 3-区服选择 4-创建角色 5-loading 6-进入游戏 7-新手引导)</param>
    public void StepLogRequest(int stepFlag)
    {
        if (stepFlag == 5 || stepFlag == 6 || stepFlag == 7) return;
        
        sb.Clear();
        sb.AppendFormat("accName={0}&", AccName);
        sb.AppendFormat("sid={0}&", Sid);
        sb.AppendFormat("channel_id={0}&", Channel_id);
        sb.AppendFormat("stepFlag={0}&", stepFlag);
        sb.Append("method=clientStepLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}&", Sign);
        sb.AppendFormat("appid={0}", AppID);
        
        string paramStr = sb.ToString();
        Debugger.Log("登录打点接口: {0}", paramStr);
#if UNITY_IOS
        HttpRequest(paramStr, null, false);
#else
        HttpRequest(paramStr, null, true);
#endif
    }

    /// <summary>
    /// 登录日志接口
    /// </summary>
    /// <param name="nickname">角色昵称</param>
    public void LoginInfoLog(string nickname)
    {
        sb.Clear();
        sb.AppendFormat("accName={0}&", AccName);
        sb.AppendFormat("sid={0}&", Sid);
        sb.AppendFormat("mac={0}&", Mac);
        sb.AppendFormat("imeiOrIdfa={0}&", ImeiOrIdfa);
        sb.AppendFormat("channel_id={0}&", Channel_id);
        sb.AppendFormat("nickname={0}&", nickname);
        sb.Append("method=loginInfoLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}", Sign);

        string paramStr = sb.ToString();
        Debugger.Log("登录日志接口: {0}", paramStr);
        HttpRequest(paramStr, null);
    }

    /// <summary>
    /// 获取公告信息
    /// </summary>
    /// <param name="callback">公告回调</param>
    public void GetNoticeInfo(Action<string> callback)
    {
        sb.Clear();
        sb.Append("method=getNoticeInfo&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}&", Sign);
        sb.AppendFormat("appid={0}", AppID);

        string paramStr = sb.ToString();
        Debugger.Log("公告参数：" + paramStr);
        HttpRequest(paramStr, (result) => {
            Debugger.Log("获取公告信息: {0}", result);
            try
            {
                object jsonParsed = MiniJSON.Json.Deserialize(result);
                if (jsonParsed != null)
                {
                    Hashtable jsonMap = jsonParsed as Hashtable;
                    if (jsonMap.ContainsKey("errorCode"))
                    {
                        int errorCode = Int32.Parse(jsonMap["errorCode"].ToString());
                        if (errorCode != 0)
                        {
                            Debugger.LogError("errorCode: " + errorCode);
                            callback("");
                            return;
                        }
                    }
                    if (jsonMap.ContainsKey("data"))
                    {
                        Hashtable ht1 = jsonMap["data"] as Hashtable;
                        if (ht1.ContainsKey("data"))
                        {
                                string noticeInfo = MiniJSON.Json.Serialize(ht1["data"]);
                                callback(noticeInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                callback(string.Empty);
            }
        });
    }

    /// <summary>
    /// 聊天监控接口
    /// </summary>
    /// <param name="nickName">昵称</param>
    /// <param name="roleId">角色ID</param>
    /// <param name="msg">聊天内容</param>
    public void ChatMonitor(string nickName, string roleId, string msg)
    {
        if (User_Config.use_dadian == 0) return;
        try {
            sb.Clear();
            sb.AppendFormat("accName={0}&", AccName);
            sb.AppendFormat("sid={0}&", Sid);
            sb.AppendFormat("nickname={0}&", nickName);
            sb.AppendFormat("roleid={0}&", roleId);
            sb.AppendFormat("msg={0}&", msg);
            sb.Append("method=chatMonitor&");
            sb.AppendFormat("pid={0}&", Pid);
            sb.AppendFormat("sign={0}", Sign);
            
            string paramStr = sb.ToString();
            Debugger.Log("聊天监控接口: {0}", paramStr);
            HttpRequest(paramStr, (result) =>
            {
            }, true);
        }
        catch (Exception ex)
        {
            Debugger.Log(ex.Message);
        }
    }

    /// <summary>
    /// 设置最后登录服务器
    /// </summary>
    public void SetLastServer()
    {
        sb.Clear();
        sb.AppendFormat("accName={0}&", AccName);
        sb.AppendFormat("sid={0}&", Sid);
        sb.Append("method=setLastServer&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}&", Sign);
        sb.AppendFormat("timestamp={0}", ConvertDateTimeInt(DateTime.Now));

        string paramStr = sb.ToString();
        Debugger.Log("设置最后登录服务器: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
        });
    }

    /// <summary>
    /// 获取cdn数据
    /// </summary>
    /// <param name="callback">方法回调。 第一个参数：version, 第二个参数：packageUrl</param>
    public void GetCdnData(string curVersion, Action<string, string> callback)
    {
        sb.Clear();
        sb.AppendFormat("version={0}&", curVersion);
        sb.AppendFormat("os={0}&", os);
        sb.AppendFormat("imei={0}&", ImeiOrIdfa);
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("appid={0}", AppID);

        string paramStr = sb.ToString();
        Debugger.Log("获取cdn数据: {0}, 请求cdnUrl: {1}", paramStr, cdnUrl);
        HttpPostAsync(cdnUrl, paramStr, (result) =>
        {
            object jsonParsed = MiniJSON.Json.Deserialize(result);
            if (jsonParsed != null)
            {
                Hashtable jsonMap = jsonParsed as Hashtable;
                if (jsonMap.ContainsKey("version"))
                {
                    version = jsonMap["version"].ToString();
                }
                if (jsonMap.ContainsKey("packageUrl"))
                {
                    packageUrl = jsonMap["packageUrl"].ToString();
                }
                Debugger.Log("cdn返回: {0}&{1}", version, packageUrl);
            }
            callback(version, packageUrl);
        });
    }
    /// <summary>
    /// 创建角色信息接口
    /// </summary>
    /// <param name="name">昵称</param>
    /// <param name="level">等级</param>
    /// <param name="roleId">角色Id</param>
    /// <param name="vip">vip</param>
    public void CreateRoleInfo(string name, int level, string roleId, int vip)
    {
        sb.Clear();
        sb.AppendFormat("regTime={0}&", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendFormat("accounts={0}&", UserID);
        sb.AppendFormat("nickName={0}&", StringToUnicode(name));
        sb.AppendFormat("roleLevel={0}&", level);
        sb.Append("method=roleInfoLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("role_id={0}&", roleId);
        sb.AppendFormat("serverId={0}&", Sid);
        sb.AppendFormat("ip={0}&", Ip);
        sb.AppendFormat("vipLevel={0}&", vip);
        sb.AppendFormat("sign={0}&", Sign);
        sb.AppendFormat("appid={0}&", AppID);
        sb.AppendFormat("timestamp={0}", ConvertDateTimeInt(DateTime.Now));

        string paramStr = sb.ToString();
        Debugger.Log("创建角色信息接口: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
        });
    }

    /// <summary>
    /// 升级记录接口
    /// </summary>
    /// <param name="name">昵称</param>
    /// <param name="level">等级</param>
    public void UpgradeInfo(string name, int level)
    {
        if (User_Config.use_dadian == 0) return;

        sb.Clear();
        sb.AppendFormat("accounts={0}&", UserID);
        sb.AppendFormat("nickName={0}&", name);
        sb.AppendFormat("roleLevel={0}&", level);
        sb.AppendFormat("serverId={0}&", Sid);
        sb.AppendFormat("updateTime={0}&", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.Append("method=upgradeInfoLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}", Sign);
        
        string paramStr = sb.ToString();
        Debugger.Log("升级记录接口: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
        } , true);
    }
    /// <summary>
    /// 更新角色信息接口
    /// </summary>
    /// <param name="name">昵称</param>
    /// <param name="level">等级</param>
    /// <param name="vip">vip</param>
    /// <param name="newName">新角色昵称</param>
    public void UpdateRoleInfo(string name, int level, int vip, string newName)
    {
        sb.Clear();
        sb.AppendFormat("accounts={0}&", UserID);
        sb.AppendFormat("nickName={0}&", name);
        sb.AppendFormat("roleLevel={0}&", level);
        sb.AppendFormat("serverId={0}&", Sid);
        sb.AppendFormat("vipLv={0}&", vip);
        sb.AppendFormat("newNickName={0}&", newName);
        sb.Append("method=updateRoleInfoLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}", Sign);

        string paramStr = sb.ToString();
        Debugger.Log("更新角色信息接口: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
        } , true);
    }
    [LuaInterface.NoToLua]
    /// <summary>
    /// 用户退出时记录在线时长接口
    /// </summary>
    public void UpdateLogin()
    {
        sb.Clear();
        sb.AppendFormat("accounts={0}&", UserID);
        sb.AppendFormat("serverId={0}&", Sid);
        sb.AppendFormat("onlineSecs={0}&", Mathf.CeilToInt(UnityEngine.Time.realtimeSinceStartup));
        sb.Append("method=updateLoginLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}", Sign);

        string paramStr = sb.ToString();
        Debugger.Log("用户退出时记录在线时长接口: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
        });
    }

    /// <summary>
    /// 公告的签名
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="serverNo"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string SignGM(string userId, int serverNo, int time)
    {
        return StringToMD5(string.Format("game={0}userid={1}serverno={2}time={3}key=8qld2rmd1d2zjc8u", User_Config.logoName, userId , serverNo,time));
    }
    /// <summary>
    /// 控制客服按钮是否屏蔽
    /// </summary>
    public void ButtonControll()
    {
        sb.Clear();
        sb.Append("method=buttonControll&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("serverId={0}&", Sid);
        sb.AppendFormat("sign={0}&", Sign);
        sb.AppendFormat("appid={0}", AppID);

        string paramStr = sb.ToString();
        Debugger.Log("控制客服按钮是否屏蔽: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
            Debugger.Log("控制客服按钮是否屏蔽: {0}", result);
            object jsonParsed = MiniJSON.Json.Deserialize(result);
            if (jsonParsed != null)
            {
                Hashtable jsonMap = jsonParsed as Hashtable;
                if (jsonMap.ContainsKey("errorCode"))
                {
                    int errorCode = Int32.Parse(jsonMap["errorCode"].ToString());
                    if (errorCode != 0)
                    {
                        Debugger.LogError("errorCode: " + errorCode);
                        return;
                    }
                }
                if (jsonMap.ContainsKey("data"))
                {
                    Hashtable dataDict = jsonMap["data"] as Hashtable;
                    if (dataDict.ContainsKey("guanwang"))
                    {
                        guanwangBtnState = Int32.Parse(dataDict["guanwang"].ToString());
                    }
                    if (dataDict.ContainsKey("kefu"))
                    {
                        kefuBtnState = Int32.Parse(dataDict["kefu"].ToString());
                    }
                }
            }
        });
    }

    /// <summary>
    /// 下订单接口
    /// </summary>
    /// <param name="itemId">商品Id</param>
    /// <param name="roleId">角色Id</param>
    /// <param name="roleLevel">角色等级</param>
    /// <param name="callback">订单回调</param>
    public void AddOrderInfo(int itemId, string roleId, int roleLevel, Action<string> callback)
    {
        sb.Clear();
        sb.AppendFormat("itemId={0}&", itemId);
        sb.AppendFormat("role_id={0}&", roleId);
        sb.AppendFormat("accounts={0}&", UserID);
        sb.AppendFormat("serverId={0}&", Sid);
        sb.AppendFormat("roleLevel={0}&", roleLevel);
        sb.AppendFormat("gameQn={0}&", "");
        sb.AppendFormat("channel_id={0}&", Channel_id);
        sb.Append("method=addOrderInfoLog&");
        sb.AppendFormat("pid={0}&", Pid);
        sb.AppendFormat("sign={0}&", Sign);
        sb.AppendFormat("timestamp={0}", ConvertDateTimeInt(DateTime.Now));

        string paramStr = sb.ToString();
        Debugger.Log("下订单接口: {0}", paramStr);
        HttpRequest(paramStr, (result) =>
        {
            Debugger.Log("下订单接口返回: {0}", result);
            object jsonParsed = MiniJSON.Json.Deserialize(result);
            if (jsonParsed != null)
            {
                Hashtable jsonMap = jsonParsed as Hashtable;
                if (jsonMap.ContainsKey("errorCode"))
                {
                    int errorCode = Int32.Parse(jsonMap["errorCode"].ToString());
                    if (errorCode != 0)
                    {
                        Debugger.LogError("errorCode: " + errorCode);
                        callback("");
                        return;
                    }
                }
                if (jsonMap.ContainsKey("data"))
                {
                    Hashtable ht1 = jsonMap["data"] as Hashtable;
                    if (ht1.ContainsKey("cpOrderId"))
                    {
                        string orderId = ht1["cpOrderId"].ToString();
                        callback(orderId);
                    }
                    if (ht1.ContainsKey("payData"))
                    {
                        Hashtable ht2 = ht1["payData"] as Hashtable;
                        if (ht2.ContainsKey("game_sign"))
                        {
                            gameSign = ht2["game_sign"].ToString();
                        }
                        if (ht2.ContainsKey("game_guid"))
                        {
                            gameGuid = ht2["game_guid"].ToString();
                        }
                    }
                }
            }
        });
    }

    string StringToUnicode(string str)
    {
        var bytes = Encoding.Unicode.GetBytes(str);
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < bytes.Length; i += 2)
        {
            stringBuilder.AppendFormat("\\u{0:x2}{1:x2}", bytes[i + 1], bytes[i]);
        }
        return stringBuilder.ToString();
    }

    public static string UrlEncode(string str)
    {
        StringBuilder sb = new StringBuilder();
        byte[] byStr = Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
        for (int i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }

        return (sb.ToString());
    }

    /// <summary>
    /// DateTime时间格式转换为10位不带毫秒的Unix时间戳
    /// </summary>
    /// <param name="time"> DateTime时间格式</param>
    /// <returns>Unix时间戳格式</returns>
    private int ConvertDateTimeInt(DateTime time)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        return (int)(time - startTime).TotalSeconds;
    }

    private string ErrorCodeText(int error)
    {
        string errorString = "";
        if (error == 0)
        {
            errorString = LanguageTips.ERROR_CODE_0;
        }
        else if (error == 1)
        {
            errorString = LanguageTips.ERROR_CODE_1;
        }
        else if (error == 2)
        {
            errorString = LanguageTips.ERROR_CODE_2;
        }
        else if (error == 3)
        {
            errorString = LanguageTips.ERROR_CODE_3;
        }
        else if (error == 4)
        {
            errorString = LanguageTips.ERROR_CODE_4;
        }
        else if (error == 5)
        {
            errorString = LanguageTips.ERROR_CODE_5;
        }
        else if (error == 6)
        {
            errorString = LanguageTips.ERROR_CODE_6;
        }
        else if (error == 7)
        {
            errorString = LanguageTips.ERROR_CODE_7;
        }
        else if (error == 8)
        {
            errorString = LanguageTips.ERROR_CODE_8;
        }
        else if (error == 9)
        {
            errorString = LanguageTips.ERROR_CODE_9;
        }
        else if (error == 10)
        {
            errorString = LanguageTips.ERROR_CODE_10;
        }
        else if (error == 11)
        {
            errorString = LanguageTips.ERROR_CODE_11;
        }
        else if (error == 12)
        {
            errorString = LanguageTips.ERROR_CODE_12;
        }
        else if (error == 13)
        {
            errorString = LanguageTips.ERROR_CODE_13;
        }
        else if (error == 14)
        {
            errorString = LanguageTips.ERROR_CODE_14;
        }
        else if (error == 15)
        {
            errorString = LanguageTips.ERROR_CODE_15;
        }
        else if (error == 16)
        {
            errorString = LanguageTips.ERROR_CODE_16;
        }
        else if (error == 17)
        {
            errorString = LanguageTips.ERROR_CODE_17;
        }
        else if (error == 18)
        {
            errorString = LanguageTips.ERROR_CODE_18;
        }
        else if (error == 19)
        {
            errorString = LanguageTips.ERROR_CODE_19;
        }
        else if (error == 20)
        {
            errorString = LanguageTips.ERROR_CODE_20;
        }
        return string.Format("{0}, ErrorCode:{1}", errorString, error);
    }
}
