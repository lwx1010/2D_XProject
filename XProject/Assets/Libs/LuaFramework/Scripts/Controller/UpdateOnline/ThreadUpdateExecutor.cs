
using System;
using System.IO;
using Frankfort.Threading;
using Ihaius;
using Riverlake.Crypto;

public class ThreadUpdateExecutor : IThreadWorkerObject
{
    public string DownloadUrl;

    public string LocalPath;

    public string FileName;

    public string SrcMD5;

    public Action<NotiData> OnComplete;

    private bool isAborted;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize;
    /// <summary>
    /// 下载文件的MD5
    /// </summary>
    public string DownloadFileMD5 { get; private set; }
    public ThreadUpdateExecutor()
    {

    }
    public void ExecuteThreadedWork()
    {
        DownloadFile download = new DownloadFile(DownloadUrl , LocalPath);

        byte[] fileBytes = download.DownloadHttp();

        if (fileBytes == null || fileBytes.Length <= 0)
        {
            Loom.DispatchToMainThread(() =>
            {
                NotiData data = new NotiData(NotiConst.UPDATE_FAILED, this.FileName, LanguageTips.UPDATE_FAILED);
                if (OnComplete != null) OnComplete(data);  //回调逻辑层
                //MessageBox.DisplayMessageBox(string.Format("{0}: {1}", UpdateTips.UPDATE_FAILED, FileName), 0, (go) =>
                //{
                //    AssetsCtrl.downloadFailed = true;
                //});
            });

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
            if (OnComplete != null) OnComplete(data);  //回调逻辑层
        }
    }
    
    public void AbortThreadedWork()
    {
        isAborted = true;
    }
}
