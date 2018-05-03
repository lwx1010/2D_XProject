using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using System.Runtime.InteropServices;

namespace Riverlake.Network
{
    public enum DisType
    {
        Exception,
        Disconnect,
    }

    /// <summary>
    /// Socket封装
    /// </summary>
    public sealed class SocketClient
    {

        public static bool loggedIn = false;

        private TcpClient client = null;
        private NetworkStream outStream = null;
        private MemoryStream memStream;
        private BinaryReader reader;

        private const int MAX_READ = 8192;
        private byte[] byteBuffer = new byte[MAX_READ];

        private HeartBeatService heartBeatService;

        public delegate void ReconnectedAction();

        public ReconnectedAction reconnectAction;

        // Use this for initialization
        public SocketClient(ReconnectedAction reconnectAction, HeartBeatService.OnLostConnectAction lostAction)
        {
            heartBeatService = new HeartBeatService(lostAction);
            this.reconnectAction = reconnectAction;
        }

        /// <summary>
        /// 注册代理
        /// </summary>
        void OnRegister()
        {
            if (memStream == null) memStream = new MemoryStream();
            if (reader == null) reader = new BinaryReader(memStream);
        }

        /// <summary>
        /// 移除代理
        /// </summary>
        public void OnRemove()
        {
            this.Close();
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (memStream != null)
            {
                memStream.Close();
                memStream = null;
            }
        }

        /// <summary>
        /// 主动从服务器断开连接
        /// </summary>
        public void DisconnectFromServer(int protocal)
        {
            StopHeartBeat();
            Close();
            if (protocal == 0) protocal = Protocal.KickOut;
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteShort((ushort)protocal);
            ConnectObserver.DispatchEvent(protocal, buffer);
        }

        public void StopHeartBeat()
        {
            heartBeatService.stop();
        }

        public void StartHeartBeat()
        {
            heartBeatService.start();
        }

        public bool isHeartBeatStopped()
        {
            return heartBeatService.isstopped();
        }

