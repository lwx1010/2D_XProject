using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using AL.Crypto;
using Config;
using LuaInterface;

[RequireComponent (typeof(AudioSource))]

public sealed class MicroPhoneInput : MonoBehaviour {

	private static MicroPhoneInput m_instance;

	public float sensitivity=100;
	public float loudness=0;

    private static string[] micArray=null;

    const int HEADER_SIZE = 44;

    const int RECORD_TIME = 30;

    const int SAMPLING_RATE = 8000;

    private const float AGCLEVEL = 24000f;

    private Speex _speex;

    public Byte[] lastByteArr;

    private int soundTime;
    private float _startTime;
    private float _realTime;

    private Action _lastAction;

    private string _curSn;
    public int idx;

    private AudioSource audioSrc;
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass _jc;
    private AndroidJavaObject _jo;
#endif

#if UNITY_IPHONE
#if UNITY_EDITOR
    const string SPEEXDLL = "speexdsp";
#else
    const string SPEEXDLL = "__Internal";
#endif
    [DllImport(SPEEXDLL)]
    public extern static void initprocess(int frame_size, int sampling_rate, float agc_level);
    [DllImport(SPEEXDLL)]
    public extern unsafe static short* preprocess(short* lin, int lin_size, short* outData);
    [DllImport(SPEEXDLL)]
    public extern static void closepreprocess();
#endif

    float[] volData = new float[128];

    // Use this for initialization
    void Awake () {
        audioSrc = GetComponent<AudioSource>();
        _speex = new Speex();

        AudioSettings.speakerMode = AudioSpeakerMode.Mono;

        InitDeNoiseProcess();
	}

	public static MicroPhoneInput getInstance(GameObject go)
	{
		if (m_instance == null) 
		{
            micArray = Microphone.devices;
            if (micArray.Length == 0)
            {
                Debugger.LogWarning("Microphone.devices is null");
            }
            int len = Microphone.devices.Length;
            for (int i = 0; i < len; i++)
            {
                string deviceStr = Microphone.devices[i];
                Debugger.Log("device name = 11 {0}", deviceStr);
            }
			if(micArray.Length == 0)
			{
				Debugger.LogWarning("no mic device");
			}

            m_instance = go.gameObject.AddComponent<MicroPhoneInput>();
		}
		return m_instance;
	}


	public void StartRecord(string sn)
	{
        audioSrc.Stop();
        if (micArray.Length == 0)
        {
            Debugger.Log("No Record Device!");
            return;
        }
        audioSrc.loop = false;
        audioSrc.mute = true;
        audioSrc.volume = 1;
        audioSrc.clip = Microphone.Start(null, false, RECORD_TIME, SAMPLING_RATE); //22050 
		while (!(Microphone.GetPosition(null)>0)) {
		}

        _curSn = sn;
        idx = 0;
        audioSrc.Play();
        Debugger.Log("StartRecord");
        _startTime = Time.time;
        
        //倒计时
        StartCoroutine(TimeDown());
	}

	public int StopRecord()
	{
        if (micArray.Length == 0)
        {
            Debugger.Log("No Record Device!");
            return 0;
        }
        if (!Microphone.IsRecording(null))
        {
            return 0;
        }
		Microphone.End (null);
        audioSrc.Stop();

        Debugger.Log("StopRecord");

        // 多存一秒
        _realTime = Time.time - _startTime;

        SaveClipData();
        return Mathf.CeilToInt(_realTime);
	}

	public Byte[] GetClipData()
    {
        if (audioSrc.clip == null)
        {
            Debugger.Log("GetClipData audio.clip is null");
            return null; 
        }

        float[] samples = new float[Mathf.CeilToInt(_realTime) * SAMPLING_RATE];
        audioSrc.clip.GetData(samples, 0);

		Byte[] outData = new byte[samples.Length * 2];
        int rescaleFactor = 32767; 
        for (int i = 0; i < samples.Length; i++)
        {
            short temshort = (short)(samples[i] * rescaleFactor);

            Byte[] temdata = System.BitConverter.GetBytes(temshort);

            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }

		if (outData == null || outData.Length <= 0)
        {
            Debugger.Log("GetClipData intData is null");
            return null; 
        }

		return outData;
    }

    private Int16[] GetClipData16()
    {
        if (audioSrc.clip == null)
        {
            Debugger.Log("GetClipData audio.clip is null");
            return null;
        }

        float[] samples = new float[Mathf.CeilToInt(_realTime) * SAMPLING_RATE];

        audioSrc.clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            short temshort = (short)(samples[i] * rescaleFactor);

            intData[i] = temshort;
        }

