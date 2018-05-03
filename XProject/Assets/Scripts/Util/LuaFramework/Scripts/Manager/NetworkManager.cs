using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using Riverlake.Network;

namespace LuaFramework {
    public sealed class NetworkManager : Manager {

        private SocketClient socket;
        static readonly object m_lockObject = new object();
        static Queue<KeyValuePair<int, ByteBuffer>> mEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        static Queue<KeyValuePair<int, ByteBuffer>> mWeakEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        private float lastPauseTime;
        public bool inBackground { get; private set; }
        public bool inCharge { get; set; }
        public static bool inGame { get; set; }
        public static bool isKickOut { get; set; }

        public bool lostConnect { get { return observer.lostConnect; } }

        private ConnectObserver observer;

        private float heartBeatDuration = 0;

        SocketClient SocketClient {
            get {
                if (socket == null)
                    socket = new SocketClient(OnReconnected, LostConnect);
                return socket;                    
            }
        }

        void OnReconnected()
        {
            if (observer.lostConnect)
                ResetConnect();
        }

        void Awake() {
            ConnectObserver.eventListener = AddEvent;
            observer = new ConnectObserver(OnLostConnect, KickOut);
        }

        public void OnInit() {
            CallMethod("Start");
        }

        internal void Unload() {
            Close();
            CallMethod("Unload");
        }

        public void Close()
        {
            mEvents.Clear();
            mWeakEvents.Clear();
            ResetConnect();
            SocketClient.StopHeartBeat();
            SocketClient.OnRemove();
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        public object[] CallMethod(string func, params object[] args) {
            return Util.CallMethod("Network", func, args);
        }

        ///------------------------------------------------------------------------------------
        public static void AddEvent(int _event, ByteBuffer data) {
            try
            {
                if (_event == 0) throw new KeyNotFoundException("0 key is not exist");
                if (_event == Protocal.WeakMessage)
                {
                    lock (m_lockObject)
                    {
                        mWeakEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
                    }
                }
                else
                {
                    lock (m_lockObject)
                    {
                        mEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Network AddEvent error: {0}\nStackTrace:{1}", e.Message, e.StackTrace));
            }
        }

        void UpdateMessage(Queue<KeyValuePair<int, ByteBuffer>> events)
        {
            if (events.Count > 0)
            {
                while (events.Count > 0)
                {
                    lock (m_lockObject)
                    {
                        KeyValuePair<int, ByteBuffer> _event = events.Dequeue();
                        if (_event.Key == 0)
                        {
                            Debug.LogWarning("receive msg that event key = 0");
                            if (!lostConnect && !inBackground) LostConnect();
                            events.Clear();
                        }
                        else if (inGame)
                        {
                            if (_event.Key == Protocal.Exception || _event.Key == Protocal.Disconnect)
                            {
                                if (!lostConnect && !inBackground) LostConnect();
                                events.Clear();
                            }
                        }
                        facade.SendMessageCommand(NotiConst.DISPATCH_MESSAGE, _event);
                    }
                }
            }
        }

        /// <summary>
        /// 交给Command，这里不想关心发给谁。
        /// </summary>
        private void FixedUpdate()
        {
            if (isKickOut) return;
            SocketClient.UpdateHeartBeat2Server(Time.fixedDeltaTime);

            UpdateMessage(mEvents);
            UpdateMessage(mWeakEvents);

            if (inBackground) return;
            if (observer != null) observer.UpdateConnectStatus();
        }

        void OnLostConnect()
        {
            if (SocketClient != null)
                SocketClient.Close();

            StartCoroutine(BeginReconnect());
            //NetworkManager.AddEvent(Protocal.BeginReconnect, new ByteBuffer());
            //SendConnect();
            //Debug.LogWarning("reconnecting to the server...");
        }

        IEnumerator BeginReconnect()
        {
            yield return Yielders.GetWaitForSeconds(1);
            NetworkManager.AddEvent(Protocal.BeginReconnect, new ByteBuffer());
            SendConnect();
            Debug.LogWarning("reconnecting to the server...");
        }

        /// <summary>
        /// 发送链接请求
        /// </summary>
        public void SendConnect() {
            SocketClient.SendConnect(AppConst.SocketAddress, AppConst.SocketPort);
        }

        public void StartHeartBeat()
        {
            ConnectObserver.reconnectCount = 0;
            SocketClient.StartHeartBeat();
        }

        void LostConnect()
        {
            if (observer != null)
                observer.LostConnect(() =>
                {
                    Loom.QueueOnMainThread(() =>
                    {
                        Debugger.LogWarning("lost connect from server");
                        SocketClient.StopHeartBeat();
                    });
                });
        }

        void ResetConnect()
        {
            if (observer != null)
                observer.ResetConnect();
        }

        public void KickOut(int protocal)
        {
            ResetConnect();
            SocketClient.DisconnectFromServer(protocal);
        }

        public void SetPowerSaveMode(bool bSet)
        {
            if (ConnectObserver.powerSaveMode && !bSet)
                SocketClient.ResetTimeout();

            ConnectObserver.powerSaveMode = bSet;
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(ByteBuffer buffer) {
            SocketClient.SendMessage(buffer);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        void OnDestroy() {
            Close();
            observer = null;
            ConnectObserver.eventListener = null;
            Debug.Log("~NetworkManager was destroy");
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                inBackground = true;
                lastPauseTime = Time.realtimeSinceStartup;
            }
            else
            {
                //SocketClient.ResetTimeout();
                if (!inCharge)
                {
                    inBackground = false;
                    SocketClient.CheckHeartBeat();
                    //if (!SocketClient.IsConnected())
                    //{
                    //    KickOut();
                    //}
                }
                inCharge = false;
            }
            if (SocketClient.IsConnected())
                Util.CallMethod("Game", "OnApplicationPause", paused, observer.lostConnect);
        }
    }
}