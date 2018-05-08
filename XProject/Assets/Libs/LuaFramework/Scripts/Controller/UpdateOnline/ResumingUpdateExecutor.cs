using UnityEngine;
using System;
using System.IO;
using System.Collections;
using Frankfort.Threading;
using Ihaius;
using Riverlake.Crypto;

public class ResumingUpdateExecutor : IThreadWorkerObject
{
    public string DownloadUrl;

    public string LocalPath;

    public string FileName;

    public string SrcMD5;

    public Action<NotiData> OnComplete;

    public Action<IThreadWorkerObject> OnWorkDone;

    private bool isAborted;
    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize;
    /// <summary>
    /// 下载文件的MD5
    /// </summary>
    public string DownloadFileMD5 { get; private set; }

    private ResumingDownloadFile download;

    public ResumingUpdateExecutor()
    {

    }
    public void ExecuteThreadedWork()
    {
        download = new ResumingDownloadFile(DownloadUrl, LocalPath);

        download.onCompleted += OnFileDownloaded;
        download.onCancelled += OnDownloadCancelled;
        download.onGetFileWriteException += OnDownloadCancelled;

        download.StartDownload();
    }

    public long GetCurrentDownloadSize()
    {
        if (download != null)
            return download.GetCurrentDownloadSize();
        return 0;
    }

    void OnFileDownloaded(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length <= 0)
        {
            NotiData data = new NotiData(NotiConst.UPDATE_FAILED, this.FileName, LanguageTips.UPDATE_FAILED);
            if (OnComplete != null) OnComplete(data);  //回调逻辑层
            //MessageBox.DisplayMessageBox(string.Format("{0}: {1}", UpdateTips.UPDATE_FAILED, FileName), 0, (go) =>
            //{
            //    AssetsCtrl.downloadFailed = true;
            //});

            if (OnWorkDone != null) OnWorkDone(this);
            download = null;
            return;
        }

        string path = Path.GetDirectoryName(LocalPath);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (LocalPath.CustomEndsWith(".ab"))
        {
            BeginDecompressExtract(LocalPath, fileBytes);
        }
        else
        {
            DownloadFileMD5 = MD5.ComputeHashString(fileBytes);
            if (DownloadFileMD5 != SrcMD5) throw new Exception(LanguageTips.UPDATE_MD5_ERROR);
            if (File.Exists(LocalPath)) File.Delete(LocalPath);
            File.WriteAllBytes(LocalPath, fileBytes);
        }
        download = null;
        if (OnWorkDone != null) OnWorkDone(this);
    }

    void OnDownloadCancelled(Exception e)
    {
        NotiData data = new NotiData(NotiConst.UPDATE_CANCELLED, this.FileName, LanguageTips.UPDATE_CANCELLED);
        if (OnComplete != null) OnComplete(data);  //回调逻辑层
        if (OnWorkDone != null) OnWorkDone(this);
        download = null;
    }

    /// <summary>
    /// 开始解压缩
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="buffer"></param>
    private void BeginDecompressExtract(string localFilePath, byte[] buffer)
    {
        try
        {
            using (var decompressor = new ZstdNet.Decompressor())
            {
                var path = localFilePath;
                var bytes = decompressor.Unwrap(buffer);
                if (File.Exists(path)) File.Delete(path);
                File.WriteAllBytes(path, bytes);

                //下载文件中的MD5=未压缩时文件的MD5
                DownloadFileMD5 = MD5.ComputeHashString(bytes);
            }
        }
        catch (Exception e)
        {
            NotiData data = new NotiData(NotiConst.DOWNLOAD_EXTRACT_FAILED, this.FileName, e.Message);
            if (File.Exists(localFilePath)) File.Delete(localFilePath);
            if (OnComplete != null) OnComplete(data);  //回调逻辑层
        }
    }

    public void AbortThreadedWork()
    {
        isAborted = true;
    }
}
