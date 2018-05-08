using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using System;
using System.ComponentModel;
using Frankfort.Threading;
using Amib.Threading;

public sealed class ThreadEvent {
    public string Key;
    public List<object> evParams = new List<object>();
    public Action<NotiData> func;
}

public sealed class NotiData {
    public string evName;
    public object evParam;
    public object extParam;

    public NotiData(string name, object param, object extParam = null) {
        this.evName = name;
        this.evParam = param;
        this.extParam = extParam;
    }
}

namespace LuaFramework {
    /// <summary>
    /// 当前线程管理器，同时只能做一个任务
    /// </summary>
    public sealed class ThreadManager : Manager {
        static readonly object m_lockObject = new object();
		
        public ThreadPoolScheduler PoolScheduler { get; private set; }

        void Awake() {
            ThreadPool.SetMaxThreads(1, 1);
			
			PoolScheduler = Loom.CreateThreadPoolScheduler("_ThreadPoolScheduler");
			DontDestroyOnLoad(PoolScheduler.gameObject);
        }

        // Use this for initialization
        void Start() {
            //thread.Start();
        }

        public void Dispose()
        {
            if (PoolScheduler != null)
                PoolScheduler.AbortASyncThreads();

            SmartThreadPool.Instance.Dispose();
        }

        public void DestroySelf()
        {
            Destroy(this);
        }

        public void AddEvent(ThreadEvent ev)
        {
            lock (m_lockObject)
            {
                switch (ev.Key)
                {
                    case NotiConst.DOWNLOAD_EXTRACT:
                        {
                            SmartThreadPool.Instance.QueueWorkItem(() =>
                            {
                                //下载文件
                                OnExtractDownloadFile(ev.evParams, ev.func);
                            });
                            //ThreadPool.QueueUserWorkItem((i) =>
                            //{
                            //    //下载文件
                            //    OnExtractDownloadFile(ev.evParams, ev.func);
                            //});
                        }
                        break;
                }
            }
        }

        void OnExtractDownloadFile(List<object> evParams, Action<NotiData> func)
        {
            var dataPath = evParams[0] as string;
            var dataName = evParams[1] as string;
            var buffer = evParams[2] as byte[];
            try
            {
                using (var decompressor = new ZstdNet.Decompressor())
                {
                    var path = dataPath + dataName;
                    var bytes = decompressor.Unwrap(buffer);
                    if (File.Exists(path)) File.Delete(path);
                    File.WriteAllBytes(path, bytes);
                    NotiData data = new NotiData(NotiConst.DOWNLOAD_EXTRACT, dataName, path);
                    if (func != null) func(data);  //回调逻辑层
                }
            }
            catch (Exception e)
            {
                NotiData data = new NotiData(NotiConst.DOWNLOAD_EXTRACT_FAILED, dataName, e.Message);
                if (func != null) func(data);  //回调逻辑层
            }
        }

        /// <summary>
        /// 应用程序退出
        /// </summary>
        void OnDestroy() {
            //thread.Abort();
        }
    }
}