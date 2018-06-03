using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Config;
using AL.Crypto;
using LuaFramework;
using LuaInterface;

public sealed class ChatVoiceService : MonoBehaviour {

    public AudioSource _audioSource;

    public static ChatVoiceService Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ChatVoiceService");
                go.transform.position = Vector3.zero;
                go.transform.localScale = Vector3.one;
                _instance = go.AddComponent<ChatVoiceService>();
            }
            return _instance;
        }
    }

    private static ChatVoiceService _instance;

    public int curShowPanel;            //聊天界面当前显示的panel
    public Transform chatCanvas;        //聊天画布
    public Transform chatWnd;           //聊天左下角小窗
    public Transform chatUi;            //聊天界面

    private static MicroPhoneInput _phoneInput;         //手机录音设备
    private int _curSoundTime;          //本次发言时长

    //private float[] _lastMsgSendTimes = { 0, 0, 0, 0, 0 };

    public int curPrivateCharId { get; set; }       //当前私聊对象id
    public string curPrivateCharUid { get; set; }      //当前私聊对象uid

    private string _lastYuYinStr;
    MicroPhoneInput phoneInputInstance { get { return _phoneInput; } }

    private List<KeyValuePair<string, string>> _yuyinList = new List<KeyValuePair<string, string>>();

    void Awake ()
    {
        DontDestroyOnLoad(gameObject);

        _audioSource = transform.GetOrAddComponent<AudioSource>();
        _phoneInput = MicroPhoneInput.getInstance(gameObject);
    }

    /// <summary>
    /// 语音识别
    /// </summary>
    /// <param name="sourceData"></param>
    /// <returns></returns>
    public string GetSoundRecogonizeString(Byte[] sourceData)
    {
        string ret = "";

        return ret;
    }

    #region 语音处理 
    private string _curSn;

    /////////////////////////////////////////////////////////////////////////
    ///录音
    /// <summary>
    /// 开始录音
    /// </summary>
    public void StartRecordSound()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_phoneInput != null) _phoneInput.CheckPermission();
#else
        RecordBegin();
