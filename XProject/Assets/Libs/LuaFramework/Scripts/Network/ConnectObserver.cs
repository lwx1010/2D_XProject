using UnityEngine;
using System;
using System.Collections;
using LuaInterface;
using LuaFramework;

namespace Riverlake.Network
{
    /// <summary>
    /// 连接观察器
    /// </summary>
    public sealed class ConnectObserver
    {
        /// <summary>
        /// 是否正在重连
        /// </summary>
        public static bool inReconnect = false;
        /// <summary>
        /// 重连次数
        /// </summary>
        public static int reconnectCount = 0;
        /// <summary>
        /// 省电模式
        /// </summary>
        public static bool powerSaveMode = false;
        /// <summary>
        /// 是否失去连接
        /// </summary>
        public bool lostConnect { get; private set; }
        /// <summary>
        /// 重连时间
        /// </summary>
        public float reconnectTime { get; private set; }
        /// <summary>
        /// 重连超时时间
        /// </summary>
        public float reconnectTimeout { get; private set; }

        public delegate void AddEvent(int eventId, ByteBuffer buffer);

        public static AddEvent eventListener;

        private const int RECONNECT_MAX_COUNT = 3;
        private const float RECONNECT_DURATION = 5f;
        private const float RECONNECT_TIMEOUT = 10;

        public delegate void OnLostConnect();
        public delegate void OnMaxCountReached(int protocal);
        OnLostConnect lostConnectAction;
        OnMaxCountReached maxCountReachedAction;

        public ConnectObserver(OnLostConnect lostAct, OnMaxCountReached countAct)
        {
            lostConnect = false;
            lostConnectAction = lostAct;
            maxCountReachedAction = countAct;
        }

        /// <summary>
        /// 失去连接
        /// </summary>
        /// <param name="action"></param>
        public void LostConnect(Action action)
        {
            inReconnect = false;
            lostConnect = true;
            reconnectTime = 0;
            if (action != null) action();
        }

        /// <summary>
        /// 重置连接
        /// </summary>
        public void ResetConnect()
        {
            lostConnect = false;
            inReconnect = false;
            reconnectTime = 0;
            reconnectCount = 0;
        }

        /// <summary>
        /// 更新连接状态
        /// </summary>
        /// <param name="onLostConnect">失去连接时的操作</param>
        /// <param name="onMaxCountReached">达到重连上限时的操作</param>
        public void UpdateConnectStatus()
        {
            if (lostConnect && !inReconnect)
            {
                reconnectTime -= Time.deltaTime;
                if (reconnectCount > RECONNECT_MAX_COUNT)
                {
                    ResetConnect();
                    if (maxCountReachedAction != null) maxCountReachedAction(Protocal.KickOut);
                }
                else if (reconnectTime <= 0)
                {
                    reconnectTime = RECONNECT_DURATION;
                    reconnectTimeout = RECONNECT_TIMEOUT;
                    inReconnect = true;
                    reconnectCount++;
                    if (lostConnectAction != null) lostConnectAction();
                }
            }
            if (lostConnect && inReconnect)
            {
                reconnectTimeout -= Time.deltaTime;
                if (reconnectTimeout < 0)
                {
                    inReconnect = false;
                }
            }
        }

        public static void DispatchEvent(int eventId, ByteBuffer data)
        {
            if (eventListener != null) eventListener(eventId, data);
        }
    }
}