        public bool IsConnected()
        {
            return client != null && client.Connected;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        void ConnectServer(string host, int port)
        {
            OnRegister();
            string newHost;
            AddressFamily af;
            getIPType(host, out newHost, out af);
            client = null;
            client = new TcpClient(af);
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;
            client.NoDelay = true;
            try
            {
                client.BeginConnect(newHost, port, new AsyncCallback(OnConnect), null);
            }
            catch (Exception e)
            {
                OnDisconnected(DisType.Exception, e.Message);
            }
        }

        /// <summary>
        /// 连接上服务器
        /// </summary>
        void OnConnect(IAsyncResult asr)
        {
            try
            {
                outStream = client.GetStream();
                client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
                if (!ConnectObserver.inReconnect)
                    ConnectObserver.DispatchEvent(Protocal.Connect, new ByteBuffer());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnDisconnected(DisType.Exception, e.Message);
            }
        }

        /// <summary>
        /// 写数据
        /// </summary>
        void WriteMessage(byte[] message)
        {
            MemoryStream ms = null;
            using (ms = new MemoryStream())
            {
                ms.Position = 0;
                BinaryWriter writer = new BinaryWriter(ms);
                //ushort msglen = (ushort)message.Length;
                //writer.Write(msglen);
                writer.Write(message);
                writer.Flush();
                if (client != null && client.Connected)
                {
                    //NetworkStream stream = client.GetStream(); 
                    byte[] payload = ms.ToArray();
                    outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
                }
                else
                {
                    OnDisconnected(DisType.Disconnect, 
                        string.Format("client is disconnected: {0}", client == null ? "client is null" : Convert.ToString(client.Connected)));
                }
            }
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        void OnRead(IAsyncResult asr)
        {
            int bytesRead = 0;
            try
            {
                if (client != null)
                {
                    lock (client.GetStream())
                    {         //读取字节流到缓冲区
                        bytesRead = client.GetStream().EndRead(asr);
                    }
                    if (bytesRead < 0)
                    {
                        //包尺寸有问题，断线处理
                        OnDisconnected(DisType.Disconnect, string.Format("bytesRead: {0}", bytesRead));
                        return;
                    }
                    OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
                    lock (client.GetStream())
                    {         //分析完，再次监听服务器发过来的新消息
                        Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
                        client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
                    }
                }
            }
            catch (Exception ex)
            {
                OnDisconnected(DisType.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 丢失链接
        /// </summary>
        void OnDisconnected(DisType dis, string msg)
        {
            if (client != null)
            {
                Close();   //关掉客户端链接
                int protocal = dis == DisType.Exception ?
                Protocal.Exception : Protocal.Disconnect;

                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteShort((ushort)protocal);
                ConnectObserver.DispatchEvent(protocal, buffer);
                if (ConnectObserver.inReconnect)
                    ConnectObserver.inReconnect = false;
                Debug.LogWarning(string.Format("Connection was closed:>{0} Distype:>{1}", msg, dis));
            }
        }

        /// <summary>
        /// 打印字节
        /// </summary>
        /// <param name="bytes"></param>
        void PrintBytes()
        {
            string returnStr = string.Empty;
            for (int i = 0; i < byteBuffer.Length; i++)
            {
                returnStr += byteBuffer[i].ToString("X2");
            }
            Debug.LogError(returnStr);
        }

        /// <summary>
        /// 向链接写入数据流
        /// </summary>
        void OnWrite(IAsyncResult r)
        {
            try
            {
                outStream.EndWrite(r);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError(string.Format("OnWrite--->>>{0}", ex.Message));
            }
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        void OnReceive(byte[] bytes, int length)
        {
            memStream.Seek(0, SeekOrigin.End);
            memStream.Write(bytes, 0, length);
            //Reset to beginning
            memStream.Seek(0, SeekOrigin.Begin);
            while (RemainingBytes() >= 6)
            {
                int messageLen = reader.ReadInt32();
                if (RemainingBytes() >= messageLen + 2)
                {
                    OnReceivedMessage(reader.ReadBytes(messageLen + 2));
                }
                else
                {
                    //Back up the position six bytes: 4 bytes length + 2 bytes id
                    memStream.Position = memStream.Position - 4;
                    break;
                }
            }
            //Create a new stream with any leftover bytes
            byte[] leftover = reader.ReadBytes((int)RemainingBytes());
            memStream.SetLength(0);     //Clear
            memStream.Write(leftover, 0, leftover.Length);
        }

        /// <summary>
        /// 剩余的字节
        /// </summary>
        private long RemainingBytes()
        {
            return memStream.Length - memStream.Position;
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        /// <param name="ms"></param>
        void OnReceivedMessage(byte[] message)
        {
            ByteBuffer buffer = new ByteBuffer(message);
            //放在lua中解析pbid
            int pbId = buffer.ReadShort();
            if (pbId == 10000)
            {
                if (ConnectObserver.inReconnect)
                {
                    ConnectObserver.DispatchEvent(Protocal.Reconnected, new ByteBuffer());
                    if (reconnectAction != null) reconnectAction();
                }
                heartBeatService.resetTimeout();
                heartBeatService.pingUpdate();
            }
            else
            {
                buffer.SeekToBegin();
                ConnectObserver.DispatchEvent(Protocal.Message, buffer);
                heartBeatService.resetTimeout();
            }
        }

        /// <summary>
        /// 会话发送
        /// </summary>
        void SessionSend(byte[] bytes)
        {
            WriteMessage(bytes);
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        public void Close()
        {
            if (client != null)
            {
                if (client.Connected) client.Close();
                client = null;
            }
            loggedIn = false;
        }

        /// <summary>
        /// 发送连接请求
        /// </summary>
        public void SendConnect(string host, int port)
        {
            ConnectServer(host, port);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void SendMessage(ByteBuffer buffer)
        {
            SessionSend(buffer.ToBytes());
            buffer.Close();
        }

        public void ResetTimeout()
        {
            if (heartBeatService != null)
                heartBeatService.resetTimeout();
        }

        public void CheckHeartBeat()
        {
            if (heartBeatService != null)
                heartBeatService.checkHeartBeat(null);
        }

        public void UpdateHeartBeat2Server(float deltaTime)
        {
            if (heartBeatService != null && client != null && client.Connected)
            {
                if (heartBeatService.updateHeartBeat2Server(deltaTime))
                    ConnectObserver.DispatchEvent(Protocal.HeartBeat2Server, new ByteBuffer());
            }
        }

        [DllImport("__Internal")]
        private static extern string getIPv6(string host);

        private string GetIPv6(string host)
        {
#if UNITY_IOS && !UNITY_EDITOR
            string mIPv6 = getIPv6(host);
            return mIPv6;
#else
            return string.Format("{0}&&ipv4", host);
#endif
        }

        private void getIPType(String serverIp, out String newServerIp, out AddressFamily ipType)
        {
            ipType = AddressFamily.InterNetwork;
            newServerIp = serverIp;
            try
            {
                string ipv6 = GetIPv6(serverIp);
                if (!string.IsNullOrEmpty(ipv6))
                {
                    string[] strTemp = System.Text.RegularExpressions.Regex.Split(ipv6, "&&");
                    if (strTemp != null && strTemp.Length >= 2)
                    {
                        if (strTemp[1] == "ipv6")
                        {
                            newServerIp = strTemp[0];
                            Debug.LogFormat("use ipv6: {0}", newServerIp);
                            ipType = AddressFamily.InterNetworkV6;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("GetIPv6 error:");
                Debug.LogException(e);
            }
        }
    }
}
