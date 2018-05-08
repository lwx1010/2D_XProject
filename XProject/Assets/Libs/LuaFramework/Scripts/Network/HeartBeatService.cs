using System;
using LuaFramework;

namespace Riverlake.Network
{
    /// <summary>
    /// 心跳管理器
    /// </summary>
    public sealed class HeartBeatService
    {
        int interval;
        public int timeout;
        System.Threading.Timer timer;
        DateTime lastTime;

        static readonly object m_lockObject = new object();

        private float heartBeartLeftTime = 0;

        private const float HEARTBEAT_DURATION = 150;

        public delegate void OnLostConnectAction();

        public OnLostConnectAction lostAction;

        public HeartBeatService(OnLostConnectAction lostAction)
        {
            this.interval = 5000;
            this.lostAction = lostAction;
        }

        internal void resetTimeout()
        {
            lock (m_lockObject)
            {
                this.timeout = 0;
                lastTime = DateTime.Now;
            }
        }

        internal void pingUpdate()
        {
            int protocal = Protocal.PingUpdate;
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteShort((ushort)protocal);
            ConnectObserver.DispatchEvent(protocal, buffer);
        }

        internal void checkHeartBeat(object source)
        {
            if (this.timer == null) return;
            lock (m_lockObject)
            {
                TimeSpan span = DateTime.Now - lastTime;
                timeout = (int)span.TotalMilliseconds;

                //check timeout
                if (timeout > interval * (ConnectObserver.powerSaveMode ? 60 : 6))
                {
                    if (lostAction != null) lostAction();
                    return;
                }
            }
        }

        internal void start()
        {
            //start check heartbeat
            this.timer = new System.Threading.Timer(new System.Threading.TimerCallback(checkHeartBeat), null, 0, interval);

            timeout = 0;
            heartBeartLeftTime = HEARTBEAT_DURATION;
            lastTime = DateTime.Now;
        }

        internal void stop()
        {
            if (this.timer != null)
                this.timer.Dispose();
            this.timer = null;
            heartBeartLeftTime = 0;
        }

        internal bool isstopped()
        {
            return this.timer == null;
        }

        internal bool updateHeartBeat2Server(float deltaTime)
        {
            if (heartBeartLeftTime > 0)
            {
                heartBeartLeftTime -= deltaTime;
            }
            if (heartBeartLeftTime <= 0)
            {
                heartBeartLeftTime = HEARTBEAT_DURATION;
                return true;
            }
            return false;
        }
    }
}
