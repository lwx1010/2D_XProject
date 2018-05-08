using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Frankfort.Threading;
using LuaFramework;
using LuaInterface;
using UnityEngine;
using Config;
using Riverlake.Crypto;
using Riverlake;

/// <summary>
/// 在线资源更新
/// </summary>
public class UpdateAssetsOnlineCtrl
{
    private int maxThreadCount;

    private List<string> downloadFiles = new List<string>();
    private List<string> extractFiles = new List<string>();
    private List<string> downloadErrorFiles = new List<string>();

    private List<string> extractLeftFiles = new List<string>();
    private int curSingleStreamingIndex = 0;
    private int totalSingleStreamingCount = 0;

    private int extractErrorCount = 0;

    private float extractStartTime = 0;
    public long totalExtractCount { get; set; }
    public long totalSize { get; set; }
    public long downloadedSize { get; set; }

    //private long curFileSize = 0;

    private bool beginPackDownload = false;

    #region 下载相关委托
    private ProgressBar bar { get; set; }

    public delegate void UpdateFinish();
    public UpdateFinish onExtractFinish;
    public UpdateFinish onUpdateFinish;
    #endregion

    private int packDownloadJumpedCount = 0;

    private string filesText; //远程服务端的更新文件

    private List<IThreadWorkerObject> resumingUpdates;

    private static UpdateAssetsOnlineCtrl mInstance;
    private UpdateAssetsOnlineCtrl() { }

    public static UpdateAssetsOnlineCtrl Instance
    {
        get
        {
            if(mInstance == null){
				mInstance = new UpdateAssetsOnlineCtrl();
				mInstance.Initialize();
			}
            return mInstance;
        }
    }

    public void Initialize()
    {
#if UNITY_EDITOR
        maxThreadCount = 4;
#else
        maxThreadCount = Math.Max(1, UnityEngine.SystemInfo.processorCount - 1);
#endif
        bar = ProgressBar.Show();
    }

#region ------------------远程资源服更新-------------------------------------

    /// <summary>
    /// 启动更新下载
    /// @files.txt 文件内格式以'|'分隔字符串: 
    /// @1.文件名; 2.压缩后md5值; 3.压缩后大小; 4.压缩前md5值; 5.压缩前大小; 6.资源文件类型(0.整包资源, 1.整包资源但不包含在包体中, 2.补丁资源)
    /// </summary>
    /// <returns></returns>
    public IEnumerator OnUpdateResource()
    {
        GameManager gameMgr = LuaHelper.GetGameManager();
        beginPackDownload = false;
        downloadFiles.Clear();
        extractFiles.Clear();
        downloadErrorFiles.Clear();
        totalExtractCount = 0;

        string random = DateTime.Now.ToString("yyyymmddhhmmss");
        string listUrl = string.Format("{0}/ios_check.txt?v={1}", Config.User_Config.web_url, random);
        bool timeout = true;
        WWW www = null;
        float endTime = 0;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            www = new WWW(listUrl);
            endTime = Time.realtimeSinceStartup + 5;
            while (Time.realtimeSinceStartup < endTime)
            {
                if (www.isDone)
                {
                    if (www.error == null) timeout = false;
                    break;
                }
                yield return null;
            }
            if (!timeout)
            {
                CenterServerManager.Instance.Checking = Convert.ToInt32(www.text);
                Debug.LogFormat("ios checking: ", CenterServerManager.Instance.Checking);
            }
            else CenterServerManager.Instance.Checking = 1;
            if (www != null) www.Dispose();
            if (CenterServerManager.Instance.Checking == 1)
            {
                onThreadWorkComplete(null);
                yield break;
            }
        }

        string dataPath = Util.DataPath; //数据目录
        listUrl = string.Format("{0}/files.txt?v={1}", Config.User_Config.web_url, random);
        Debugger.Log(listUrl);

        bar.UpdateProgress(0);

        www = new WWW(listUrl);
        endTime = Time.realtimeSinceStartup + 15;
        timeout = true;
        while (Time.realtimeSinceStartup < endTime)
        {
            if (www.isDone)
            {
                if (www.error == null) timeout = false;
                break;
            }
            yield return 0;
        }
        if (timeout)
        {
            gameMgr.OnUpdateFailed(www.error != null 
                ? string.Format("{0}:{1}", LanguageTips.CHECK_UPDATE_FAILED, www.error) 
                : LanguageTips.CHECK_UPDATE_TIMEOUT);
            www.Dispose();
            yield break;
        }
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        List<string> needDownloadFiles = new List<string>();
        downloadedSize = 0;
        //curFileSize = 0;
       