        if (intData == null || intData.Length <= 0)
        {
            Debugger.Log("GetClipData intData is null");
            return null;
        }

        return intData;
    }

    void PlayClipData(Int16[] intArr, Action action = null)
    {
        if (intArr.Length == 0)
        {
            Debugger.Log("get intarr clipdata is null");
            return;
        }
        //从Int16[]到float[]
        float[] samples = new float[intArr.Length];
        int rescaleFactor = 32767;
        for (int i = 0; i < intArr.Length; i++)
        {
            samples[i] = (float)intArr[i] / rescaleFactor;
        }
        
        //从float[]到Clip
        audioSrc.clip = null;
        if (audioSrc.clip == null)
        {
            audioSrc.clip = AudioClip.Create("playRecordClip", intArr.Length, 1, SAMPLING_RATE, false);
        }
        audioSrc.clip.SetData(samples, 0);
        audioSrc.mute = false;
        audioSrc.Play();

        if (action != null) StartCoroutine(WaitForPlayFinished((float)intArr.Length / (float)SAMPLING_RATE, action));
    }

    IEnumerator WaitForPlayFinished(float delay, Action action)
    {
        yield return Yielders.GetWaitForSeconds(delay);
        if (action != null)
        {
            action();
            _lastAction = null;
        }
    }
    
    public void PlayRecord()
	{
        if (audioSrc.clip == null)
        {
            Debugger.Log("audio.clip=null");
            return;
        }
        audioSrc.mute = false;
        audioSrc.loop = false;
        audioSrc.Play ();
        Debugger.Log("PlayRecord");
	}

	public float GetAveragedVolume()
	{
        try
        {
            if (audioSrc == null || audioSrc.clip == null) return 0;
            int startPosition = Microphone.GetPosition(null) - (256 + 1);
            if (startPosition < 0) return 0;
            Array.Clear(volData, 0, volData.Length);
            audioSrc.clip.GetData(volData, startPosition);
            float a = 0;
            for (int i = 0; i < volData.Length; i++)
            {
                float s = volData[i];
                a += Mathf.Abs(s);
            }
            return a / 256;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return 0;
        }
        
    }
	
	// Update is called once per frame
	void Update ()
    {

	}

    public Byte[] GetFinalClipData()
    {
        if (audioSrc.clip == null)
        {
            Debugger.Log("GetClipData audio.clip is null");
            return null;
        }

        float[] samples = new float[Mathf.CeilToInt(_realTime) * SAMPLING_RATE];

        audioSrc.clip.GetData(samples, 0);

        int rescaleFactor = 32767;

        Byte[] outData = new byte[samples.Length * 2 - (idx - 1) * SAMPLING_RATE * 2];
        for (int i = 0; i < outData.Length / 2; i++)
        {
            short temshort = (short)(samples[i + (idx - 1) * SAMPLING_RATE] * rescaleFactor);

            Byte[] temdata = System.BitConverter.GetBytes(temshort);

            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }

        return _speex.Encode(outData);
    }

    /// <summary>
    /// 获取idx对应的数据
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public Byte[] GetClipDataByIdx(int idx)
    {
        if (audioSrc.clip == null)
        {
            Debugger.Log("GetClipData audio.clip is null");
            return null;
        }

        float[] samples = new float[soundTime* SAMPLING_RATE];

        audioSrc.clip.GetData(samples, 0);

        int rescaleFactor = 32767;
        Byte[] outData = new byte[SAMPLING_RATE * 2];
        for (int i = 0; i < SAMPLING_RATE; i++)
        {
            short temshort = (short)(samples[i + (idx - 1) * SAMPLING_RATE] * rescaleFactor);

            Byte[] temdata = System.BitConverter.GetBytes(temshort);

            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }

        return _speex.Encode(outData);
    }

    private IEnumerator TimeDown()
    {
        Debugger.Log(" IEnumerator TimeDown()");

        soundTime = 0;
        while (soundTime < RECORD_TIME-4)
        {
			if (!Microphone.IsRecording (null)) 
			{ //如果没有录制
				Debugger.Log ("IsRecording false");
				yield break;
			}
            //Debugger.Log("yield return Yielders.GetWaitForSeconds {0}", soundTime);

            WWW www = SendHttpToSoundServer(idx);
            idx++;
            yield return www;
            yield return Yielders.GetWaitForSeconds(1);
            if (www != null)
                www.Dispose();
            soundTime++;
        }
        if (soundTime >= RECORD_TIME-4)
        {
            Debugger.Log("RECORD_TIME is out! stop record!");
            ChatVoiceService.Instance.EndRecordSound(true);
        }
        yield return 0;
    }

    public bool PlayLastSound()
    {
        if (lastByteArr == null)
        {
            Debugger.Log("没有之前的音频!!!");
            return false;
        }

        PlayClipData(byteTo16(_speex.Decode(lastByteArr)));

        return true;
    }

    private Int16[] byteTo16(Byte[] data)
    {
        Int16[] retData = new Int16[data.Length / 2];
        IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
        Marshal.Copy(p, retData, 0, retData.Length);

        return retData;
    }

    private Byte[] int16ToByte(Int16[] data)
    {
        Byte[] resData = new Byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            Byte[] tmp = BitConverter.GetBytes(data[i]);
            resData[i * 2] = tmp[0];
            resData[i * 2 + 1] = tmp[1];
        }

        return resData;
    }

    /// <summary>
    ///  播放64string
    /// </summary>
    /// <param name="data"></param>
    /// <param name="action"></param>
    public void Play64String(string data, int voiceTime, Action action)
    {
        StopCoroutine("WaitForPlayFinished");
        if( _lastAction != null )
            _lastAction();

        _lastAction = action;
        PlayClipData(GetDeNoiseData1(byteTo16(_speex.Decode(Convert.FromBase64String(data)))), action);
    }

    /// <summary>
    /// 停止当前的录音播放
    /// </summary>
    public void StopPlayCurrentVoice()
    {
        if(audioSrc.clip != null)
        {
            audioSrc.Stop();
            audioSrc.clip = null;
            StopCoroutine("WaitForPlayFinished");
        }
    }

    private void SaveClipData()
    {
        Byte[] data = GetClipData();
        if (data == null)
            return;


        lastByteArr = _speex.Encode(GetClipData());
    }

    private void SaveData(string str)
    {
        string fileName = "soundTest.txt";
        FileStream fs = null;
        try
        {
            fs = new FileStream(fileName, FileMode.Open);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(str);
            }
        }
        finally
        {
            if (fs != null)
                fs.Dispose();
        }
    }

    private void SaveWav(Byte[] data)
    {
#if UNITY_EDITOR
        FileStream fs = new FileStream(@"sound.pcm", FileMode.Create);
        fs.Write(data, 0, data.Length);
        fs.Flush();
        fs.Close();
#endif
    }

    /// <summary>
    ///  上传语音数据到语音服务器
    /// </summary>
    /// <param name="sn"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private WWW SendHttpToSoundServer(int idx)
    {
        if (idx <= 0)
            return null;

        Byte[] data = GetClipDataByIdx(idx);
        //Debugger.Log("idx = " + idx);
        string tk = MD5.ComputeHashString(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}{1}", _curSn, User_Config.VOICE_MD5_STRING)));
        string url = string.Format("http://{0}:{1}/send?sn={2}&idx={3}&data={4}&tk={5}", 
            User_Config.soundIp, User_Config.soundPort, _curSn, idx, Convert.ToBase64String(data).Replace("+", "%2B"), tk);

        return new WWW(url);
    }


