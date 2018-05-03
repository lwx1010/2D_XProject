
namespace LuaFramework {
    public class Protocal {
        ///BUILD TABLE
        public const int Connect = 101;         //连接服务器
        public const int Exception = 102;       //异常掉线
        public const int Disconnect = 103;      //正常断线
        public const int Message = 104;         //接收消息
        public const int Reconnected = 105;     //重连服务器
        public const int BeginReconnect = 106;  //重连服务器失败
        public const int KickOut = 107;         //被踢出服务器
        public const int PingUpdate = 108;         //ping值
        public const int AccountConflict = 109; //被顶号
        public const int HeartBeat2Server = 110; //发送心跳给服务器
        public const int WeakMessage = 111;     //aoi等不重要的协议另外处理
    }
}