#endif
    }

    private void RecordBegin()
    {
        try
        {
            _curSn = CreateSnByNow();

            _phoneInput.StartRecord(_curSn);
            LuaHelper.GetSoundManager().CloseSound(true);
        }
        catch (Exception e)
        {
            Debugger.LogException(e);
        }
    }

    /// <summary>
    /// 结束录音
    /// </summary>
    /// <param name="isSend"></param> 是否发送
    public void EndRecordSound(bool isSend)
    {
        _curSoundTime = _phoneInput.StopRecord();

        if (_curSoundTime <= 1)
        {
            Util.CallMethod("CHATLOGIC", "PopUpVoiceLengthTooShort");
        }
        else if (isSend)
        {
            if (User_Config.soundIp == "")
                return;

            if (_phoneInput.GetClipData() == null)
                return;

            StartCoroutine(DealWithSound());
        }
        LuaHelper.GetSoundManager().CloseSound(false);
    }


    /// <summary>
    ///  获取音量大小
    /// </summary>
    /// <returns></returns>
    public float GetVoice()
    {
        if (_phoneInput == null) return 0;
        return _phoneInput.GetAveragedVolume() * _phoneInput.sensitivity;
    }

    /// <summary>
    ///  www的返回值
    /// </summary>
    private string regRet;      //识别返回字符串
    private string sendRet;     //上传返回字符串
    private string getRet;      //获取返回字符串

    /// <summary>
    ///  处理语音
    /// </summary>
    private IEnumerator DealWithSound()
    {
        regRet = string.Empty;
        sendRet = string.Empty;
        string sn = _curSn;

        yield return StartCoroutine(SendHttpToSoundServer(sn, _phoneInput.idx, Convert.ToBase64String(_phoneInput.GetFinalClipData()).Replace("+", "%2B")));
        //yield return StartCoroutine(SoundRecognitionWWW(data));
        //{"corpus_no":"6141520439644542158","err_msg":"success.","err_no":0,"result":["彭老师，"],"sn":"590051594681429934156"}
        Hashtable tbl = MiniJSON.Json.Deserialize(regRet) as Hashtable;
        //yield return StartCoroutine(SendHttpToSoundServer(sn, _phoneInput.idx, Convert.ToBase64String(_phoneInput.GetFinalClipData()).Replace("+", "%2B")));
        if (!string.IsNullOrEmpty(sendRet))
        {
            string[] retArr = sendRet.Split('@');
            if (retArr != null && retArr.Length == 2 && retArr[0].Equals("1"))
            {
                if (tbl != null && tbl["err_msg"].ToString().Contains("success"))
                {
                    string resultStr = (tbl["result"] as ArrayList)[0].ToString();
                    resultStr = resultStr.Substring(0, resultStr.Length - 1);
                    Util.CallMethod("CHATLOGIC", "SendVoiceMsg", resultStr, retArr[1], _curSoundTime);
                }
                else
                {
                    Util.CallMethod("CHATLOGIC", "SendVoiceMsg", "", retArr[1], _curSoundTime);
                }
            }
            else
            {
                Debugger.Log("上传语音服务器失败！！！sendRet = {0}", sendRet);
                Util.CallMethod("CHATLOGIC", "PopUploadError");
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////
    ///点击图标播放
    ///

    private GameObject _curClickYuYinGo;

    /// <summary>
    /// 播放语音
    /// </summary>
    /// <param name="go"></param>
    public void YuYinClick(GameObject go, int voiceTime)
    {
        if (go == null)
            return;

        _curClickYuYinGo = go.transform.GetChild(0).gameObject;
        //var animate = go.GetComponent<AnimateTexture>();
        //if (animate != null) animate.start = true;

        //本地列表查找
        string data = GetYuYinDataBySn(_curClickYuYinGo.name);

        if (data == "")
        {
            if (string.IsNullOrEmpty(User_Config.soundIp))
                return;

            StartCoroutine(YuYinClickDeal(voiceTime));
        }
        else
        {
            PlayYuYin(_curClickYuYinGo, data, voiceTime);
        }
    }

    /// <summary>
    /// 从语音服务器获取语音并播放
    /// </summary>
    IEnumerator YuYinClickDeal(int voiceTime)
    {
        yield return StartCoroutine(GetSoundFromSoundServer(_curClickYuYinGo.name));

        if (!string.IsNullOrEmpty(getRet))
        {
            //添加到本地语音列表
            AddYuYinData(_curClickYuYinGo.name, getRet);
            //播放
            PlayYuYin(_curClickYuYinGo, getRet, voiceTime);
        }
    }

    /// <summary>
    /// 播放语音
    /// </summary>
    /// <param name="data"></param>
    private void PlayYuYin(GameObject go, string data, int voiceTime)
    {
        LuaHelper.GetSoundManager().CloseSound(true); 
        _phoneInput.Play64String(data, voiceTime, () => 
        {
            //var animate = go.GetComponentInParent<AnimateTexture>();
            //if (animate != null) animate.start = false;
            Util.CallMethod("CHATLOGIC", "VoicePlayFinished");
            LuaHelper.GetSoundManager().CloseSound(false);
        });
    }

    /// <summary>
    /// 停止语音播放
    /// </summary>
    public void StopPlayYuyin()
    {
        LuaHelper.GetSoundManager().CloseSound(false);
        _phoneInput.StopPlayCurrentVoice();
        if(_curClickYuYinGo != null)
        {
            //var animate = _curClickYuYinGo.GetComponentInParent<AnimateTexture>();
            //if (animate != null) animate.start = false;
        }
    }

    /// <summary>
    /// 保存本地语音
    /// </summary>
    /// <param name="sn"></param>
    /// <param name="data"></param>
    void AddYuYinData(string sn, string data)
    {
        _yuyinList.Add(new KeyValuePair<string, string>(sn, data));
    }

    /// <summary>
    /// 移出语音
    /// </summary>
    /// <param name="sn"></param>
    public void RemoveYuYinData(string sn)
    {
        KeyValuePair<string, string> removeItem = new KeyValuePair<string, string>();
        bool has = false;
        int len = _yuyinList.Count;
        for (int i = 0; i < len; i++)
        {
            KeyValuePair<string, string> item = _yuyinList[i];
            if (item.Key == sn)
            {
                removeItem = item;
                has = true;
                break;
            }
        }

        if (has) _yuyinList.Remove(removeItem);
    }

    /// <summary>
    ///  根据sn在本地语音列表中查找
    /// </summary>
    /// <param name="sn"></param>
    /// <returns></returns>
    public string GetYuYinDataBySn(string sn)
    {
        string ret = "";
        int len = _yuyinList.Count;
        for (int i = 0; i < len; i++)
        {
            KeyValuePair<string, string> item = _yuyinList[i];
            if (item.Key == sn)
            {
                ret = item.Value;
                break;
            }
        }

        return ret;
    }
    #endregion


    #region 语音服务器
    /// <summary>
    /// 上传语音
    /// </summary>
    /// <param name="sn"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private IEnumerator SendHttpToSoundServer(string sn, int idx, string data)
    {
        Debugger.Log("sn2 = {0}", sn);
        string tk = MD5.ComputeHashString(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}", sn, 
            User_Config.VOICE_MD5_STRING)));
        string url = string.Format("http://{0}:{1}/send?sn={2}&idx={3}&end=1&data={4}&tk={5}", 
            User_Config.soundIp, User_Config.soundPort, sn, idx, data, tk);

        WWW getData = new WWW(url);

        yield return getData;

        if (getData.error != null)
        {
            Debugger.LogError("Voice record error: {0}", getData.error);
            sendRet = "";
        }
        else
        {
            sendRet = getData.text;
        }
        getData.Dispose();
    }

    /// <summary>
    /// 向语音服务器请求语音数据
    /// </summary>
    /// <param name="sn"></param>
    /// <returns></returns>
    private IEnumerator GetSoundFromSoundServer(string sn)
    {
        string tk = MD5.ComputeHashString(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}", sn, User_Config.VOICE_MD5_STRING)));
        string url = String.Format("http://{0}:{1}/get?sn={2}&tk={3}", User_Config.soundIp, User_Config.soundPort, sn, tk);

        WWW getData = new WWW(url);

        yield return getData;

        if (getData.error != null)
        {
            getRet = "";
        }
        else
        {
            getRet = getData.text;
        }
        getData.Dispose();
    }
    #endregion

    #region 识别
    /// <summary>
    ///  客户端创建sn
    /// </summary>
    /// <returns></returns>
    private string CreateSnByNow()
    {
        long microsecond = TimeConverter.ConvertDateTimeLong(DateTime.Now) / 10;

        string ret = String.Format("{0}{1}{2}", microsecond, UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10));
        return ret;
    }
    #endregion

    public void CheckMicrophonePermission(string result)
    {
        if (result == "succeed")
        {
            RecordBegin();
        }
        else if (result == "failed")
        {
            MessageBox.Show(LanguageTips.CHAT_PERMISSION_FAILED);
        }
    }
}
