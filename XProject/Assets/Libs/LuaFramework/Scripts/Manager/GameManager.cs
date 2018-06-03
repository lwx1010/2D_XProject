using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.IO;
using System.Linq;
using AL;
using AL.Crypto;
using Config;

namespace LuaFramework {
    public class GameManager : Manager {
        public static GameVersion localVersion { get; set; }
        public static GameVersion packVersion { get; set; }

        public Action MessageBoxYes;
        public Action MessageBoxNo;

        protected static bool initialize = false;
        private HashSet<string> extractFiles = new HashSet<string>();
        private HashSet<string> downloadErrorFiles = new HashSet<string>();
        private Dictionary<string, string> downloadCheckDatas = new Dictionary<string, string>();

        private long totalExtractCount = 0;

        public List<string> needDownloadFiles = new List<string>();
        public long TotalDownloadSize { get; private set; }

        #region 分包下载相关
        /// <summary>
        /// 需要下载的整包资源及其状态
        /// </summary>
        private Dictionary<string, string> needDownloadPackFiles = new Dictionary<string, string>();

        private int curDownloadIndex = 0;

        public bool pauseDownloading
        {
            get
            {
                return _pauseDownloading;
            }
            set
            {
                if (!value)
                {
                    if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
                    {
                        MessageBox.Show(LanguageTips.NO_WIFI_DOWNLOADING, (result) =>
                        {
                            if (result == DialogResult.Yes)
                            {
                                allowCarrierDataDownload = true;
                                if (MessageBoxYes != null)
                                    MessageBoxYes();
                            }
                            else if (result == DialogResult.No)
                            {
                                allowCarrierDataDownload = false;
                                if (MessageBoxNo != null)
                                    MessageBoxNo();
                            }
                            
                        }, MessageBoxButtons.YesNo);
                    }
                }
                else
                {
                    pausedIndex = curDownloadIndex;
                    allowCarrierDataDownload = false;
                }
                _pauseDownloading = value;
            }
        }

        public delegate void DownloadPackProgressChanged(float progress);

        public DownloadPackProgressChanged progressChanged;

        private int pausedIndex = 0;

        private bool _pauseDownloading;

        private bool allowCarrierDataDownload = false;

        private static bool beginPackDownload = false;
        /// <summary>
        /// 是否完成后台下载
        /// </summary>
        public bool finishedBackgroundDownloading = false;
        /// <summary>
        /// 是否后台解压
        /// </summary>
        public bool backgroundExtracting = false;

        public bool unlimited { get; set; }
        /// <summary>
        /// 是否直接下载分包资源（刚进入游戏时）
        /// </summary>
        public bool downloadAll { get; set; }

        private int packDownloadErrorCount = 0;

        private int packDownloadJumpedCount = 0;

        private float packDownloadTimeout = 0;
        private bool timeout = false;
        #endregion

        #region 省电模式
        private float powerSaveDelay = 0;
        private bool showPowerSave = false;