        filesText = www.text;
        www.Dispose();

        string[] files = filesText.Split('\n');
        totalSize = gameMgr.GetTotalDownloadSize(files, needDownloadFiles);

        if (totalSize > 0)
        {
            bool networkAvaliable = false;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                MessageBox.Show(LanguageTips.NETWORK_NOT_REACHABLE, (result) =>
                {
                    if (bar != null)
                    {
                        StartUpController.Instance.clickEvent += gameMgr.CheckUpdateClick;
                        bar.SetMessage(LanguageTips.CLICK_SCREEN_UPDATE);
                    }
                });
                yield break;
            }
            else if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                MessageBox.Show(LanguageTips.NETWORK_VIA_CARRIERDATA, (result) =>
                    {
                        if (result == DialogResult.Yes)
                        {
                            networkAvaliable = true;
                        }
                        else if (result == DialogResult.No)
                        {
                            if (bar != null)
                            {
                                StartUpController.Instance.clickEvent += gameMgr.CheckUpdateClick;
                                bar.SetMessage(LanguageTips.CLICK_SCREEN_UPDATE);
                            }
                        }
                    });
            }
            else
            {
                networkAvaliable = true;
            }
            while (!networkAvaliable)
            {
                yield return null;
            }
        }

        //UpdateScene.Instance.SetMessage(UpdateTips.CHECK_UPDATE);
        yield return null;

        //启动多线程下载
        //startMulThreadUpdate(needDownloadFiles);
        startResumingUpdate(needDownloadFiles);
    }

    public void UpdateResumingDownloadProgress()
    {
        if (resumingUpdates != null && resumingUpdates.Count > 0)
        {
            ResumingUpdateExecutor executor = resumingUpdates[0] as ResumingUpdateExecutor;
            if (executor != null)
            {
                long curSize = downloadedSize + executor.GetCurrentDownloadSize();
                float percent = (float)curSize / (float)totalSize;
                bar.SetMessage(string.Format("{0}（{1}/{2}）", LanguageTips.DOWNLOAD_FILE_ING,
                                        GameManager.GetSizeString(curSize),
                                        GameManager.GetSizeString(totalSize)));
                bar.UpdateProgress(percent);
            }
        }
    }

    /// <summary>
    /// 启动多线程解压
    /// </summary>
    private void startMulThreadUpdate(List<string> needDownloadFiles)
    {
        bar.UpdateProgress(0);

        string dataPath = Util.DataPath;  //数据目录
        string url = Config.User_Config.web_url + "/";
        string random = DateTime.Now.ToString("yyyymmddhhmmss");

        List<ThreadUpdateExecutor> updates = new List<ThreadUpdateExecutor>();
        for (int i = 0; i < needDownloadFiles.Count; i++)
        {
            var file = needDownloadFiles[i].TrimEnd('\r');
            string[] keyValue = file.Split('|');

            string f = keyValue[0];
            string localfile = (dataPath + f).Trim();

            string fileUrl = string.Concat(url , keyValue[0] , "?v=" , random);
            string md5 = keyValue[1].Trim();

            string localMd5 = PlayerPrefs.GetString(f + "_md5", "");
            if (!localMd5.Equals(md5) || !File.Exists(localfile))
            {
                //更新：
                //1.本地文件与资源服不一致
                //2.本地文件丢失
                ThreadUpdateExecutor updateExe = new ThreadUpdateExecutor();

                updateExe.DownloadUrl = fileUrl;
                updateExe.LocalPath = localfile;
                updateExe.FileName = f;
                updateExe.SrcMD5 = md5;
                updateExe.FileSize = Convert.ToInt64(keyValue[2]);
                updateExe.OnComplete = OnUpdateChangeStatus;
                updates.Add(updateExe);
            }
            else
            {
                downloadedSize += Convert.ToInt64(keyValue[2]);
            }
        }
        
        if (updates.Count <= 0)
        {
            //异常情况： 如果不存在更新文件时
            this.onThreadWorkComplete(null);
        }
        else
        {
            ThreadManager threadMgr = AppFacade.Instance.GetManager<ThreadManager>();
            threadMgr.PoolScheduler.StartASyncThreads(updates.ToArray(), onThreadWorkComplete, onWorkObjectDone, maxThreadCount);

            //更新当前的进度
            float percent = (float)downloadedSize / (float)totalSize;
            bar.SetMessage(string.Format("{0}（{1}/{2}）", LanguageTips.DOWNLOAD_FILE_ING,
                                    GameManager.GetSizeString(downloadedSize),
                                    GameManager.GetSizeString(totalSize)));
            bar.UpdateProgress(percent);
        }
    }

    private void startResumingUpdate(List<string> needDownloadFiles)
    {
        bar.UpdateProgress(0);

        string dataPath = Util.DataPath;  //数据目录
        string url = Config.User_Config.web_url + "/";
        string random = DateTime.Now.ToString("yyyymmddhhmmss");

        resumingUpdates = new List<IThreadWorkerObject>();
        for (int i = 0; i < needDownloadFiles.Count; i++)
        {
            var file = needDownloadFiles[i].TrimEnd('\r');
            string[] keyValue = file.Split('|');

            string f = keyValue[0];
            string localfile = (dataPath + f).Trim();

            string fileUrl = string.Concat(url, keyValue[0], "?v=", random);
            string md5 = keyValue[1].Trim();

            string localMd5 = PlayerPrefs.GetString(f + "_md5", "");
            if (!localMd5.Equals(md5) || !File.Exists(localfile))
            {
                //更新：
                //1.本地文件与资源服不一致
                //2.本地文件丢失
                ResumingUpdateExecutor updateExe = new ResumingUpdateExecutor();

                updateExe.DownloadUrl = fileUrl;
                updateExe.LocalPath = localfile;
                updateExe.FileName = f;
                updateExe.SrcMD5 = md5;
                updateExe.FileSize = Convert.ToInt64(keyValue[2]);
                updateExe.OnComplete = OnUpdateChangeStatus;
                updateExe.OnWorkDone = StartResumingDownload;
                resumingUpdates.Add(updateExe);
            }
            else
            {
                downloadedSize += Convert.ToInt64(keyValue[2]);
            }
        }

        if (resumingUpdates.Count <= 0)
        {
            //异常情况： 如果不存在更新文件时
            this.onThreadWorkComplete(null);
        }
        else
        {
            //更新当前的进度
            float percent = (float)downloadedSize / (float)totalSize;
            bar.SetMessage(string.Format("{0}（{1}/{2}）", LanguageTips.DOWNLOAD_FILE_ING,
                                    GameManager.GetSizeString(downloadedSize),
                                    GameManager.GetSizeString(totalSize)));
            bar.UpdateProgress(percent);
            resumingUpdates[0].ExecuteThreadedWork();
        }
    }

    private void StartResumingDownload(IThreadWorkerObject workerObj)
    {
        onResumingWorkerObjectDone(workerObj);
        if (resumingUpdates.Count > 0)
            resumingUpdates.Remove(workerObj);
        if (resumingUpdates.Count > 0)
            resumingUpdates[0].ExecuteThreadedWork();
        else
            onThreadWorkComplete(null);
    }

    private void onResumingWorkerObjectDone(IThreadWorkerObject finishedObject)
    {
        ResumingUpdateExecutor updateExe = finishedObject as ResumingUpdateExecutor;
        Debugger.Log(string.Format("Download file>>{0}", updateExe.DownloadUrl));

        
        downloadedSize += updateExe.FileSize;
        float percent = (float)downloadedSize / (float)totalSize;
        bar.SetMessage(string.Format("{0}（{1}/{2}）", LanguageTips.DOWNLOAD_FILE_ING,
            GameManager.GetSizeString(downloadedSize),
            GameManager.GetSizeString(totalSize)));
        bar.UpdateProgress(percent);

        downloadFiles.Add(updateExe.FileName);

        //检查下载文件的合法性，是否更新一致
        if (!updateExe.SrcMD5.Equals(updateExe.DownloadFileMD5))
        {
            if (!downloadErrorFiles.Contains(updateExe.FileName)) downloadErrorFiles.Add(updateExe.FileName);
        }
        else
        {
            if (downloadErrorFiles.Contains(updateExe.FileName)) downloadErrorFiles.Remove(updateExe.FileName);

            if (beginPackDownload)
                Util.CallMethod("Game", "OnOnePackFileDownload", updateExe.LocalPath);
            LuaHelper.GetGameManager().SetMd5(updateExe.FileName, updateExe.DownloadFileMD5, downloadFiles.Count % 3 == 0);
        }

    }


    /// <summary>
    /// 单个下载完成
    /// </summary>
    /// <param name="finishedObject"></param>
    private void onWorkObjectDone(IThreadWorkerObject finishedObject)
    {
        ResumingUpdateExecutor updateExe = finishedObject as ResumingUpdateExecutor;
        Debugger.Log(string.Format("Download file>>{0}", updateExe.DownloadUrl));

        
        downloadedSize += updateExe.FileSize;
        float percent = (float)downloadedSize / (float)totalSize;
        bar.SetMessage(string.Format("{0}（{1}/{2}）", LanguageTips.DOWNLOAD_FILE_ING, 
            GameManager.GetSizeString(downloadedSize),
            GameManager.GetSizeString(totalSize)));
        bar.UpdateProgress(percent);

        downloadFiles.Add(updateExe.FileName);

        //检查下载文件的合法性，是否更新一致
        if (!updateExe.SrcMD5.Equals(updateExe.DownloadFileMD5))
        {
            if (!downloadErrorFiles.Contains(updateExe.FileName)) downloadErrorFiles.Add(updateExe.FileName);
        }
        else
        {
            if (downloadErrorFiles.Contains(updateExe.FileName)) downloadErrorFiles.Remove(updateExe.FileName);
            
            if (beginPackDownload)
                Util.CallMethod("Game", "OnOnePackFileDownload", updateExe.LocalPath);
            LuaHelper.GetGameManager().SetMd5(updateExe.FileName, updateExe.DownloadFileMD5 , downloadFiles.Count % 3 == 0);
        }
        
    }

    /// <summary>
    /// 全部下载解压完成
    /// </summary>
    /// <param name="finishedObject"></param>
    private void onThreadWorkComplete(IThreadWorkerObject[] finishedObjects)
    {
        GameManager gameMgr = LuaHelper.GetGameManager();
        
        PlayerPrefs.Save();
        if (downloadErrorFiles.Count > 0)
        {
            gameMgr.OnUpdateFailed(LanguageTips.UPDATE_MD5_ERROR);
            return;
        }

        string message = LanguageTips.UPDATE_FINISHED;
        bar.SetMessage(message);
        bar.UpdateProgress(1f);


        File.WriteAllText(Util.DataPath + "files.txt", filesText);

        Util.ClearMemory();

        if (onUpdateFinish != null) onUpdateFinish();
    }


