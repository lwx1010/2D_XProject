using UnityEngine;
using System.Collections;
using Toqe.Downloader.Business.Download;
using Toqe.Downloader.Business.Contract.Events;
using Toqe.Downloader.Business.Utils;
using Toqe.Downloader.Business.DownloadBuilder;
using Toqe.Downloader.Business.Contract;
using Toqe.Downloader.Business.Observer;
using System.IO;
using System;
using LuaInterface;

namespace Ihaius
{
    public class ResumingDownloadFile
    {
        public Action<byte[]> onCompleted;
        public Action<Exception> onCancelled;
        public Action<Exception> onGetFileWriteException;

        private DownloadSpeedMonitor speedMonitor;
        private DownloadProgressMonitor progressMonitor;
        private ResumingDownload downloader;

        private string downloadUrl;
        private string localPath;

        private long totalLength;

        public ResumingDownloadFile(string downloadUrl, string savePath)
        {
            this.downloadUrl = downloadUrl;
            this.localPath = string.Format("{0}.tmp", savePath);
        }

        public void StartDownload()
        {
            Debugger.Log("start download: {0}", downloadUrl);
            var url = new Uri(downloadUrl);
            var requestBuilder = new SimpleWebRequestBuilder();
            bool needDownload = false;
            long fileLength = 0;
            string path = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (FileStream fileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileLength = fileStream.Length;
                totalLength = requestBuilder.GetFileLength(downloadUrl);
                if (fileLength < totalLength) needDownload = true;
            }
            if (needDownload)
            {
                var dlChecker = new DownloadChecker();
                var httpDlBuilder = new SimpleDownloadBuilder(requestBuilder, dlChecker);
                var timeForHeartbeat = 3000;
                var timeToRetry = 5000;
                var maxRetries = 8;
                var bufferSize = 2048;
                downloader = new ResumingDownload(url, bufferSize, fileLength, null, timeForHeartbeat, timeToRetry, maxRetries, httpDlBuilder);
                speedMonitor = new DownloadSpeedMonitor(128);
                speedMonitor.Attach(downloader);
                progressMonitor = new DownloadProgressMonitor();
                progressMonitor.Attach(downloader);

                var dlSaver = new DownloadToFileSaver(localPath, OnGetFileWriteException);
                dlSaver.Attach(downloader);
                downloader.DownloadCompleted += OnCompleted;
                downloader.DownloadCancelled += OnCancelled;
                downloader.Start();
            }
            else
            {
                OnCompleted(null);
            }
        }

        public long GetCurrentDownloadSize()
        {
            if (downloader != null && progressMonitor != null)
            {
                var progress = progressMonitor.GetCurrentProgressPercentage(downloader);
                return (long)(totalLength * progress);
            }
            return 0;
        }

        void OnCompleted(DownloadEventArgs args)
        {
            try
            {
                if (args != null)
                    args.Download.DetachAllHandlers();
                Loom.QueueOnMainThread(OnCompletedWithMainThread);
            }
            catch (Exception e)
            {
                OnCompleteException(e);
            }
        }

        void OnCompletedWithMainThread()
        {
            try
            {
                Debugger.Log("download success");
                byte[] data = File.ReadAllBytes(localPath);
                File.Delete(localPath);
                if (onCompleted != null) onCompleted(data);
                if (downloader != null)
                    downloader.Dispose();
            }
            catch (Exception ex)
            {
                OnCompleteException(ex);
            }
        }

        void OnCompleteException(Exception e)
        {
            Loom.QueueOnMainThread(() =>
            {
                Debugger.LogException(e);
                if (onCompleted != null) onCompleted(null);
                if (downloader != null)
                    downloader.Dispose();
            });
        }

        void OnCancelled(DownloadCancelledEventArgs args)
        {
            Loom.QueueOnMainThread(() =>
            {
                if (onCancelled != null) onCancelled(args.Exception);
                Debugger.LogException(args.Exception);
                if (downloader != null)
                    downloader.Dispose();
            });
        }

        void OnGetFileWriteException(Exception e)
        {
            Loom.QueueOnMainThread(() =>
            {
                if (onGetFileWriteException != null) onGetFileWriteException(e);
                Debugger.LogException(e);
                if (downloader != null)
                    downloader.Dispose();
            });
        }
    }
}
