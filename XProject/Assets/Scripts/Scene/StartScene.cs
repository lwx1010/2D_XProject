using UnityEngine;
using System.Collections;
using DG.Tweening;
using LuaFramework;
using UnityEngine.SceneManagement;
using Config;
using System.IO;
using Riverlake;

public sealed class StartScene : MonoBehaviour
{

    public const string Name = "StartScene";

    public ScreenResolution sr;

    void Awake()
    {
        Debug.Log(AppSystem.Dump());
        if (SystemInfo.supports3DTextures)
            Application.backgroundLoadingPriority = ThreadPriority.High;

        LoadConfig();
        StartCoroutine(InitVersionAndBugly());
    }

    IEnumerator InitVersionAndBugly()
    {
        sr.AdjustResolution();
        sr.setDesignContentScale();
        //Util.AutoAdjustCameraRect(UICamera.mainCamera);
        Debug.Log(string.Format("Resolution: {0}", Screen.currentResolution));
        yield return Yielders.GetWaitForSeconds(0.1f);
        string versionFile = string.Format("{0}{1}/version.txt", Util.AppContentPath(), LuaConst.osDir);
        Debug.Log(versionFile);
        string version = string.Empty;
        if (Application.isMobilePlatform && Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(versionFile);
            yield return www;
            if (www.error != null)
            {
                Debug.LogError(www.error);
                version = Application.version;
            }
            else version = www.text;
            www.Dispose();
            Debug.Log(string.Format("package version: {0}", version));
        }
        else
        {
            if (File.Exists(versionFile)) version = File.ReadAllText(versionFile);
            else version = Application.version;
        }
        GameManager.packVersion = GameVersion.CreateVersion(version);
        string localVersionFile = Path.Combine(Util.DataPath, "version.txt");
        GameManager.localVersion = GameVersion.CreateVersion(localVersionFile, GameManager.packVersion.ToString());

#if UNITY_EDITOR
        //BuglyAgent.ConfigDebugMode(true);
#else
        //BuglyAgent.ConfigDebugMode(false);
#endif
#if UNITY_IOS
        BuglyAgent.InitWithAppId("1ce5a132bc");
#elif UNITY_ANDROID
        //BuglyAgent.InitWithAppId("529dceaf06");  
#endif
        BuglyAgent.SetUserId(string.Format("{0}:{1}", SystemInfo.deviceModel, SystemInfo.deviceName));
        BuglyAgent.ConfigAutoReportLogLevel(LogSeverity.LogAssert);
        BuglyAgent.EnableExceptionHandler();

        InitOtherSetting();
        yield return Yielders.GetWaitForSeconds(2f);
        SceneManager.LoadScene("UpdateScene");
    }

    void InitOtherSetting()
    {
        DOTween.Init();
        DOTween.defaultEaseType = Ease.Linear;
#if UNITY_EDITOR
        AutoUpdateScene.jumped = true;
#endif

#if !UNITY_EDITOR
        if (!Directory.Exists(Util.DataPath))
            Directory.CreateDirectory(Util.DataPath);
#endif
    }

    void LoadConfig()
    {
        // first check local config exists or not
        string content = string.Empty;

        if (!AppConst.DebugMode && !Directory.Exists(Util.DataPath))
            Directory.CreateDirectory(Util.DataPath);

        string config_path = Util.DataPath + "/config.txt";
        if (FileManager.FileExist(config_path))
        {
            content = FileManager.ReadFileString(config_path);
        }
        else
        {
            TextAsset obj = Resources.Load<TextAsset>("config");
#if UNITY_EDITOR
            if (obj == null) Debug.LogError(LanguageTips.NO_CONFIG_VIA_EDITORMODE);
#else
            if (obj == null)
            {
                MessageBox.Show(UpdateTips.NO_CONFIG_VIA_MOBLIEPLATFORM, (int)MessageBox.Style.OkOnly, (go) =>
                {
                    Application.Quit();
                });
            }
#endif
            content = obj.text;
#if !UNITY_EDITOR
            FileManager.WriteFile(config_path, content);
#endif
        }
        // load config
        User_Config.LoadConfig(content);
        User_Config.LoadGlobalSetting();
#if UNITY_IOS && !UNITY_EDITOR
        // iphone 6及以下强行设置为流畅 否则可能出现内存问题闪退
        try
        {
            string deviceVersion = SystemInfo.deviceModel;
            Debug.Log(deviceVersion);
            if (!string.IsNullOrEmpty(deviceVersion))
            {
                string[] tmps = deviceVersion.Split(',');
                Debug.Assert(tmps.Length == 2);
                int iVer = Convert.ToInt32(tmps[0].Substring(tmps[0].Length - 1, 1));
                if (iVer < 7 || (iVer == 7 && tmps[1].Equals("iPhone7,2")))
                {
                    Debug.Log("iphone version < iphone6, set quality to 0");
                    User_Config.quality = 0;
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
#endif
    }
}