        private const int POWER_SAVE_DELAY = 5 * 60;
        #endregion

        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        void Start() {
            unlimited = false;
            powerSaveDelay = POWER_SAVE_DELAY;
            
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init() {

            DontDestroyOnLoad(gameObject);  //防止销毁自己

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = AppConst.GameFrameRate;

            Debug.Assert(StartUpController.Instance != null);
            if (StartUpController.Instance.startUpEvent != null)
                StartUpController.Instance.startUpEvent();
            StartUpController.Instance.onGetServerInfo += CheckExtractResource;
            StartUpController.Instance.CheckServerInfo();
        }

        void Update()
        {
            if (UpdateAssetsOnlineCtrl.Instance != null)
            {
                UpdateAssetsOnlineCtrl.Instance.UpdateResumingDownloadProgress();
            }
            //if (NetManager != null && User_Config.powSaving == 1 && !NetManager.inBackground)
            //{
            //    if (!showPowerSave)
            //    {
            //        if (Input.GetMouseButton(0) || Input.touchCount > 0)
            //            powerSaveDelay = POWER_SAVE_DELAY;
            //        else
            //            powerSaveDelay -= Time.deltaTime;
            //    }
            //    if (powerSaveDelay < 0 && !showPowerSave)
            //    {
            //        showPowerSave = true;
            //        powerSaveDelay = 0;
            //        NetManager.SetPowerSaveMode(true);
            //        PowerSaveingPanel.DisplayPowerSavingPanel(OnPowerSaveClick);
            //    }
            //}
            
            //SDK 退出接口
            if (Config.User_Config.internal_sdk == 1 && Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            {
                SDKInterface.Instance.SDKExit();
            }
        }

        void OnPowerSaveClick()
        {
            NetManager.SetPowerSaveMode(false);
            powerSaveDelay = POWER_SAVE_DELAY;
            showPowerSave = false;
        }

        public static string GetSizeString(long size)
        {
            string ext = "MB";
            float fmtSize = 0;
            if (size > 1024 * 1024)
            {
                fmtSize = (float)size / (float)(1024 * 1024);
            }
            else if (size > 1024)
            {
                fmtSize = (float)size / (float)1024;
                ext = "KB";
            }
            else
            {
                fmtSize = (float)size;
                ext = "B";
            }
            return string.Format("{0}{1}", fmtSize.ToString("f2"), ext);
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public void CheckExtractResource() {
            if (AppConst.AssetBundleMode)
            {
                // 检测apk是否有更新
                StartCoroutine(GetPackageResourcesOnServer());
            }
            else
            {
                StartExtractAndUpdate();
            }
        }

        IEnumerator GetPackageResourcesOnServer()
        {
            string infile = string.Format("{0}{1}/files.txt", Util.AppContentPath(), LuaConst.osDir);
            string text = string.Empty;
            if (Application.isMobilePlatform && Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(infile);
                yield return www;
                if (www.error != null)
                {
                    Debug.LogError(www.error);
                    StartExtractAndUpdate();
                    yield break;
                }
                text = www.text;
                www.Dispose();
            }
            else
            {
                text = File.ReadAllText(infile);
            }
            string[] files = text.Split('\n');
            for (int i = 0; i < files.Length; ++i)
            {
                var file = files[i].TrimEnd('\r');
                string[] keyValue = file.Split('|');
                if (keyValue.Length == 4 || keyValue.Length == 5)
                {
                    if (Convert.ToInt32(keyValue[3].Split(':')[1]) == 1)
                    {
                        //var filePath = Path.Combine(Util.DataPath, keyValue[0]);
                        needDownloadPackFiles.Add(keyValue[0], file);
                    }
                }
            }

            var sortedFiles = needDownloadPackFiles.OrderBy(p => Convert.ToInt32(p.Value.Split('|')[4].Split(':')[1])).ToDictionary(p => p.Key, p => p.Value);
            needDownloadPackFiles.Clear();
            needDownloadPackFiles = sortedFiles;

            StartExtractAndUpdate();
        }

        void StartExtractAndUpdate()
        {
            bool isExtracted = false;
            if (!AppConst.DebugMode)
            {
                Debug.LogWarning(string.Format("Local Version:{0}, Pack Version:{1}", localVersion.ToString(), packVersion.ToString()));
                if (File.Exists(Util.DataPath + "extract.txt"))
                    isExtracted = localVersion >= packVersion;
            }
            UpdateAssetsOnlineCtrl.Instance.onExtractFinish += BeginUpdate;
            UpdateAssetsOnlineCtrl.Instance.onUpdateFinish += OnResourceInited;
            if (isExtracted || AppConst.DebugMode)
            {
                BeginUpdate();
                return;   //文件已经解压过了，自己可添加检查文件列表逻辑
            }
            //var filePath = Util.DataPath + "datamap.ab";
            //多线程池版本测试
            StartCoroutine(UpdateAssetsOnlineCtrl.Instance.OnExtractResources());
        }

        /// <summary>
        /// 开始资源更新
        /// </summary>
        /// <param name="remoteVersion"></param>
        public void BeginUpdate()
        {
            CheckUpdateClick();
        }

        public void SetMd5(string name, string realMd5, bool isSave = true)
        {
            PlayerPrefs.SetInt(name, 1);
            PlayerPrefs.SetString(string.Format("{0}_md5", name), realMd5);
            if (isSave) PlayerPrefs.Save();
        }

        public void CheckUpdateClick()
        {
            if (!AppConst.UpdateMode)
            {
                OnResourceInited();
            }
            else
            {
                UpdateAssetsOnlineCtrl updateCtrl = UpdateAssetsOnlineCtrl.Instance;
                //多线程测试
                StopCoroutine(updateCtrl.OnUpdateResource());
                StartCoroutine(updateCtrl.OnUpdateResource());
            }
        }

        public void OnUpdateFailed(string errorMsg)
        {
            MessageBox.Show(errorMsg, (result) =>
            {
                OnResourceInited();
            });
        }

        public IEnumerator AdjustFileStatus(string[] files, ProgressBar progressBar)
        {
            string dataPath = Util.DataPath;  //数据目录
            for (int i = 0; i < files.Length; ++i)
            {
                string[] keyValue = files[i].Split('|');
                string f = keyValue[0];
                string localfile = (dataPath + f).Trim();
                string md5 = keyValue[1].Trim();
                var status = GetFileStatus(localfile, files[i]);
                if (status == 2 && !MD5.ComputeHashString(localfile).Equals(md5))
                {
                    needDownloadFiles.Add(files[i]);
                    PlayerPrefs.DeleteKey(f + "_md5");
                    PlayerPrefs.Save();
                    if (needDownloadPackFiles.ContainsKey(f))
                        needDownloadPackFiles.Remove(f);
                }
                float progress = (float)i / (float)files.Length;
                progressBar.SetMessage(string.Format("{0}{1}", LanguageTips.AUTO_ADJUST_FILES, progress * 100));
                progressBar.UpdateProgress(progress);
                yield return Yielders.EndOfFrame;
            }
        }

        public long GetTotalDownloadSize()
        {
            long totalSize = 0;
            for (int i = 0; i < needDownloadFiles.Count; i++)
            {
                if (string.IsNullOrEmpty(needDownloadFiles[i])) continue;
                string[] keyValue = needDownloadFiles[i].Split('|');
                string f = keyValue[0];
                totalSize += Convert.ToInt64(keyValue[2]);
            }
            return totalSize;
        }

        /// <summary>
        /// 获取文件状态码
        /// </summary>
        /// <param name="file"></param>
        /// <param name="value"></param>
        /// <returns>0:不需要下载; 1:需要下载更新; 2:需要比较MD5</returns>
        int GetFileStatus(string file, string value)
        {
            int status = 0;
            string[] keyValue = value.TrimEnd('\r').Split('|');
            var fileStatus = PlayerPrefs.GetInt(keyValue[0]);
            if (fileStatus == 0)
            {
                status = 1;
                if (keyValue.Length == 4 || keyValue.Length == 5)
                {
                    var temp = Convert.ToString(keyValue[3]).Split(':');
                    var type = Convert.ToInt32(temp[1]);
                    // type = 1 分包资源 2 补丁资源 3 不需要下载
                    if (type == 1 && !downloadAll && !File.Exists(file))
                        status = 0;
                    else if (type == 3)
                        status = 0;
                }
            }
            else if (fileStatus == 1)
            {
                status = File.Exists(file) ? 2 : 1;
            }
            return status;
        }

        public void RestartUpatePack()
        {
            StopCoroutine(UpdatePackResources());
            beginPackDownload = false;
        }

        private IEnumerator UpdatePackResources()
        {
            if (beginPackDownload) yield break;
            extractFiles.Clear();
            downloadCheckDatas.Clear();
            downloadErrorFiles.Clear();
            totalExtractCount = 0;
            beginPackDownload = true;
            curDownloadIndex = 0;
            string dataPath = Util.DataPath;  //数据目录
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);

            int curOrder = 0;
            foreach (var key in needDownloadPackFiles.Keys)
            {
                string[] keyValue = needDownloadPackFiles[key].Split('|');
                var order = Convert.ToInt32(keyValue[4].Split(':')[1]);
                string f = keyValue[0];
                string localfile = (dataPath + f).Trim();
                string path = Path.GetDirectoryName(localfile);
                string md5 = keyValue[1].Trim();
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var tempMd5 = string.Format("{0}_md5", f);
                if (File.Exists(localfile) && PlayerPrefs.GetString(tempMd5) == md5)
                {
                    curDownloadIndex++;
                    if (progressChanged != null && !pauseDownloading)
                        progressChanged((float)curDownloadIndex / (float)needDownloadPackFiles.Count);
                    continue;
                }
                while ((Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork && !allowCarrierDataDownload) || pauseDownloading)
                    yield return Yielders.EndOfFrame;
                if (curOrder != order)
                {
                    if (!unlimited) yield return Yielders.GetWaitForSeconds(1f);
                    curOrder = order;
                }
                string fileUrl = string.Format("{0}/{1}?v={2}", Config.User_Config.web_url, keyValue[0], random);
                Debugger.Log(string.Format("[0]Download in background>>{0}", fileUrl));

                packDownloadTimeout = 0;
                timeout = false;
                var www = new WWW(fileUrl);
                while (!www.isDone)
                {
                    if (packDownloadTimeout >= 20 && www.progress == 0 || packDownloadTimeout >= 300)
                    {
                        timeout = true;
                        break;
                    }
                    packDownloadTimeout += Time.deltaTime;
                    yield return Yielders.EndOfFrame;
                }

                if (www.error != null || timeout)
                {
                    packDownloadErrorCount++;
                    if (timeout)
                        Debugger.LogError(string.Format("[0]Background download error:>> timeout\nfile url: {0}", fileUrl));
                    else
                        Debugger.LogError(string.Format("[0]Background download error:>>{0}\nfile url: {1}", www.error, fileUrl));
                    if (packDownloadErrorCount > 3)
                    {
                        packDownloadJumpedCount++;
                        continue;
                    }
                    else
                    {
                        beginPackDownload = false;
                        StartCoroutine(UpdatePackResources());
                        yield break;
                    }
                }
                if (www.isDone)
                {
                    try
                    {
                        if (keyValue[0].EndsWith(".ab"))
                        {
                            if (!downloadCheckDatas.ContainsKey(keyValue[0]))
                                downloadCheckDatas.Add(keyValue[0], md5);
                            else if (downloadCheckDatas[keyValue[0]] != md5)
                                downloadCheckDatas[keyValue[0]] = md5;
                            BeginDownloadExtract(dataPath, keyValue[0], www.bytes);
                        }
                        else
                        {
                            var realMd5 = MD5.ComputeHashString(www.bytes);
                            if (realMd5 != md5)
                            {
                                packDownloadJumpedCount++;
                                curDownloadIndex++;
                                Debug.LogError(string.Format("[0]Background downloading md5 error!\nfile url: {0}", fileUrl));
                                www.Dispose();
                                www = null;
                                continue;
                            }
                            if (File.Exists(localfile)) File.Delete(localfile);
                            File.WriteAllBytes(localfile, www.bytes);
                            // 记录文件下载状态: 1-下载完毕
                            PlayerPrefs.SetInt(f, 1);
                            PlayerPrefs.DeleteKey(tempMd5);
                            PlayerPrefs.SetString(tempMd5, realMd5);
                            PlayerPrefs.Save();
                            Util.CallMethod("Game", "OnOnePackFileDownload", keyValue[0]);
                        }
                        www.Dispose();
                        www = null;
                        curDownloadIndex++;
                        var value = (float)curDownloadIndex / (float)needDownloadPackFiles.Count;
                        var percent = (int)(value * 100);
                        if (percent == 30) Util.CallMethod("Network", "OnPackageDownloadComplete", 30);
                        else if (percent == 60) Util.CallMethod("Network", "OnPackageDownloadComplete", 60);
                        if (progressChanged != null && !pauseDownloading)
                            progressChanged(value);
                    }
                    catch (Exception e)
                    {
                        packDownloadJumpedCount++;
                        curDownloadIndex++;
                        Debug.LogError(string.Format("[0]Background downloading error: {0}\nFile url: {1}", e.Message, fileUrl));
                        www.Dispose();
                        www = null;
                        continue;
                    }
                    if (!unlimited) yield return Yielders.GetWaitForSeconds(1f);
                }
            }
            if (downloadErrorFiles.Count > 0)
            {
                Debug.LogError(string.Format("[0]Background downloading error: {0}", LanguageTips.UPDATE_MD5_ERROR));
                StartCoroutine(DownloadErrorFiles());
            }
            while (extractFiles.Count > 0)
            {
                backgroundExtracting = true;
                if (progressChanged != null) progressChanged(GetExtractingPercent());
                yield return Yielders.GetWaitForSeconds(0.5f);
            }
            backgroundExtracting = false;
            finishedBackgroundDownloading = true;
            if (downloadErrorFiles.Count == 0) ThreadManager.Dispose();
            if (progressChanged != null && !pauseDownloading)
                progressChanged(1);
            Util.CallMethod("Network", "OnPackageDownloadComplete", 100);
        }

        public float GetExtractingPercent()
        {
            return (float)(1 - extractFiles.Count) / (float)totalExtractCount;
        }

        IEnumerator DownloadErrorFiles()
        {
            if (packDownloadErrorCount > 3)
            {
                ThreadManager.Dispose();
                yield break;
            }
            string dataPath = Util.DataPath;  //数据目录
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            foreach (var key in downloadErrorFiles)
            {
                string fileUrl = string.Format("{0}/{1}?v={2}", Config.User_Config.web_url, key, random);
                Debugger.Log(string.Format("[1]Download in background>>{0}", fileUrl));
                var www = new WWW(fileUrl);
                yield return www;

                if (www.error != null)
                {
                    packDownloadErrorCount++;
                    Debugger.LogError(string.Format("[1]Background download error:>>{0}\nfile url: {1}", www.error, fileUrl));
                    StartCoroutine(DownloadErrorFiles());
                    yield break;
                }
                if (www.isDone)
                {
                    try
                    {
                        Debug.Assert(key.CustomEndsWith(".ab"));
                        BeginDownloadExtract(dataPath, key, www.bytes);
                        www.Dispose();
                        www = null;
                    }
                    catch (Exception e)
                    {
                        packDownloadErrorCount++;
                        Debug.LogError(string.Format("[1]Background downloading error: {0}\nFile url: {1}", e.Message, fileUrl));
                        www.Dispose();
                        www = null;
                        StartCoroutine(DownloadErrorFiles());
                        yield break;
                    }
                    yield return Yielders.GetWaitForSeconds(2f);
                }
            }

            yield return Yielders.GetWaitForSeconds(5f);

            if (downloadErrorFiles.Count > 0)
            {
                Debug.LogError(string.Format("[1]Background downloading error: {0}", LanguageTips.UPDATE_MD5_ERROR));
                packDownloadErrorCount++;
                StartCoroutine(DownloadErrorFiles());
            }
            else
            {
                ThreadManager.Dispose();
            }
        }

        void BeginDownloadExtract(string dataPath, string dataName, byte[] buffer)
        {
            totalExtractCount += 1;
            if (!extractFiles.Contains(dataName))
                extractFiles.Add(dataName);
            object[] param = new object[3] { dataPath, dataName, buffer };

            ThreadEvent ev = new ThreadEvent();
            ev.Key = NotiConst.DOWNLOAD_EXTRACT;
            ev.evParams.AddRange(param);
            ev.func = OnThreadCompleted;
            ThreadManager.AddEvent(ev);   //线程解压
        }

        /// <summary>
        /// 线程完成
        /// </summary>
        /// <param name="data"></param>
        void OnThreadCompleted(NotiData data) {
            switch (data.evName) {
                case NotiConst.DOWNLOAD_EXTRACT:
                    {
                        var key = data.evParam.ToString();
                        if (downloadCheckDatas.ContainsKey(key) && data.extParam != null)
                        {
                            var fileMd5 = MD5.ComputeHashString(data.extParam.ToString());
                            bool success = false;
                            if (fileMd5 != downloadCheckDatas[key])
                            {
                                if (!downloadErrorFiles.Contains(key)) downloadErrorFiles.Add(key);
                            }
                            else
                            {
                                if (downloadErrorFiles.Contains(key)) downloadErrorFiles.Remove(key);
                                downloadCheckDatas.Remove(key);
                                success = true;
                            }
                            Loom.DispatchToMainThread(() => 
                            {
                                if (success)
                                {
                                    Debugger.Log("[0]Download Success: {0}", key);
                                    if (beginPackDownload)
                                        Util.CallMethod("Game", "OnOnePackFileDownload", key);
                                    SetMd5(key, fileMd5);
                                }
                            });
                        }
                        extractFiles.Remove(key);
                    }
                    break;
                case NotiConst.DOWNLOAD_EXTRACT_FAILED:
                    {
                        var key = data.evParam.ToString();
                        extractFiles.Remove(key);
                        var message = data.extParam.ToString();
                        Loom.DispatchToMainThread(() =>
                        {
                            packDownloadJumpedCount++;
                            Debugger.LogError("Download extract error! {0}", data.extParam.ToString());
                            if (message.Contains("disk full", StringComparison.OrdinalIgnoreCase))
                                MessageBox.Show(string.Format("{0}，{1}", LanguageTips.BACKGROUND_DOWNLOADING_ERROR, LanguageTips.DISK_FULL));
                        });
                    }
                    break;
            }
        }

        public bool IsDownloadError()
        {
            return packDownloadJumpedCount > 0 || downloadErrorFiles.Count > 0; 
        }

        /// <summary>
        /// 资源初始化结束
        /// </summary>
        public void OnResourceInited() {
            UpdateAssetsOnlineCtrl.Instance.onExtractFinish = null;
            UpdateAssetsOnlineCtrl.Instance.onUpdateFinish = null;
            // ab包模式下先加载manifest
            if (AppConst.AssetBundleMode)
            {
                var bar = ProgressBar.Show();
                bar.SetMessage(LanguageTips.LOADING_GAME_ING);
                AssetBundleManager.Instance.onProgressChange += bar.UpdateProgress;
                AssetBundleManager.Instance.AssetBundleInit(() => 
                {
                    AssetBundleManager.Instance.onProgressChange -= bar.UpdateProgress;
                    bar.SetMessage(LanguageTips.LOADING_GAME_FINISHED);
                    LuaStart();
                    initialize = true;                          //初始化完 
                });
            }
            else
            {
                LuaStart();
                initialize = true;                          //初始化完 
            }
        }

        void LuaBundleProgress(float percent)
        {
            var bar = ProgressBar.ShowCurBar();
            bar.SetMessage(LanguageTips.LOADING_LUA_ING);
            bar.UpdateProgress(percent);
        }

        public void Restart()
        {
            StopAllCoroutines();
            RemoveDownloadPackProgressListener();
            beginPackDownload = false;
            NetworkManager.inGame = false;
            StartCoroutine(_restart());
        }

        IEnumerator _restart()
        {
            Debug.Log("LUA RESTART");
            Util.ClearMemory();
            yield return Yielders.GetWaitForSeconds(0.2f);
            Unload();
            AppFacade.Instance.RemoveManager<NetworkManager>();
            yield return Yielders.GetWaitForSeconds(0.2f);
            AppFacade.Instance.AddManager<NetworkManager>();
            //PanelManager.sceneUIRoot.SetActive(true);
            LuaStart();
            Util.ClearMemory();
        }

        private void LuaStart()
        {
            LuaManager.bundleProgress = LuaBundleProgress;
            LuaManager.InitStart(() => {
                var bar = ProgressBar.ShowCurBar();
                bar.UpdateProgress(1);
                bar.Hide();
                LuaManager.DoFile("Logic/Network");         //加载网络
                LuaManager.DoFile("Common/define");
                LuaManager.DoFile("Logic/Game");            //加载游戏
                NetManager.OnInit();                        //初始化网络

                Util.CallMethod("Game", "OnInitOK");          //初始化完成
                RegisterHandlers();                         //注册协议处理
            });
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        void OnDestroy() {
            Unload();
            Debug.Log("~GameManager was destroyed");
        }

        void Unload()
        {
            if (NetManager != null)
                NetManager.Unload();
            if (LuaManager != null)
                LuaManager.Close();
        }

        public void RegisterHandlers()
        {
            string handlePath = "";
            if (AppConst.LuaBundleMode)
                handlePath = Util.DataPath + "lua/protocol/handlers.txt";
            else
                handlePath = AppConst.FrameworkRoot + "/Lua/protocol/handlers.txt";
            string text = File.ReadAllText(handlePath);
            string[] handlers = text.Split(';');
            for (int i = 0; i < handlers.Length; ++i)
            {
                if (string.IsNullOrEmpty(handlers[i]))
                    continue;
                string fileName = string.Format("protocol/handler/{0}", handlers[i]);
                LuaManager.DoFile(fileName);
            }
        }

        public string[] GetSceneFileList()
        {
            List<string> scenelist = new List<string>();
            if (AppConst.AssetBundleMode)
            {
                string root = Path.Combine(Util.DataPath, "scenes");
                string[] files = Directory.GetFiles(root, "*.ab", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; ++i)
                {
                    scenelist.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            else
            {
#if UNITY_EDITOR
                string root = Path.Combine(Application.dataPath, "Scenes/Level");
                string[] files = Directory.GetFiles(root, "*.unity", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; ++i)
                {
                    scenelist.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
#endif
            }
            return scenelist.ToArray();
        }

        bool IsAllPackResourcesDownloaded()
        {
            return needDownloadPackFiles.Count == 0;
        }

        public void StartDownloadPackFiles()
        {
            if (!IsAllPackResourcesDownloaded())
                StartCoroutine(UpdatePackResources());
            else if (!backgroundExtracting)
                finishedBackgroundDownloading = true;
        }

        public float GetDownloadPackProgress()
        {
            if (finishedBackgroundDownloading)
            {
                return 1;
            }
            else
            {
                if (pauseDownloading)
                    return (float)pausedIndex / (float)needDownloadPackFiles.Count;
                else
                    return (float)curDownloadIndex / (float)needDownloadPackFiles.Count;
            }
        }

        public string GetTotalDownloadedPackSize()
        {
            long totalSize = 0;
            //string sizeStr = string.Empty;
            foreach (var key in needDownloadPackFiles.Keys)
            {
                string[] keyValue = needDownloadPackFiles[key].Split('|');

                string f = keyValue[0];
                string localfile = (Util.DataPath + f).Trim();
                string md5 = keyValue[1].Trim();
                if (File.Exists(localfile) && PlayerPrefs.GetString(string.Format("{0}_md5", f)) == md5)
                    continue;

                totalSize += Convert.ToInt64(keyValue[2]);
            }
            return GetSizeString(totalSize);
        }

        public void AddDownloadPackProgressListener(DownloadPackProgressChanged listener)
        {
            progressChanged = listener;
        }

        public void RemoveDownloadPackProgressListener()
        {
            progressChanged = null;
        }

        public bool IsStopped()
        {
            return (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork && !allowCarrierDataDownload) || pauseDownloading;
        }

        private void OnApplicationQuit()
        {
            if (Config.User_Config.internal_sdk == 1)
                CenterServerManager.Instance.SetLastServer();
        }
    }
}