#endregion


    /// <summary>
    /// 更新状态改变
    /// </summary>
    /// <param name="data"></param>
    private void OnUpdateChangeStatus(NotiData data)
    {
        switch (data.evName)
        {

            case NotiConst.UPDATE_EXTRACT_FAILED:
                {
                    Loom.DispatchToMainThread(() =>
                    {
                        var errorMsg = data.extParam.ToString();
                        Debugger.LogError("decompress error: " + errorMsg);
                        if (errorMsg.Contains("disk full", StringComparison.OrdinalIgnoreCase))
                            MessageBox.Show(string.Format("{0}：{1}", LanguageTips.EXTRACT_ERROR, LanguageTips.DISK_FULL));
                        else
                            MessageBox.Show(LanguageTips.EXTRACT_ERROR);
                    });
                }
                break;
            case NotiConst.DOWNLOAD_EXTRACT_FAILED:
                {
                    Loom.DispatchToMainThread(() =>
                    {
                        packDownloadJumpedCount++;
                        Debugger.LogError("Download extract error! " + data.extParam.ToString());
                    });
                }
                break;
            case NotiConst.UPDATE_FAILED:
                {
                    Loom.DispatchToMainThread(() =>
                    {
                        MessageBox.Show(data.extParam.ToString());
                    });
                }
                break;
            case NotiConst.UPDATE_CANCELLED:
                {
                    Loom.DispatchToMainThread(() =>
                    {
                        MessageBox.Show(data.extParam.ToString());
                    });
                }
                break;
        }
    }