#region 调用android方法
    /// <summary>
    /// 去噪
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public short[] GetDeNoiseData1(short[] data)
    {
        short[] ret;
#if UNITY_ANDROID && !UNITY_EDITOR
        Int16[] outData = new Int16[data.Length];
        outData = _jo.Call<short[]>("preprocess",data, data.Length, outData);
        ret = outData;
#elif UNITY_IPHONE
        unsafe
        {
            short[] outData = new short[data.Length];
            fixed (short* pOut = &outData[0])
            {
                fixed (short* pIn = &data[0])
                {
                    preprocess(pIn, data.Length, pOut);
                }
            }
            ret = data;
        }
#else
        ret = data;
#endif
        return ret;
    }

    /// <summary>
    /// 初始化去噪处理
    /// </summary>
    private void InitDeNoiseProcess()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _jc = new AndroidJavaClass("com.unity3d.player.UnityActivity");
        _jo = _jc.GetStatic<AndroidJavaObject>("currentActivity");
        _jo.Call("init_preprocess", 160, SAMPLING_RATE, AGCLEVEL);
#elif UNITY_IPHONE
        initprocess(160, SAMPLING_RATE, AGCLEVEL);
#endif
    }

    /// <summary>
    /// 关闭去噪处理
    /// </summary>
    private void CloseDeNoiseProcess()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _jo.Call("closepreprocess");
#elif UNITY_IPHONE
        closepreprocess();
#endif
    }

    /// <summary>
    /// 关闭去噪处理
    /// </summary>
    public int TestAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        int rlt = _jo.Call<int>("Max", 10, 20);
        return rlt;
#else
        return 0;
#endif
    }
#endregion

    public void CheckPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _jo.Call("checkMicrophonePermission");
#endif
    }

}