#region ------------------本地整包资源释放-----------------------------------------
    /// <summary>
    /// 释放本地的整包资源
    /// </summary>
    /// <returns></returns>
    public IEnumerator OnExtractResources()
    {
        ThreadManager threadMgr = AppFacade.Instance.GetManager<ThreadManager>();

        if (extractStartTime == 0) extractStartTime = Time.realtimeSinceStartup;
        if (extractErrorCount >= 3)
        {
            MessageBox.Show(LanguageTips.READ_FILES_ERROR_QUIT, (result) =>
            {
                Application.Quit();
            });
            yield break;
        }
        extractFiles.Clear();

#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
#else
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            PlayerPrefs.DeleteAll();
#endif

        string dataPath = Util.DataPath;  //数据目录
        string resPath = Path.Combine(Util.StreamingAssetsPath() ,LuaConst.osDir); //游戏包资源目录

        if (Application.platform != RuntimePlatform.WindowsPlayer)
        {
            if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
            Directory.CreateDirectory(dataPath);
        }

        // 解压前先重新加载一次config，防止覆盖安装config文件不能更改的问题
        TextAsset obj = Resources.Load<TextAsset>("config");
        if (obj == null)
        {
            MessageBox.Show(LanguageTips.NO_CONFIG_VIA_MOBLIEPLATFORM, (result) =>
            {
                Application.Quit();
            });
        }
        string config_path = Util.DataPath + "/config.txt";
        if (File.Exists(config_path)) File.Delete(config_path);
        FileManager.WriteFile(config_path, obj.text);
        User_Config.ResetConfig(obj.text);
        User_Config.LoadGlobalSetting();

        string splitFile = Path.Combine(resPath, "split.txt");
        WWW www = new WWW(splitFile);
        yield return www;

        if (www.isDone && www.text == "1") LuaHelper.GetGameManager().downloadAll = true;
        if (www != null) www.Dispose();

        if (LuaHelper.GetGameManager().downloadAll)
        {
            onExtractComplete(null);
            yield break;
        } 

        yield return Yielders.EndOfFrame;
        string infile = Path.Combine(resPath , "packlist.txt");
        string content = string.Empty;
//        Debug.Log(infile);
        www = new WWW(infile);
        yield return www;

        if (www.isDone) content = www.text;
        www.Dispose();

        yield return Yielders.EndOfFrame;

        //释放所有文件到数据目录
		
        string[] files = content.Split('\n');
#if UNITY_IOS
        extractLeftFiles.AddRange(files);
        curSingleStreamingIndex = 0;
        totalSingleStreamingCount = files.Length;
        bar.UpdateProgress(0);
        if (extractLeftFiles.Count > 0)
            bar.SetMessage(LanguageTips.READ_FILES);
        LuaHelper.GetGameManager().StartCoroutine(ExtractOneByOne());
#else
        int curStreamingIndex = 0;
        int totalStreamingCount = files.Length;
        string outfile = string.Empty;
        List<LocalExtractExecutor> les = new List<LocalExtractExecutor>();
        bar.SetMessage(LanguageTips.READ_FILES);
        for (int i = 0; i < files.Length; ++i)
        {
            var file = files[i].Trim();
			curStreamingIndex++;

            var percent1 = (float)curStreamingIndex / (float)totalStreamingCount;
            bar.SetMessage(string.Format("{0}: {1}%", LanguageTips.READ_FILES, (percent1 * 100).ToString("f0")));
            bar.UpdateProgress(percent1);

            if (string.IsNullOrEmpty(file))	continue;

            string[] fs = file.Split('|');
            infile = Path.Combine(resPath , fs[0]);  //

            outfile = dataPath + fs[0];
            string srcMd5 = fs[1].Trim();
            Debug.Assert(fs.Length == 2, "read packlist.txt error:" + file);

            bool isBinFile = outfile.CustomEndsWith(".bin");
            //文件被非法删除,Bin文件无法确认存在，只能直接再释放一次
            var tempMd5 = string.Format("{0}_md5", fs[0]);
            if (isBinFile || !File.Exists(outfile))
            {
                PlayerPrefs.DeleteKey(fs[0]);
                PlayerPrefs.DeleteKey(tempMd5);
            }

            if (srcMd5 == PlayerPrefs.GetString(tempMd5))
            {
                if (isBinFile)  totalExtractCount++;
                continue;
            }

            WWW  streamingWWW = new WWW(infile);
            yield return streamingWWW;
            if (streamingWWW.error != null)
            {
                Debug.LogError(infile);
                continue;
            }
            if (streamingWWW.isDone)
            {
                string dir = Path.GetDirectoryName(outfile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (isBinFile)
                {
                    LocalExtractExecutor lee = new LocalExtractExecutor();
                    lee.LocalPath = dataPath;
                    lee.FileName = fs[0];
                    lee.SrcMD5 = srcMd5;
                    lee.FileBytes = streamingWWW.bytes;
                    lee.OnComplete = OnUpdateChangeStatus;

                    totalExtractCount++;
                    les.Add(lee);
                    extractFiles.Add(fs[0]);
                }
                else
                {
                    File.WriteAllBytes(outfile, streamingWWW.bytes);
                    PlayerPrefs.SetInt(fs[0], 1);
                    PlayerPrefs.SetString(tempMd5, srcMd5);
                    if (curStreamingIndex % 3 == 0)  //每写入3个文件后保存记录，减少IO
                        PlayerPrefs.Save();
//                    Debugger.Log("正在解包文件:>" + infile) ;
                }
                
            }
            streamingWWW.Dispose();
        }
        
        yield return Yielders.EndOfFrame;
        //启动多线程解压
        //		Debugger.Log("MaxThreadCount:" + maxThreadCount);
        bar.UpdateProgress(0);
        if (les.Count > 0)
        {
            bar.SetMessage(LanguageTips.EXTRACT_PACKAGE);
            threadMgr.PoolScheduler.StartASyncThreads(les.ToArray(), onExtractComplete, onExtractDone, maxThreadCount);
        }
        else
            onExtractComplete(null);
#endif
    }

    public IEnumerator ExtractOneByOne()
    {
        if (extractLeftFiles.Count == 0)
        {
            LuaHelper.GetGameManager().StartCoroutine(WaitForThreadExtractDone());
            yield break;
        }
        var file = extractLeftFiles[0].Trim();
        curSingleStreamingIndex++;

        string dataPath = Util.DataPath;  //数据目录
        string resPath = Path.Combine(Util.StreamingAssetsPath(), LuaConst.osDir); //游戏包资源目录

        var percent1 = (float)curSingleStreamingIndex / (float)totalSingleStreamingCount;
        bar.SetMessage(string.Format("{0}: {1}%", LanguageTips.LOADING_LUA_ING, (percent1 * 100).ToString("f0")));
        bar.UpdateProgress(percent1);

        if (string.IsNullOrEmpty(file))
        {
            extractLeftFiles.RemoveAt(0);
            LuaHelper.GetGameManager().StartCoroutine(ExtractOneByOne());
            yield break;
        }

        string[] fs = file.Split('|');
        string realFileName = fs[0];
        if (realFileName.CustomEndsWith(".unity3d"))
        {
            string newStr = Convert.ToBase64String(Crypto.Encode(Encoding.GetBytes(Path.GetFileName(realFileName))));
            if (newStr.Contains("/"))
                newStr = newStr.Replace("/", "");
            realFileName = string.Format("lua/{0}.unity3d", newStr);
        }
        var infile = Path.Combine(resPath, realFileName);  //

        var outfile = dataPath + fs[0];
        string srcMd5 = fs[1].Trim();
        Debug.Assert(fs.Length == 2, "read packlist.txt error:" + file);

        bool isBinFile = outfile.CustomEndsWith(".bin");
        //文件被非法删除,Bin文件无法确认存在，只能直接再释放一次
        var tempMd5 = string.Format("{0}_md5", fs[0]);
        if (isBinFile || !File.Exists(outfile))
        {
            PlayerPrefs.DeleteKey(fs[0]);
            PlayerPrefs.DeleteKey(tempMd5);
        }

        if (srcMd5 == PlayerPrefs.GetString(tempMd5))
        {
            if (isBinFile) totalExtractCount++;
            extractLeftFiles.RemoveAt(0);
            LuaHelper.GetGameManager().StartCoroutine(ExtractOneByOne());
            yield break;
        }

        WWW streamingWWW = new WWW(infile);
        yield return streamingWWW;
        if (streamingWWW.error != null)
        {
            Debug.LogError(infile);
            extractLeftFiles.RemoveAt(0);
            LuaHelper.GetGameManager().StartCoroutine(ExtractOneByOne());
            yield break;
        }
        if (streamingWWW.isDone)
        {
            List<LocalExtractExecutor> les = new List<LocalExtractExecutor>();
            string dir = Path.GetDirectoryName(outfile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (isBinFile)
            {
                while (extractFiles.Count >= 2) yield return null;
                LocalExtractExecutor lee = new LocalExtractExecutor();
                lee.LocalPath = dataPath;
                lee.FileName = fs[0];
                lee.SrcMD5 = srcMd5;
                lee.FileBytes = streamingWWW.bytes;
                lee.OnComplete = OnUpdateChangeStatus;

                totalExtractCount++;
                les.Add(lee);
                extractFiles.Add(fs[0]);
                Amib.Threading.SmartThreadPool.Instance.QueueWorkItem(() =>
                {
                    lee.ExecuteThreadedWork();
                    Loom.QueueOnMainThread(() =>
                    {
                        onExtractDone(lee);
                    });
                });
            }
            else
            {
                File.WriteAllBytes(outfile, streamingWWW.bytes);
                PlayerPrefs.SetInt(fs[0], 1);
                PlayerPrefs.SetString(tempMd5, srcMd5);
                if (curSingleStreamingIndex % 3 == 0)  //每写入3个文件后保存记录，减少IO
                    PlayerPrefs.Save();
                //                    Debugger.Log("正在解包文件:>" + infile) ;
                
            }
        }
        streamingWWW.Dispose();
        extractLeftFiles.RemoveAt(0);
        LuaHelper.GetGameManager().StartCoroutine(ExtractOneByOne());
    }

    IEnumerator WaitForThreadExtractDone()
    {
        bar.UpdateProgress(0);
        while (extractFiles.Count > 0)
        {
            var percent = (double)(totalExtractCount - extractFiles.Count) / (double)totalExtractCount;
            bar.SetMessage(string.Format("{0}:{1}%", LanguageTips.EXTRACT_PACKAGE, (percent * 100).ToString("f0")));
            bar.UpdateProgress((float)percent);
            yield return null;
        }
        onExtractComplete(null);
    }

    /// <summary>
    /// 解压释放单个文件
    /// </summary>
    /// <param name="finishedObject"></param>
    private void onExtractDone(IThreadWorkerObject finishedObject)
    {
        LocalExtractExecutor lee = finishedObject as LocalExtractExecutor;

        // 文件本身
        //PlayerPrefs.SetInt(lee.FileName , 1);
        //PlayerPrefs.SetString(lee.FileName + "_md5", lee.FileMD5);

        //解压出来的文件
        foreach (string key in lee.SubFileMD5Dic.Keys)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.SetString(key + "_md5", lee.SubFileMD5Dic[key]);
        }
        PlayerPrefs.Save();

        extractFiles.Remove(lee.FileName);
        //Debugger.Log("正在解包文件:>" + lee.FileName);
        //监听解压进度
#if !UNITY_IOS
        var percent = (double)(totalExtractCount - extractFiles.Count) / (double)totalExtractCount;
        bar.SetMessage(string.Format("{0}:{1}%", LanguageTips.EXTRACT_PACKAGE, (percent * 100).ToString("f0")));
        bar.UpdateProgress((float)percent);
#endif
        Util.ClearMemory();
    }

    /// <summary>
    /// 全部解压完成
    /// </summary>
    /// <param name="finishedObject"></param>
    private void onExtractComplete(IThreadWorkerObject[] finishedObjects)
    {
        string message = LanguageTips.EXTRACT_FINISHED;
        Debugger.Log(string.Format("启动：{0}", message));
        bar.SetMessage(message);

        File.WriteAllText(Util.DataPath + "extract.txt", "1");

        Util.ClearMemory();

        string localVersionFile = Path.Combine(Util.DataPath, "version.txt");
        GameManager.localVersion = GameVersion.CreateVersion(localVersionFile, GameManager.packVersion.ToString());

        if (onExtractFinish != null) onExtractFinish();
    }

#endregion
}
